using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static Plugin;

public static class MenuWASD
{
    public static void OpenMenu(CCSPlayerController player)
    {
        IWasdMenu MainMenu = WasdManager.CreateMenu("Block Maker");

        MainMenu.Add($"Block Commands", (player, menuOption) =>
        {
            IWasdMenu CommandsMenu = WasdManager.CreateMenu("Block Commands");

            CommandsMenu.Add("Create", (player, menuOption) =>
            {
                Instance.Command_CreateBlock(player);
            });

            CommandsMenu.Add("Delete", (player, menuOption) =>
            {
                Instance.Command_DeleteBlock(player);
            });

            CommandsMenu.Add("Rotate", (player, menuOption) =>
            {
                float[] rotateValues = Instance.Config.Settings.Building.RotationValues;
                string[] rotateOptions = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });

            CommandsMenu.Add("Convert", (player, menuOption) =>
            {
                Instance.Command_ConvertBlock(player);
            });

            CommandsMenu.Add("Copy", (player, menuOption) =>
            {
                Instance.Command_CopyBlock(player);
                WasdManager.OpenMainMenu(player, CommandsMenu);
            });

            WasdManager.OpenMainMenu(player, CommandsMenu);
        });

        MainMenu.Add($"Block Settings", (player, menuOption) =>
        {
            IWasdMenu BlockMenu = WasdManager.CreateMenu("Block Settings");

            BlockMenu.Add($"Type: {Instance.playerData[player.Slot].BlockType}", (player, menuOption) =>
            {
                TypeMenuOptions(player);
            });

            BlockMenu.Add($"Size: {Instance.playerData[player.Slot].BlockSize}", (player, menuOption) =>
            {
                string[] sizeValues = { "Pole", "Small", "Normal", "Large", "X-Large" };

                SizeMenuOptions(player, sizeValues);
            });

            BlockMenu.Add($"Team: {Instance.playerData[player.Slot].BlockTeam}", (player, menuOption) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, BlockMenu, teamValues);
            });

            BlockMenu.Add($"Grid: {Instance.playerData[player.Slot].GridValue} Units", (player, menuOption) =>
            {
                float[] gridValues = Instance.Config.Settings.Building.GridValues;

                GridMenuOptions(player, gridValues);
            });

            BlockMenu.Add($"Transparency: {Instance.playerData[player.Slot].BlockTransparency}", (player, menuOption) =>
            {
                TransparencyMenuOptions(player);
            });

            BlockMenu.Add($"Color: {Instance.playerData[player.Slot].BlockColor}", (player, menuOption) =>
            {
                ColorMenuOptions(player);
            });

            WasdManager.OpenMainMenu(player, BlockMenu);
        });

        MainMenu.Add("Build Settings", (player, menuOption) =>
        {
            SettingsOptions(player);
        });

        WasdManager.OpenMainMenu(player, MainMenu);
    }

    private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        IWasdMenu RotateMenu = WasdManager.CreateMenu($"Rotate Block ({Instance.playerData[player.Slot].RotationValue} Units)");

        RotateMenu.Add($"Select Units", (player, option) =>
        {
            RotateValuesMenuOptions(player, rotateOptions, rotateValues);
        });

        foreach (string rotateOption in rotateOptions)
        {
            RotateMenu.Add(rotateOption, (player, option) =>
            {
                Instance.Command_RotateBlock(player, rotateOption);
            });
        }

        WasdManager.OpenMainMenu(player, RotateMenu);
    }

    private static void RotateValuesMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        IWasdMenu RotateValuesMenu = WasdManager.CreateMenu($"Rotate Values");

        foreach (float rotateValueOption in rotateValues)
        {
            RotateValuesMenu.Add(rotateValueOption.ToString() + " Units", (player, option) =>
            {
                Instance.playerData[player.Slot].RotationValue = rotateValueOption;

                Instance.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });
        }

        WasdManager.OpenMainMenu(player, RotateValuesMenu);
    }

    private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
    {
        IWasdMenu SizeMenu = WasdManager.CreateMenu($"Select Size ({Instance.playerData[player.Slot].BlockSize})");

        foreach (string sizeValue in sizeValues)
        {
            SizeMenu.Add(sizeValue, (player, option) =>
            {
                Instance.playerData[player.Slot].BlockSize = sizeValue;

                Instance.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                SizeMenuOptions(player, sizeValues);
            });
        }

        WasdManager.OpenMainMenu(player, SizeMenu);
    }

    private static void TeamMenuOptions(CCSPlayerController player, IWasdMenu openMainMenu, string[] teamValues)
    {
        IWasdMenu TeamMenu = WasdManager.CreateMenu($"Select Team ({Instance.playerData[player.Slot].BlockTeam})");

        foreach (string teamValue in teamValues)
        {
            TeamMenu.Add(teamValue, (player, option) =>
            {
                Instance.playerData[player.Slot].BlockTeam = teamValue;

                Instance.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                WasdManager.OpenMainMenu(player, openMainMenu);
            });
        }

        WasdManager.OpenMainMenu(player, TeamMenu);
    }

    private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
    {
        IWasdMenu GridMenu = WasdManager.CreateMenu($"Select Grid ({(Instance.playerData[player.Slot].Grid ? "ON" : "OFF")} - {Instance.playerData[player.Slot].GridValue})");

        GridMenu.Add($"Toggle Grid", (player, option) =>
        {
            Instance.Command_Grid(player, "");

            GridMenuOptions(player, gridValues);
        });

        foreach (float gridValue in gridValues)
        {
            GridMenu.Add(gridValue.ToString() + " Units", (player, option) =>
            {
                Instance.Command_Grid(player, gridValue.ToString());

                GridMenuOptions(player, gridValues);
            });
        }

        WasdManager.OpenMainMenu(player, GridMenu);
    }

    private static void TypeMenuOptions(CCSPlayerController player)
    {
        IWasdMenu TypeMenu = WasdManager.CreateMenu($"Select Type ({Instance.playerData[player.Slot].BlockType})");

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockSizes)property.GetValue(Plugin.BlockModels)!;

            string blockName = block.Title;

            TypeMenu.Add(blockName, (player, menuOption) =>
            {
                Instance.Command_BlockType(player, blockName);

                TypeMenuOptions(player);
            });
        }
        WasdManager.OpenMainMenu(player, TypeMenu);
    }

    private static void ColorMenuOptions(CCSPlayerController player)
    {
        IWasdMenu ColorMenu = WasdManager.CreateMenu($"Select Color ({Instance.playerData[player.Slot].BlockColor})");

        foreach (var color in ColorMapping.Keys)
        {
            ColorMenu.Add(color, (player, menuOption) =>
            {
                Instance.Command_BlockColor(player, color);

                TypeMenuOptions(player);
            });
        }

        WasdManager.OpenMainMenu(player, ColorMenu);
    }

    private static void TransparencyMenuOptions(CCSPlayerController player)
    {
        IWasdMenu TransparencyMenu = WasdManager.CreateMenu($"Select Transparency ({Instance.playerData[player.Slot].BlockTransparency})");

        foreach (var value in AlphaMapping.Keys)
        {
            TransparencyMenu.Add(value, (player, menuOption) =>
            {
                Instance.playerData[player.Slot].BlockTransparency = value;

                Instance.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                Instance.Command_TransparenyBlock(player, value);

                TransparencyMenuOptions(player);
            });
        }

        WasdManager.OpenMainMenu(player, TransparencyMenu);
    }


    private static void SettingsOptions(CCSPlayerController player)
    {
        IWasdMenu SettingsMenu = WasdManager.CreateMenu("Settings");

        SettingsMenu.Add("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_BuildMode(player);

            SettingsOptions(player);
        });

        SettingsMenu.Add("Godmode: " + (Instance.playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_Godmode(player);

            SettingsOptions(player);
        });

        SettingsMenu.Add("Noclip: " + (Instance.playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_Noclip(player);

            SettingsOptions(player);
        });

        SettingsMenu.Add("Save Blocks", (player, menuOption) =>
        {
            Instance.Command_SaveBlocks(player);

            SettingsOptions(player);
        });

        SettingsMenu.Add("Clear Blocks", (player, menuOption) =>
        {
            IWasdMenu ConfirmMenu = WasdManager.CreateMenu("Confirm");

            ConfirmMenu.Add("NO - keep blocks", (player, menuOption) =>
            {
                SettingsOptions(player);
            });

            ConfirmMenu.Add("YES - remove blocks", (player, menuOption) =>
            {
                Instance.Command_ClearBlocks(player);

                SettingsOptions(player);
            });

            WasdManager.OpenMainMenu(player, ConfirmMenu);
        });

        WasdManager.OpenMainMenu(player, SettingsMenu);
    }
}