using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;

public partial class Plugin : BasePlugin, IPluginConfig<Config>
{
    public void RegisterEvents()
    {
        RegisterListener<OnTick>(Blocks.OnTick);
        RegisterListener<OnMapStart>(OnMapStart);
        RegisterListener<OnServerPrecacheResources>(OnServerPrecacheResources);

        RegisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        RegisterEventHandler<EventRoundStart>(EventRoundStart);
        RegisterEventHandler<EventRoundEnd>(EventRoundEnd);
        RegisterEventHandler<EventPlayerDeath>(EventPlayerDeath);

        HookEntityOutput("trigger_multiple", "OnStartTouch", OnStartTouch, HookMode.Pre);
    }

    public void UnregisterEvents()
    {
        RemoveListener<OnTick>(Blocks.OnTick);
        RemoveListener<OnMapStart>(OnMapStart);
        RemoveListener<OnServerPrecacheResources>(OnServerPrecacheResources);

        DeregisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        DeregisterEventHandler<EventRoundStart>(EventRoundStart);
        DeregisterEventHandler<EventRoundEnd>(EventRoundEnd);
        DeregisterEventHandler<EventPlayerDeath>(EventPlayerDeath);

        UnhookEntityOutput("trigger_multiple", "OnStartTouch", OnStartTouch, HookMode.Pre);
    }

    private HookResult OnStartTouch(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        if (activator.DesignerName != "player") return HookResult.Continue;

        var pawn = activator.As<CCSPlayerPawn>();

        if (!pawn.IsValid) return HookResult.Continue;
        if (!pawn.Controller.IsValid || pawn.Controller.Value is null) return HookResult.Continue;

        var player = pawn.Controller.Value.As<CCSPlayerController>();

        if (player.IsBot) return HookResult.Continue;

        if (Blocks.BlockTriggers.TryGetValue(caller, out CEntityInstance? block))
            Blocks.Actions(player, block.As<CBaseEntity>());

        return HookResult.Continue;
    }

    public void OnServerPrecacheResources(ResourceManifest manifest)
    {
        var blockProperties = typeof(BlockModels).GetProperties();

        foreach (var property in blockProperties)
        {
            var block = (BlockSizes)property.GetValue(BlockModels)!;

            if (block != null)
            {
                if (!string.IsNullOrEmpty(block.Small))
                    manifest.AddResource(block.Small);

                if (!string.IsNullOrEmpty(block.Medium))
                    manifest.AddResource(block.Medium);

                if (!string.IsNullOrEmpty(block.Large))
                    manifest.AddResource(block.Large);

                if (!string.IsNullOrEmpty(block.Pole))
                    manifest.AddResource(block.Pole);
            }
        }

        manifest.AddResource(Config.Settings.Blocks.Camouflage.ModelT);
        manifest.AddResource(Config.Settings.Blocks.Camouflage.ModelCT);
        manifest.AddResource("particles/burning_fx/env_fire_medium.vpcf");
    }

    public void OnMapStart(string mapname)
    {
        Blocks.savedPath = Path.Combine(blocksFolder, $"{GetMapName()}.json");

        if (Config.Settings.Building.AutoSave)
        {
            AddTimer(Config.Settings.Building.SaveTime, () => {
                PrintToChatAll("Auto-Saving Blocks");
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
            if (HasPermission(player))
                playerData[player.Slot].Builder = true;
        }

        return HookResult.Continue;
    }

    HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        Timers.Clear();

        Blocks.UsedBlocks.Clear();
        Blocks.PlayerHolds.Clear();
        Blocks.blocksCooldown.Clear();
        Blocks.Timers.Clear();

        Blocks.Spawn();

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

        if (Blocks.Timers.TryGetValue(player!, out var playerTimers))
        {
            foreach (var timer in playerTimers)
                timer.Kill();

            Blocks.Timers[player!].Clear();
        }

        return HookResult.Continue;
    }
}