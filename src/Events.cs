using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
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
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);

        RegisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        RegisterEventHandler<EventRoundStart>(EventRoundStart);
        RegisterEventHandler<EventRoundEnd>(EventRoundEnd);
        RegisterEventHandler<EventPlayerDeath>(EventPlayerDeath);

        AddCommandListener("say", OnCommandSay, HookMode.Pre);
        AddCommandListener("say_team", OnCommandSay, HookMode.Pre);

        HookEntityOutput("trigger_multiple", "OnStartTouch", OnStartTouch, HookMode.Pre);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
    }

    void UnregisterEvents()
    {
        RemoveListener<Listeners.OnTick>(Blocks.OnTick);
        RemoveListener<Listeners.OnMapStart>(OnMapStart);
        RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
        RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);

        DeregisterEventHandler<EventPlayerConnectFull>(EventPlayerConnectFull);
        DeregisterEventHandler<EventRoundStart>(EventRoundStart);
        DeregisterEventHandler<EventRoundEnd>(EventRoundEnd);
        DeregisterEventHandler<EventPlayerDeath>(EventPlayerDeath);

        RemoveCommandListener("say", OnCommandSay, HookMode.Pre);
        RemoveCommandListener("say_team", OnCommandSay, HookMode.Pre);

        UnhookEntityOutput("trigger_multiple", "OnStartTouch", OnStartTouch, HookMode.Pre);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
    }

    void OnMapStart(string mapname)
    {
        Files.mapsFolder = Path.Combine(ModuleDirectory, "maps", Server.MapName);
        Directory.CreateDirectory(Files.mapsFolder);

        if (Config.Settings.Building.AutoSave)
        {
            AddTimer(Config.Settings.Building.SaveTime, () => {
                Utils.PrintToChatAll("Auto-Saving Blocks");
                Files.PropsData.Save();
            }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
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

    void OnMapEnd()
    {
        Blocks.Clear();
    }

    void OnServerPrecacheResources(ResourceManifest manifest)
    {
        var blockProperties = typeof(BlockModels).GetProperties();

        foreach (var property in blockProperties)
        {
            var models = (BlockModel)property.GetValue(Files.Models.Props)!;

            if (models != null)
            {
                if (!string.IsNullOrEmpty(models.Block))
                    manifest.AddResource(models.Block);

                if (!string.IsNullOrEmpty(models.Pole))
                    manifest.AddResource(models.Pole);
            }
        }

        manifest.AddResource(Config.Settings.Teleports.EntryModel);
        manifest.AddResource(Config.Settings.Teleports.ExitModel);

        manifest.AddResource(Config.Settings.Blocks.CamouflageT);
        manifest.AddResource(Config.Settings.Blocks.CamouflageCT);

        manifest.AddResource(Config.Settings.Blocks.FireParticle);
    }

    HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || player.NotValid())
            return HookResult.Continue;

        playerData[player.Slot] = new PlayerData();

        if (buildMode)
        {
            Files.Builders.Load();

            if (Utils.HasPermission(player) || Files.Builders.steamids.Contains(player.SteamID.ToString()))
                playerData[player.Slot].Builder = true;
        }

        return HookResult.Continue;
    }

    HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        Blocks.Clear();
        Files.PropsData.Load();

        return HookResult.Continue;
    }

    HookResult EventRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (buildMode && Config.Settings.Building.AutoSave)
            Files.PropsData.Save();

        return HookResult.Continue;
    }

    HookResult EventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || player.NotValid())
            return HookResult.Continue;

        if (buildMode)
            AddTimer(1.0f, player!.Respawn);

        if (Blocks.CooldownsTimers.TryGetValue(player.Slot, out var playerTimers))
        {
            foreach (var timer in playerTimers)
                timer.Kill();

            Blocks.CooldownsTimers[player.Slot].Clear();
        }

        return HookResult.Continue;
    }

    HookResult OnCommandSay(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || player.NotValid())
            return HookResult.Continue;

        if (playerData.ContainsKey(player.Slot))
        {
            var type = playerData[player.Slot].PropertyType;

            if (!string.IsNullOrEmpty(type))
            {
                var input = info.ArgString.Replace("\"", "");

                Commands.Properties(player, type, input);
                return HookResult.Handled;
            }
        }
    
        return HookResult.Continue;
    }

    HookResult OnStartTouch(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        if (activator.DesignerName != "player") return HookResult.Continue;

        var pawn = activator.As<CCSPlayerPawn>();
        if (!pawn.IsValid) return HookResult.Continue;
        if (!pawn.Controller.IsValid || pawn.Controller.Value is null) return HookResult.Continue;

        var player = pawn.Controller.Value.As<CCSPlayerController>();
        if (player.IsBot) return HookResult.Continue;

        if (Blocks.Triggers.TryGetValue(caller, out CBaseProp? block))
        {
            var teleport = Blocks.Teleports.FirstOrDefault(pair => pair.Entry.Entity == block || pair.Exit.Entity == block);

            if (teleport != null)
            {
                if (teleport.Entry == null || teleport.Exit == null)
                    return HookResult.Continue;

                if (block.Entity!.Name.Contains("Entry"))
                {
                    pawn.Teleport(
                        teleport.Exit.Entity.AbsOrigin,
                        Config.Settings.Teleports.ForceAngles
                        ? teleport.Exit.Entity.AbsRotation
                        : pawn.EyeAngles,
                        pawn.AbsVelocity
                    );

                    if (!String.IsNullOrEmpty(Config.Sounds.Blocks.Teleport))
                        player.ExecuteClientCommand($"play {Config.Sounds.Blocks.Teleport}");
                }

                return HookResult.Continue;
            }
            else
            {
                var blockData = Blocks.Props[block];

                if (blockData.Properties.OnTop)
                {
                    var playerMaxs = pawn.Collision.Maxs * 2;
                    var blockMaxs = block.Collision.Maxs * Utils.GetSize(blockData.Size) * 2;

                    if (!VectorUtils.IsWithinBounds(block.AbsOrigin!, pawn.AbsOrigin!, blockMaxs, playerMaxs))
                        return HookResult.Continue;
                }

                if (blockData.Team == "T" && player.Team == CsTeam.Terrorist ||
                    blockData.Team == "CT" && player.Team == CsTeam.CounterTerrorist ||
                    blockData.Team == "Both"
                )
                {
                    Blocks.Actions(player, block);
                }
            }
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

        foreach(var block in Blocks.Props.Where(b =>
            b.Value.Name == Files.Models.Props.NoFallDmg.Title ||
            b.Value.Name == Files.Models.Props.Trampoline.Title)
        )
        {
            var player = entity.As<CCSPlayerPawn>();

            var playerPos = new Vector(player.AbsOrigin!.X, player.AbsOrigin.Y, player.AbsOrigin.Z);
            var blockPos = new Vector(block.Key.AbsOrigin!.X, block.Key.AbsOrigin.Y, block.Key.AbsOrigin.Z);

            var playerMaxs = player.Collision.Maxs * 2;
            var blockMaxs = block.Key.Collision!.Maxs * Utils.GetSize(block.Value.Size) * 2;

            if (VectorUtils.IsWithinBounds(blockPos, playerPos, blockMaxs, playerMaxs))
                return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}