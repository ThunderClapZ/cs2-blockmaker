using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

public partial class Plugin : BasePlugin, IPluginConfig<Config>
{
    void AddCommands()
    {
        var commands = Config.Commands;

        foreach (var cmd in commands.Admin.BuildMode.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_BuildMode(player));

        foreach (var cmd in commands.Admin.ManageBuilder.Split(','))
            AddCommand($"css_{cmd}", "", Command_ManageBuilder);

        foreach (var cmd in commands.Building.BuildMenu.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_BuildMenu(player));

        foreach (var cmd in commands.Building.BlockType.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_BlockType(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.BlockColor.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_BlockColor(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.CreateBlock.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_CreateBlock(player));

        foreach (var cmd in commands.Building.DeleteBlock.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_DeleteBlock(player));

        foreach (var cmd in commands.Building.RotateBlock.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_RotateBlock(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.SaveBlocks.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_SaveBlocks(player));

        foreach (var cmd in commands.Building.Snapping.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_Snapping(player));

        foreach (var cmd in commands.Building.Grid.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_Grid(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.Noclip.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_Noclip(player));

        foreach (var cmd in commands.Building.Godmode.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_Godmode(player));

        foreach (var cmd in commands.Building.TestBlock.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_Testblock(player));

        foreach (var cmd in commands.Building.ConvertBlock.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_ConvertBlock(player));

        foreach (var cmd in commands.Building.CopyBlock.Split(','))
            AddCommand($"css_{cmd}", "", (player, command) => Command_CopyBlock(player));
    }

    void RemoveCommands()
    {
        var commands = Config.Commands;

        foreach (var cmd in commands.Admin.BuildMode.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_BuildMode(player));

        foreach (var cmd in commands.Admin.ManageBuilder.Split(','))
            RemoveCommand($"css_{cmd}", Command_ManageBuilder);

        foreach (var cmd in commands.Building.BuildMenu.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_BuildMenu(player));

        foreach (var cmd in commands.Building.BlockType.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_BlockType(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.BlockColor.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_BlockColor(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.CreateBlock.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_CreateBlock(player));

        foreach (var cmd in commands.Building.DeleteBlock.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_DeleteBlock(player));

        foreach (var cmd in commands.Building.RotateBlock.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_RotateBlock(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.SaveBlocks.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_SaveBlocks(player));

        foreach (var cmd in commands.Building.Snapping.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_Snapping(player));

        foreach (var cmd in commands.Building.Grid.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_Grid(player, command.ArgByIndex(1)));

        foreach (var cmd in commands.Building.Noclip.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_Noclip(player));

        foreach (var cmd in commands.Building.Godmode.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_Godmode(player));

        foreach (var cmd in commands.Building.TestBlock.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_Testblock(player));

        foreach (var cmd in commands.Building.ConvertBlock.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_ConvertBlock(player));

        foreach (var cmd in commands.Building.CopyBlock.Split(','))
            RemoveCommand($"css_{cmd}", (player, command) => Command_CopyBlock(player));
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

    public void Command_BuildMode(CCSPlayerController? player)
    {
        if (player == null || player.NotValid())
            return;

        if (!Utils.HasPermission(player))
        {
            Utils.PrintToChatAll($"{ChatColors.Red}You don't have permission to change Build Mode");
            return;
        }

        if (!buildMode)
        {
            buildMode = true;
            foreach (var target in Utilities.GetPlayers().Where(p => !p.IsBot))
            {
                playerData[target.Slot] = new PlayerData();
                Blocks.PlayerHolds[target] = new BuildingData();
                if (Utils.HasPermission(target))
                    playerData[target.Slot].Builder = true;
            }
        }
        else
        {
            buildMode = false;
            Blocks.PlayerHolds.Clear();
        }

        string status = buildMode ? "Enabled" : "Disabled";
        char color = buildMode ? ChatColors.Green : ChatColors.Red;

        Utils.PrintToChatAll($"Build Mode: {color}{status} {ChatColors.Grey}by {ChatColors.LightPurple}{player.PlayerName}");
    }

    public void Command_ManageBuilder(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (!Utils.HasPermission(player))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}You don't have permission to manage Builders");
            return;
        }

        string input = command.ArgString;

        if (string.IsNullOrEmpty(input))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}No player specified");
            return;
        }

        var targetPlayer = Utilities.GetPlayers()
            .FirstOrDefault(target => target.PlayerName.Contains(input, StringComparison.OrdinalIgnoreCase) || target.SteamID.ToString() == input);

        if (targetPlayer == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Player not found");
            return;
        }

        var builderStatus = playerData[targetPlayer.Slot].Builder;
        playerData[targetPlayer.Slot].Builder = !builderStatus;

        var action = builderStatus ? "removed" : "granted";
        var color = builderStatus ? ChatColors.Red : ChatColors.Green;

        Utils.PrintToChat(targetPlayer, $"{ChatColors.LightPurple}{player.PlayerName} {color}{action} your access to Build");
        Utils.PrintToChat(player, $"{color}You {action} {ChatColors.LightPurple}{targetPlayer.PlayerName} {color}access to Build");
    }

    public void Command_BuildMenu(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Menu.Command_OpenMenus(player);
    }

    public void Command_BlockType(CCSPlayerController? player, string selectType)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (string.IsNullOrEmpty(selectType))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}No block type specified");
            return;
        }

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var model = (BlockSizes)property.GetValue(BlockModels)!;

            if (string.Equals(model.Title, selectType, StringComparison.OrdinalIgnoreCase))
            {
                playerData[player.Slot].BlockType = model.Title;
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}{model.Title}");
                return;
            }
        }

        Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a matching block type");
    }

    public void Command_BlockColor(CCSPlayerController? player, string selectColor = "None")
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (string.IsNullOrEmpty(selectColor))
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}No color specified");
            return;
        }

        foreach (var color in Utils.ColorMapping.Keys)
        {
            if (string.Equals(color, selectColor, StringComparison.OrdinalIgnoreCase))
            {
                playerData[player.Slot].BlockColor = color;
                Utils.PrintToChat(player, $"Selected Color: {ChatColors.White}{color}");

                Blocks.RenderColor(player);

                return;
            }
        }

        Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a matching color");
    }

    public void Command_CreateBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Create(player);
    }

    public void Command_DeleteBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Delete(player);
    }

    public void Command_RotateBlock(CCSPlayerController? player, string rotation)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Rotate(player, rotation);
    }

    public void Command_SaveBlocks(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Save();
    }

    public void Command_Snapping(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        ToggleCommand(player, ref playerData[player.Slot].Snapping, "Block Snapping");
    }

    public void Command_Grid(CCSPlayerController? player, string grid)
    {
        if (player == null || !AllowedCommand(player))
            return;

        if (string.IsNullOrEmpty(grid))
        {
            ToggleCommand(player, ref playerData[player.Slot].Grid, "Block Grid");
            return;
        }

        playerData[player.Slot].GridValue = float.Parse(grid);

        Utils.PrintToChat(player, $"Selected Grid: {ChatColors.White}{grid} Units");
    }

    public void Command_Noclip(CCSPlayerController? player)
    {
        if (player == null || player.NotValid())
            return;

        if (!Utils.BuildMode(player))
            return;

        ToggleCommand(player, ref playerData[player.Slot].Noclip, "Noclip");

        if (playerData[player.Slot].Noclip)
        {
            player.Pawn.Value!.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            Schema.SetSchemaValue(player.Pawn.Value!.Handle, "CBaseEntity", "m_nActualMoveType", 8); // noclip
            Utilities.SetStateChanged(player.Pawn.Value!, "CBaseEntity", "m_MoveType");
        }

        else if (!playerData[player.Slot].Noclip)
        {
            player.Pawn.Value!.MoveType = MoveType_t.MOVETYPE_WALK;
            Schema.SetSchemaValue(player!.Pawn.Value!.Handle, "CBaseEntity", "m_nActualMoveType", 2); // walk
            Utilities.SetStateChanged(player!.Pawn.Value!, "CBaseEntity", "m_MoveType");
        }
    }

    public void Command_Godmode(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        ToggleCommand(player, ref playerData[player.Slot].Godmode, "Godmode");

        if (playerData[player.Slot].Godmode)
            player.Pawn()!.TakesDamage = false;

        else if (!playerData[player.Slot].Godmode)
            player.Pawn()!.TakesDamage = true;
    }

    public void Command_Testblock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Test(player);
    }

    public void Command_ClearBlocks(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Clear();

        Utils.PlaySoundAll(Config.Sounds.Building.Delete);
        Utils.PrintToChatAll($"{ChatColors.Red}Blocks cleared by {ChatColors.LightPurple}{player.PlayerName}");
    }

    public void Command_ConvertBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Convert(player);
    }

    public void Command_CopyBlock(CCSPlayerController? player)
    {
        if (player == null || !AllowedCommand(player))
            return;

        Blocks.Copy(player);
    }

    public void Command_TransparenyBlock(CCSPlayerController? player, string transparency = "0%")
    {
        if (player == null || !AllowedCommand(player))
            return;

        var entity = player.GetBlockAimTarget();

        if (entity == null)
            return;

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Blocks.BlocksEntities.TryGetValue(entity, out var block))
        {
            Blocks.BlocksEntities[entity].Transparency = transparency;

            var color = Utils.GetColor(block.Color);
            int alpha = Utils.GetAlpha(transparency);
            entity.Render = Color.FromArgb(alpha, color.R, color.G, color.B);
            Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");

            Utils.PrintToChat(player, $"Changed block transparency to {ChatColors.White}{transparency}");
        }
    }
}