using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using StarCore.Module.MathModule.Utils;
using StarCore.Utils;
using System.Drawing;

public static class Commands
{
    private static Plugin Instance = Plugin.Instance;
    private static Config config = Instance.Config;
    private static Config_Commands commands = Instance.Config.Commands;
    private static Dictionary<int, Building.BuilderData> BuilderData = Instance.BuilderData;

    public static void Load()
    {
        AddCommands(commands.Admin.BuildMode, BuildMode);
        AddCommands(commands.Admin.ManageBuilder, ManageBuilder);
        AddCommands(commands.Admin.ResetProperties, ResetProperties);
        AddCommands(commands.Building.BuildMenu, BuildMenu);
        AddCommands(commands.Building.BlockType, BlockType);
        AddCommands(commands.Building.BlockColor, BlockColor);
        AddCommands(commands.Building.CreateBlock, CreateBlock);
        AddCommands(commands.Building.DeleteBlock, DeleteBlock);
        AddCommands(commands.Building.RotateBlock, RotateBlock);
        AddCommands(commands.Building.PositionBlock, PositionBlock);
        AddCommands(commands.Building.SaveBlocks, SaveBlocks);
        AddCommands(commands.Building.Snapping, Snapping);
        AddCommands(commands.Building.Grid, Grid);
        AddCommands(commands.Building.Noclip, Noclip);
        AddCommands(commands.Building.Godmode, Godmode);
        AddCommands(commands.Building.TestBlock, TestBlock);
        AddCommands(commands.Building.ConvertBlock, ConvertBlock);
        AddCommands(commands.Building.CopyBlock, CopyBlock);
        AddCommands(commands.Building.LockBlock, LockBlock);
        AddCommands(commands.Building.LockAll, LockAll);
        Instance.AddCommand("css_clearblockwithinspawn", "移除出生点附近的板块", RemoveBlockWhinSpawn);
        Instance.AddCommand("css_enablebuildmode", "允许搭建", EnableBuildMode);
        Instance.AddCommand("css_disablebuildmode", "关闭搭建", DisableBuildMode);
    }
    private static void AddCommands(List<string> commands, Action<CCSPlayerController?> action)
    {
        foreach (var cmd in commands)
            Instance.AddCommand($"css_{cmd}", "", (player, command) => action(player));
    }
    private static void AddCommands(List<string> commands, Action<CCSPlayerController?, string> action)
    {
        foreach (var cmd in commands)
            Instance.AddCommand($"css_{cmd}", "", (player, command) => action(player, command.ArgByIndex(1)));
    }

    public static void Unload()
    {
        RemoveCommands(commands.Admin.BuildMode, BuildMode);
        RemoveCommands(commands.Admin.ManageBuilder, ManageBuilder);
        RemoveCommands(commands.Admin.ResetProperties, ResetProperties);
        RemoveCommands(commands.Building.BuildMenu, BuildMenu);
        RemoveCommands(commands.Building.BlockType, BlockType);
        RemoveCommands(commands.Building.BlockColor, BlockColor);
        RemoveCommands(commands.Building.CreateBlock, CreateBlock);
        RemoveCommands(commands.Building.DeleteBlock, DeleteBlock);
        RemoveCommands(commands.Building.RotateBlock, RotateBlock);
        RemoveCommands(commands.Building.PositionBlock, PositionBlock);
        RemoveCommands(commands.Building.SaveBlocks, SaveBlocks);
        RemoveCommands(commands.Building.Snapping, Snapping);
        RemoveCommands(commands.Building.Grid, Grid);
        RemoveCommands(commands.Building.Noclip, Noclip);
        RemoveCommands(commands.Building.Godmode, Godmode);
        RemoveCommands(commands.Building.TestBlock, TestBlock);
        RemoveCommands(commands.Building.ConvertBlock, ConvertBlock);
        RemoveCommands(commands.Building.CopyBlock, CopyBlock);
        RemoveCommands(commands.Building.LockBlock, LockBlock);
        RemoveCommands(commands.Building.LockAll, LockAll);
    }

    private static void RemoveBlockWhinSpawn(CCSPlayerController? player, CommandInfo cmdInfo)
    {
        var tSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").ToList();
        var ctSPawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").ToList();
        var spawnPoints = tSpawns.Concat(ctSPawns).ToList();
        foreach (var spawnPoint in spawnPoints)
        {
            var spawnPointPos = spawnPoint.AbsOrigin!.Vec3();
            foreach (var prop in Utilities.GetAllEntities().Where(e => 
            e != null && e.IsValid
            && !string.IsNullOrWhiteSpace(e.Entity!.Name)
            && e.DesignerName.Contains("prop_physics_override") 
            && e.Entity!.Name.StartsWith("blockmaker")))
            {
                var currentProp = prop.As<CBaseProp>();
                var currentPropPos = currentProp.AbsOrigin!.Vec3();
                var distance = StarMath.DistanceXYZ(spawnPointPos, currentPropPos);
                if (distance < 400)
                {
                    Blocks.DeleteBlock(currentProp);
                }
                if (currentProp.Render.A != 255)
                {
                    Blocks.DeleteBlock(currentProp);
                }
            }
        }
    }

    private static void EnableBuildMode(CCSPlayerController? player, CommandInfo cmdInfo)
    {
        Instance.buildMode = true;
    }

    private static void DisableBuildMode(CCSPlayerController? player, CommandInfo cmdInfo)
    {
        Instance.buildMode = false;
    }

    private static void RemoveCommands(List<string> commands, Action<CCSPlayerController?> action)
    {
        foreach (var cmd in commands)
            Instance.RemoveCommand($"css_{cmd}", (player, command) => action(player));
    }
    private static void RemoveCommands(List<string> commands, Action<CCSPlayerController?, string> action)
    {
        foreach (var cmd in commands)
            Instance.RemoveCommand($"css_{cmd}", (player, command) => action(player, command.ArgByIndex(1)));
    }

    private static bool AllowedCommand(CCSPlayerController? player)
    {
        if (player == null || player.NotValid())
            return false;

        if (!Utils.BuildMode(player))
            return false;

        return true;
    }

    private static void ToggleCommand(CCSPlayerController player, ref bool commandStatus, string commandName)
    {
        commandStatus = !commandStatus;

        string status = commandStatus ? "ON" : "OFF";
        char color = commandStatus ? ChatColors.Green : ChatColors.Red;

        Utils.PrintToChat(player, $"{commandName}: {color}{status}");
    }

    public static void BuildMode(CCSPlayerController? player)
    {
        if (player == null || player.NotValid())
            return;

        if (!Utils.HasPermission(player))
        {
            Utils.PrintToChatAll($"{ChatColors.Red}无权限");
            return;
        }

        if (!Instance.buildMode)
        {
            Instance.buildMode = true;
            foreach (var target in Utilities.GetPlayers().Where(p => !p.IsBot))
            {
                if (Utils.HasPermission(target) || Files.Builders.steamids.Contains(target.SteamID.ToString()))
                {
                    BuilderData[player.Slot] = new Building.BuilderData { BlockType = Blocks.Models.Data.Platform.Title };
                    Building.PlayerHolds[target] = new Building.BuildData();
                }
            }
        }
        else
        {
            Instance.buildMode = false;
            BuilderData.Clear();
            Building.PlayerHolds.Clear();
        }

        string status = Instance.buildMode ? "开启" : "关闭";
        char color = Instance.buildMode ? ChatColors.Green : ChatColors.Red;

        Utils.PrintToChatAll($"建造模式: {color}{status} {ChatColors.Grey}- {ChatColors.LightPurple}{player.PlayerName}");
    }

    public static void ManageBuilder(CCSPlayerController? player, string input)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (!Utils.HasPermission(player))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}无权限");
            return;
        }

        var targetPlayer = Utilities.GetPlayers()
            .FirstOrDefault(target => target.SteamID.ToString() == input);

        if (string.IsNullOrEmpty(input) || targetPlayer == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}找不到玩家");
            return;
        }

        bool isBuilder = BuilderData.TryGetValue(targetPlayer.Slot, out var builderData);

        if (isBuilder)
            BuilderData.Remove(targetPlayer.Slot);

        else BuilderData[player.Slot] = new Building.BuilderData { BlockType = Blocks.Models.Data.Platform.Title };

        var action = isBuilder ? "removed" : "granted";
        var color = isBuilder ? ChatColors.Red : ChatColors.Green;

        Utils.PrintToChat(targetPlayer, $"{ChatColors.LightPurple}{player.PlayerName} {color}{action} 给与了你建造权限");
        Utils.PrintToChat(player, $"{color}你给了 {action} {ChatColors.LightPurple}{targetPlayer.PlayerName} {color}建造权限");

        var builders = Files.Builders.steamids;
        string steamId = targetPlayer.SteamID.ToString();

        if (isBuilder && builders.Contains(steamId))
            builders.Remove(steamId);

        else
        {
            if (!builders.Contains(steamId))
                builders.Add(steamId);
        }

        Files.Builders.Save(builders);
    }

    public static void BuildMenu(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (!Utils.BuildMode(player))
            return;

        Menu.Open(player, "Block Maker");
    }

    public static void BlockType(CCSPlayerController? player, string selectType)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (string.IsNullOrEmpty(selectType))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}未指定板块类型");
            return;
        }

        if (string.Equals("Teleport", selectType, StringComparison.OrdinalIgnoreCase))
        {
            BuilderData[player.Slot].BlockType = "Teleport";
            Utils.PrintToChat(player, $"选择种类: {ChatColors.White}传送");
            return;
        }

        var blockModels = Blocks.Models.Data;
        foreach (var model in blockModels.GetAllBlocks())
        {
            if (string.Equals(model.Title, selectType, StringComparison.OrdinalIgnoreCase))
            {
                BuilderData[player.Slot].BlockType = model.Title;
                Utils.PrintToChat(player, $"选择种类: {ChatColors.White}{model.Title}");
                return;
            }
        }

        Utils.PrintToChat(player, $"{ChatColors.Red}查询不到 {ChatColors.White}{selectType}");
    }

    public static void BlockColor(CCSPlayerController? player, string selectColor = "None")
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (string.IsNullOrEmpty(selectColor))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}未指定颜色");
            return;
        }

        foreach (var color in Utils.ColorMapping.Keys)
        {
            if (string.Equals(color, selectColor, StringComparison.OrdinalIgnoreCase))
            {
                BuilderData[player.Slot].BlockColor = color;
                Utils.PrintToChat(player, $"选择颜色: {ChatColors.White}{color}");

                Blocks.RenderColor(player);

                return;
            }
        }

        Utils.PrintToChat(player, $"{ChatColors.Red}找不到匹配的颜色");
    }

    public static void CreateBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Create(player);
    }

    public static void DeleteBlock(CCSPlayerController? player)
    {
        if (player == null)
            return;

        Blocks.Delete(player);
    }

    public static void RotateBlock(CCSPlayerController? player, string rotation)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Position(player, rotation, true);
    }

    public static void PositionBlock(CCSPlayerController? player, string position)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Position(player, position, false);
    }

    public static void SaveBlocks(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (Utils.GetPlacedBlocksCount() <= 0)
        {
             Utils.PrintToChatAll($"{ChatColors.Red}无可保存板块");
            return;
        }

        Files.EntitiesData.SaveDefault();
    }

    public static void Snapping(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        ToggleCommand(player, ref BuilderData[player.Slot].Snapping, "Block Snapping");
    }

    public static void Grid(CCSPlayerController? player, string grid)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (string.IsNullOrEmpty(grid))
        {
            ToggleCommand(player, ref BuilderData[player.Slot].Grid, "Block Grid");
            return;
        }

        BuilderData[player.Slot].GridValue = float.Parse(grid);

        Utils.PrintToChat(player, $"网格: {ChatColors.White}{grid} 单位");
    }

    public static void Noclip(CCSPlayerController? player)
    {
        if (player == null || player.NotValid())
            return;

        if (!Utils.BuildMode(player))
            return;

        ToggleCommand(player, ref BuilderData[player.Slot].Noclip, "Noclip");

        if (BuilderData[player.Slot].Noclip)
        {
            player.Pawn.Value!.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            Schema.SetSchemaValue(player.Pawn.Value!.Handle, "CBaseEntity", "m_nActualMoveType", 8); // noclip
            Utilities.SetStateChanged(player.Pawn.Value!, "CBaseEntity", "m_MoveType");
        }

        else if (!BuilderData[player.Slot].Noclip)
        {
            player.Pawn.Value!.MoveType = MoveType_t.MOVETYPE_WALK;
            Schema.SetSchemaValue(player!.Pawn.Value!.Handle, "CBaseEntity", "m_nActualMoveType", 2); // walk
            Utilities.SetStateChanged(player!.Pawn.Value!, "CBaseEntity", "m_MoveType");
        }
    }

    public static void Godmode(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        ToggleCommand(player, ref BuilderData[player.Slot].Godmode, "Godmode");

        if (BuilderData[player.Slot].Godmode)
            player.Pawn()!.TakesDamage = false;

        else player.Pawn()!.TakesDamage = true;
    }

    public static void TestBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Test(player);
    }

    public static void ClearBlocks(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Delete(player, true);

        Utils.PlaySoundAll(config.Sounds.Building.Delete);
        Utils.PrintToChatAll($"{ChatColors.Red}板块被 {ChatColors.LightPurple}{player.PlayerName} 清空");
    }

    public static void ConvertBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Convert(player);
    }

    public static void CopyBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;
        Blocks.Copy(player);
    }

    public static void LockBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Lock(player);
    }

    public static void LockAll(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.LockAll(player);
    }

    public static void TransparencyBlock(CCSPlayerController? player, string transparency = "100%")
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Transparency(player, transparency);
    }

    public static void EffectBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;
        
        Blocks.ChangeEffect(player);
    }

    public static void TeamBlock(CCSPlayerController? player, string team = "Both")
    {
        if (player == null || !AllowedCommand(player))
            return;

        var entity = player.GetBlockAim();

        if (entity == null || entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Blocks.Entities.TryGetValue(entity, out var block))
        {
            if (Utils.BlockLocked(player, block))
                return;

            Blocks.Entities[entity].Team = team;
            Utils.PrintToChat(player, $"将板块阵营变更为 {ChatColors.White}{team}");
        }
    }

    public static void Pole(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        ToggleCommand(player, ref BuilderData[player.Slot].BlockPole, "Pole");
    }

    public static void Properties(CCSPlayerController? player, string type, string input)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.ChangeProperties(player, type, input);
    }

    public static void LightSettings(CCSPlayerController? player, string type, string input)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Lights.Settings(player, type, input);
    }

    public static void ResetProperties(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (!Utils.HasPermission(player))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}无权");
            return;
        }

        foreach (var block in Blocks.Entities.Values)
        {
            if (Blocks.Properties.BlockProperties.TryGetValue(block.Type.Split('.')[0], out var defaultProperties))
            {
                block.Properties = new Blocks.Property
                {
                    Cooldown = defaultProperties.Cooldown,
                    Value = defaultProperties.Value,
                    Duration = defaultProperties.Duration,
                    OnTop = defaultProperties.OnTop,
                    Locked = defaultProperties.Locked,
                    Builder = block.Properties.Builder,
                };
            }
            else Utils.PrintToChatAll($"{ChatColors.Red}找不到 {ChatColors.White}{block.Type} {ChatColors.Red}的基础属性");
        }
        Utils.PrintToChatAll($"{ChatColors.Red}所有板块已被重置!");
    }
}