using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

public partial class Plugin
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

        HookEntityOutput("trigger_multiple", "OnStartTouch", trigger_multiple, HookMode.Pre);

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);

        Transmit.Load();
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

        UnhookEntityOutput("trigger_multiple", "OnStartTouch", trigger_multiple, HookMode.Pre);

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);

        Transmit.Unload();
    }

    public Timer? AutoSaveTimer;
    void OnMapStart(string mapname)
    {
        Files.mapsFolder = Path.Combine(ModuleDirectory, "maps", Server.MapName);
        Directory.CreateDirectory(Files.mapsFolder);

        if (Config.Settings.Building.AutoSave.Enable)
        {
            AutoSaveTimer?.Kill();
            AutoSaveTimer = AddTimer(Config.Settings.Building.AutoSave.Timer, () => {
                if (!buildMode) return;
                Files.PropsData.Save(true);
            }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
        }

        if (Config.Settings.Building.BuildMode.Config)
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
        foreach (var model in Files.Models.Props.GetAllBlocks())
        {
            manifest.AddResource(model.Block);
            manifest.AddResource(model.Pole);
        }

        manifest.AddResource(Config.Sounds.SoundEvents);

        manifest.AddResource(Config.Settings.Teleports.Entry.Model);
        manifest.AddResource(Config.Settings.Teleports.Exit.Model);

        manifest.AddResource(Config.Settings.Blocks.CamouflageT);
        manifest.AddResource(Config.Settings.Blocks.CamouflageCT);

        manifest.AddResource(Config.Settings.Blocks.FireParticle);
    }

    HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || player.NotValid())
            return HookResult.Continue;

        if (buildMode)
        {
            playerData[player.Slot] = new PlayerData { BlockType = Files.Models.Props.Platform.Title };

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
        if (buildMode && Config.Settings.Building.AutoSave.Enable)
            Files.PropsData.Save();

        return HookResult.Continue;
    }

    HookResult EventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || player.NotValid())
            return HookResult.Continue;

        if (Blocks.PlayerCooldowns.TryGetValue(player.Slot, out var playerCooldowns))
            playerCooldowns.Clear();

        if (Blocks.CooldownsTimers.TryGetValue(player.Slot, out var playerTimers))
        {
            foreach (var timer in playerTimers)
                timer.Kill();

            playerTimers.Clear();
        }

        if (Blocks.HiddenPlayers.TryGetValue(player, out var hiddenPlayer))
            Blocks.HiddenPlayers.Remove(player);

        return HookResult.Continue;
    }

    HookResult OnCommandSay(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || player.NotValid())
            return HookResult.Continue;

        if (playerData.ContainsKey(player.Slot))
        {
            var pData = playerData[player.Slot];
            var type = pData.ChatInput;

            if (!string.IsNullOrEmpty(type))
            {
                var input = info.ArgString.Replace("\"", "");

                if (!float.TryParse(input, out float number) || (number <= 0 && type != "Snap"))
                {
                    Utils.PrintToChat(player, $"{ChatColors.Red}Invalid input value: {ChatColors.White}{input}");
                    return HookResult.Handled;
                }

                switch (type)
                {
                    case "Grid":
                        pData.GridValue = number;
                        Utils.PrintToChat(player, $"Grid Value: {ChatColors.White}{number}");
                        break;
                    case "Snap":
                        pData.SnapValue = number;
                        Utils.PrintToChat(player, $"Snap Value: {ChatColors.White}{number}");
                        break;
                    case "Rotation":
                        pData.RotationValue = number;
                        Utils.PrintToChat(player, $"Rotation Value: {ChatColors.White}{number}");
                        break;
                    case "Position":
                        pData.PositionValue = number;
                        Utils.PrintToChat(player, $"Position Value: {ChatColors.White}{number}");
                        break;
                    case "Reset":
                    default:
                        Commands.Properties(player, type, input);
                        break;
                }

                pData.ChatInput = "";

                return HookResult.Handled;
            }
        }
    
        return HookResult.Continue;
    }

    HookResult trigger_multiple(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        if (activator.DesignerName != "player")
            return HookResult.Continue;

        var pawn = activator.As<CCSPlayerPawn>();
        if (pawn == null || !pawn.IsValid)
            return HookResult.Continue;

        var player = pawn.OriginalController?.Value?.As<CCSPlayerController>();
        if (player == null || player.IsBot)
            return HookResult.Continue;

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

                    player.EmitSound(Config.Sounds.Blocks.Teleport);
                }

                return HookResult.Continue;
            }
            else
            {
                if (buildMode)
                {
                    foreach (var kvp in Blocks.PlayerHolds)
                        if (kvp.Value.block == block)
                            return HookResult.Continue;
                }

                var blockData = Blocks.Props[block];

                if (blockData.Properties.OnTop)
                {
                    var playerMaxs = pawn.Collision.Maxs * 2;
                    var blockMaxs = block.Collision.Maxs * Utils.GetSize(blockData.Size) * 2;

                    if (!VectorUtils.IsTopOnly(block.AbsOrigin!, pawn.AbsOrigin!, blockMaxs, playerMaxs, block.AbsRotation!))
                        return HookResult.Continue;
                }

                if (blockData.Team == "T" && player.Team == CsTeam.Terrorist ||
                    blockData.Team == "CT" && player.Team == CsTeam.CounterTerrorist ||
                    blockData.Team == "Both" || string.IsNullOrEmpty(blockData.Team)
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

        if (entity.DesignerName == "player" && info.Attacker.Value?.DesignerName == "player")
            return HookResult.Continue;

        var props = Files.Models.Props;
        string NoFallDmg = props.NoFallDmg.Title;
        string Trampoline = props.Trampoline.Title;

        foreach (var blocktarget in Blocks.Props.Where(x => x.Value.Type.Equals(NoFallDmg) || x.Value.Type.Equals(Trampoline)))
        {
            var player = entity.As<CCSPlayerPawn>();
            var block = blocktarget.Key;

            if (player.AbsOrigin == null || block.AbsOrigin == null)
                return HookResult.Continue;

            var playerMaxs = player.Collision.Maxs * 2;
            var blockMaxs = block.Collision!.Maxs * Utils.GetSize(blocktarget.Value.Size) * 2;

            if (VectorUtils.IsWithinBounds(block.AbsOrigin, player.AbsOrigin, blockMaxs, playerMaxs))
            {
                if (blocktarget.Value.Properties.OnTop)
                {
                    if (VectorUtils.IsTopOnly(block.AbsOrigin!, player.AbsOrigin!, blockMaxs, playerMaxs, block.AbsRotation!))
                        return HookResult.Handled;
                }
                else return HookResult.Handled;
            }
        }

        return HookResult.Continue;
    }
}