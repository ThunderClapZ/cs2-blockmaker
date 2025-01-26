using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.InteropServices;

public partial class Plugin : BasePlugin, IPluginConfig<Config>
{
    void RegisterEvents()
    {
        RegisterListener<Listeners.OnTick>(Blocks.OnTick);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);

        RegisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        RegisterEventHandler<EventRoundStart>(EventRoundStart);
        RegisterEventHandler<EventRoundEnd>(EventRoundEnd);
        RegisterEventHandler<EventPlayerDeath>(EventPlayerDeath);

        HookEntityOutput("trigger_multiple", "OnStartTouch", OnStartTouch, HookMode.Pre);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
    }

    void UnregisterEvents()
    {
        RemoveListener<Listeners.OnTick>(Blocks.OnTick);
        RemoveListener<Listeners.OnMapStart>(OnMapStart);
        RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);

        DeregisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        DeregisterEventHandler<EventRoundStart>(EventRoundStart);
        DeregisterEventHandler<EventRoundEnd>(EventRoundEnd);
        DeregisterEventHandler<EventPlayerDeath>(EventPlayerDeath);

        UnhookEntityOutput("trigger_multiple", "OnStartTouch", OnStartTouch, HookMode.Pre);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    HookResult OnStartTouch(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        if (activator.DesignerName != "player") return HookResult.Continue;

        var pawn = activator.As<CCSPlayerPawn>();

        if (!pawn.IsValid) return HookResult.Continue;
        if (!pawn.Controller.IsValid || pawn.Controller.Value is null) return HookResult.Continue;

        var player = pawn.Controller.Value.As<CCSPlayerController>();

        if (player.IsBot) return HookResult.Continue;

        if (Blocks.BlockTriggers.TryGetValue(caller, out CEntityInstance? blockEntity))
        {
            var block = Blocks.BlocksEntities[blockEntity.As<CBaseEntity>()];

            if (block.Team == "T" && player.Team == CsTeam.Terrorist ||
                block.Team == "CT" && player.Team == CsTeam.CounterTerrorist ||
                block.Team == "Both")
            {
                Blocks.Actions(player, block.Entity);
            }
        }

        return HookResult.Continue;
    }

    void OnServerPrecacheResources(ResourceManifest manifest)
    {
        var blockProperties = typeof(BlockModels).GetProperties();

        foreach (var property in blockProperties)
        {
            var models = (BlockSizes)property.GetValue(BlockModels)!;

            if (models != null)
            {
                if (!string.IsNullOrEmpty(models.Block))
                    manifest.AddResource(models.Block);

                if (!string.IsNullOrEmpty(models.Pole))
                    manifest.AddResource(models.Pole);
            }
        }

        manifest.AddResource(Config.Settings.Blocks.Camouflage.ModelT);
        manifest.AddResource(Config.Settings.Blocks.Camouflage.ModelCT);
        manifest.AddResource("particles/burning_fx/env_fire_medium.vpcf");
    }

    void OnMapStart(string mapname)
    {
        Blocks.savedPath = Path.Combine(blocksFolder, $"{Utils.GetMapName()}.json");

        if (Config.Settings.Building.AutoSave)
        {
            AddTimer(Config.Settings.Building.SaveTime, () => {
                Utils.PrintToChatAll("Auto-Saving Blocks");
                Blocks.Save();
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }

        if (Config.Settings.Building.BuildModeConfig)
        {
            string[] commands =
                {
                    "sv_cheats 1", "mp_join_grace_time 3600", "mp_timelimit 60",
                    "mp_roundtime 60", "mp_freezetime 0", "mp_warmuptime 0", "mp_maxrounds 99"
                };

            foreach (string command in commands)
                Server.ExecuteCommand(command);
        }
    }

    HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || player.NotValid())
            return HookResult.Continue;

        playerData[player.Slot] = new PlayerData();

        if (buildMode)
        {
            if (Utils.HasPermission(player))
                playerData[player.Slot].Builder = true;
        }

        return HookResult.Continue;
    }

    HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        Timers.Clear();

        Blocks.PlayerHolds.Clear();
        Blocks.PlayerCooldowns.Clear();
        Blocks.CooldownsTimers.Clear();
        Blocks.BlocksEntities.Clear();

        Blocks.Spawn();

        Blocks.nuked = false;

        return HookResult.Continue;
    }

    HookResult EventRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (buildMode && Config.Settings.Building.AutoSave)
            Blocks.Save();

        return HookResult.Continue;
    }

    HookResult EventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (@event == null || player.NotValid())
            return HookResult.Continue;

        if (buildMode)
            AddTimer(1.0f, player!.Respawn);

        if (Blocks.CooldownsTimers.TryGetValue(player!, out var playerTimers))
        {
            foreach (var timer in playerTimers)
                timer.Kill();

            Blocks.CooldownsTimers[player!].Clear();
        }

        return HookResult.Continue;
    }

    HookResult OnTakeDamage(DynamicHook hook)
    {
        var entity = hook.GetParam<CEntityInstance>(0);
        var info = hook.GetParam<CTakeDamageInfo>(1);

        if (!entity.IsValid || !info.Attacker.IsValid)
            return HookResult.Continue;

        if (entity.DesignerName == "player" && info.Attacker.Value!.DesignerName == "player")
            return HookResult.Continue;

        foreach(var block in Blocks.BlocksEntities.Where(b =>
            b.Value.Name == BlockModels.NoFallDmg.Title ||
            b.Value.Name == BlockModels.Trampoline.Title)
        )
        {
            var player = entity.As<CCSPlayerPawn>();

            var playerPos = new Vector(player.AbsOrigin!.X, player.AbsOrigin.Y, player.AbsOrigin.Z);
            var blockPos = new Vector(block.Key.AbsOrigin!.X, block.Key.AbsOrigin.Y, block.Key.AbsOrigin.Z);

            var playerMaxs = VectorUtils.GetBlockSizeMax(player);
            var blockMaxs = VectorUtils.GetBlockSizeMax(block.Key) * Utils.GetSize(block.Value.Size);

            if (VectorUtils.IsWithinBounds(blockPos, playerPos, blockMaxs, playerMaxs))
                return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}