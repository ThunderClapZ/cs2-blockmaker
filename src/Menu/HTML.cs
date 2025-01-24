using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using static Plugin;

public static class MenuHTML
{
    public static void OpenMenu(CCSPlayerController player)
    {
        CenterHtmlMenu MainMenu = new("Block Maker", Instance);

        MainMenu.AddMenuOption($"Block Commands", (player, menuOption) =>
        {
            CenterHtmlMenu CommandsMenu = new("Block Commands", Instance);

            CommandsMenu.AddMenuOption("Create", (player, menuOption) =>
            {
                Instance.Command_CreateBlock(player);
            });

            CommandsMenu.AddMenuOption("Delete", (player, menuOption) =>
            {
                Instance.Command_DeleteBlock(player);
            });

            CommandsMenu.AddMenuOption("Rotate", (player, menuOption) =>
            {
                float[] rotateValues = Instance.Config.Settings.Building.RotationValues;
                string[] rotateOptions = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });

            CommandsMenu.AddMenuOption("Convert", (player, menuOption) =>
            {
                Instance.Command_ConvertBlock(player);
            });

            CommandsMenu.AddMenuOption("Copy", (player, menuOption) =>
            {
                Instance.Command_CopyBlock(player);
                MenuManager.OpenCenterHtmlMenu(Instance, player, CommandsMenu);
            });

            MenuManager.OpenCenterHtmlMenu(Instance, player, CommandsMenu);
        });

        MainMenu.AddMenuOption($"Block Settings", (player, menuOption) =>
        {
            CenterHtmlMenu BlockMenu = new("Block Settings", Instance);

            BlockMenu.AddMenuOption($"Type: {Instance.playerData[player.Slot].BlockType}", (player, menuOption) =>
            {
                TypeMenuOptions(player);
            });

            BlockMenu.AddMenuOption($"Size: {Instance.playerData[player.Slot].BlockSize}", (player, menuOption) =>
            {
                string[] sizeValues = { "Pole", "Small", "Normal", "Large", "X-Large" };

                SizeMenuOptions(player, sizeValues);
            });

            BlockMenu.AddMenuOption($"Team: {Instance.playerData[player.Slot].BlockTeam}", (player, menuOption) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, BlockMenu, teamValues);
            });

            BlockMenu.AddMenuOption($"Grid: {Instance.playerData[player.Slot].GridValue} Units", (player, menuOption) =>
            {
                float[] gridValues = Instance.Config.Settings.Building.GridValues;

                GridMenuOptions(player, gridValues);
            });

            BlockMenu.AddMenuOption($"Transparency: {Instance.playerData[player.Slot].BlockTransparency}", (player, menuOption) =>
            {
                TransparencyMenuOptions(player);
            });

            BlockMenu.AddMenuOption($"Color: {Instance.playerData[player.Slot].BlockColor}", (player, menuOption) =>
            {
                ColorMenuOptions(player);
            });

            MenuManager.OpenCenterHtmlMenu(Instance, player, BlockMenu);
        });

        MainMenu.AddMenuOption("Build Settings", (player, menuOption) =>
        {
            SettingsOptions(player);
        });

        MenuManager.OpenCenterHtmlMenu(Instance, player, MainMenu);
    }

    private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        CenterHtmlMenu RotateMenu = new($"Rotate Block ({Instance.playerData[player.Slot].RotationValue} Units)", Instance);

        RotateMenu.AddMenuOption($"Select Units", (player, option) =>
        {
            RotateValuesMenuOptions(player, rotateOptions, rotateValues);
        });

        foreach (string rotateOption in rotateOptions)
        {
            RotateMenu.AddMenuOption(rotateOption, (player, option) =>
            {
                Instance.Command_RotateBlock(player, rotateOption);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, RotateMenu);
    }

    private static void RotateValuesMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        CenterHtmlMenu RotateValuesMenu = new($"Rotate Values", Instance);

        foreach (float rotateValueOption in rotateValues)
        {
            RotateValuesMenu.AddMenuOption(rotateValueOption.ToString() + " Units", (player, option) =>
            {
                Instance.playerData[player.Slot].RotationValue = rotateValueOption;

                Instance.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, RotateValuesMenu);
    }

    private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
    {
        CenterHtmlMenu SizeMenu = new($"Select Size ({Instance.playerData[player.Slot].BlockSize})", Instance);

        foreach (string sizeValue in sizeValues)
        {
            SizeMenu.AddMenuOption(sizeValue, (player, option) =>
            {
                Instance.playerData[player.Slot].BlockSize = sizeValue;

                Instance.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                SizeMenuOptions(player, sizeValues);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, SizeMenu);
    }

    private static void TeamMenuOptions(CCSPlayerController player, CenterHtmlMenu openMainMenu, string[] teamValues)
    {
        CenterHtmlMenu TeamMenu = new($"Select Team ({Instance.playerData[player.Slot].BlockTeam})", Instance);

        foreach (string teamValue in teamValues)
        {
            TeamMenu.AddMenuOption(teamValue, (player, option) =>
            {
                Instance.playerData[player.Slot].BlockTeam = teamValue;

                Instance.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                MenuManager.OpenCenterHtmlMenu(Instance, player, openMainMenu);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, TeamMenu);
    }

    private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
    {
        CenterHtmlMenu GridMenu = new($"Select Grid ({(Instance.playerData[player.Slot].Grid ? "ON" : "OFF")} - {Instance.playerData[player.Slot].GridValue})", Instance);

        GridMenu.AddMenuOption($"Toggle Grid", (player, option) =>
        {
            Instance.Command_Grid(player, "");

            GridMenuOptions(player, gridValues);
        });

        foreach (float gridValue in gridValues)
        {
            GridMenu.AddMenuOption(gridValue.ToString() + " Units", (player, option) =>
            {
                Instance.Command_Grid(player, gridValue.ToString());

                GridMenuOptions(player, gridValues);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, GridMenu);
    }

    private static void TypeMenuOptions(CCSPlayerController player)
    {
        CenterHtmlMenu TypeMenu = new($"Select Type ({Instance.playerData[player.Slot].BlockType})", Instance);

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockSizes)property.GetValue(Plugin.BlockModels)!;

            string blockName = block.Title;

            TypeMenu.AddMenuOption(blockName, (player, menuOption) =>
            {
                Instance.Command_BlockType(player, blockName);

                TypeMenuOptions(player);
            });
        }
        MenuManager.OpenCenterHtmlMenu(Instance, player, TypeMenu);
    }

    private static void ColorMenuOptions(CCSPlayerController player)
    {
        CenterHtmlMenu ColorMenu = new($"Select Color ({Instance.playerData[player.Slot].BlockColor})", Instance);

        foreach (var color in ColorMapping.Keys)
        {
            ColorMenu.AddMenuOption(color, (player, menuOption) =>
            {
                Instance.Command_BlockColor(player, color);

                TypeMenuOptions(player);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, ColorMenu);
    }

    private static void TransparencyMenuOptions(CCSPlayerController player)
    {
        CenterHtmlMenu TransparencyMenu = new($"Select Transparency ({Instance.playerData[player.Slot].BlockTransparency})", Instance);

        foreach (var value in AlphaMapping.Keys)
        {
            TransparencyMenu.AddMenuOption(value, (player, menuOption) =>
            {
                Instance.playerData[player.Slot].BlockTransparency = value;

                Instance.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                Instance.Command_TransparenyBlock(player, value);

                TransparencyMenuOptions(player);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, TransparencyMenu);
    }


    private static void SettingsOptions(CCSPlayerController player)
    {
        CenterHtmlMenu SettingsMenu = new("Settings", Instance);

        SettingsMenu.AddMenuOption("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_BuildMode(player);

            SettingsOptions(player);
        });

        SettingsMenu.AddMenuOption("Godmode: " + (Instance.playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_Godmode(player);

            SettingsOptions(player);
        });

        SettingsMenu.AddMenuOption("Noclip: " + (Instance.playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_Noclip(player);

            SettingsOptions(player);
        });

        SettingsMenu.AddMenuOption("Save Blocks", (player, menuOption) =>
        {
            Instance.Command_SaveBlocks(player);

            SettingsOptions(player);
        });

        SettingsMenu.AddMenuOption("Clear Blocks", (player, menuOption) =>
        {
            CenterHtmlMenu ConfirmMenu = new("Confirm", Instance);

            ConfirmMenu.AddMenuOption("NO - keep blocks", (player, menuOption) =>
            {
                SettingsOptions(player);
            });

            ConfirmMenu.AddMenuOption("YES - remove blocks", (player, menuOption) =>
            {
                Instance.Command_ClearBlocks(player);

                SettingsOptions(player);
            });

            MenuManager.OpenCenterHtmlMenu(Instance, player, ConfirmMenu);
        });

        MenuManager.OpenCenterHtmlMenu(Instance, player, SettingsMenu);
    }
}