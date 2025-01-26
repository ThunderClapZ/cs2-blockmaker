using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using static Plugin;

public static class MenuChat
{
    public static void OpenMenu(CCSPlayerController player)
    {
        ChatMenu MainMenu = new("Block Maker");

        MainMenu.AddMenuOption($"Block Commands", (player, menuOption) =>
        {
            Menu_Commands(player);
        });

        MainMenu.AddMenuOption($"Block Settings", (player, menuOption) =>
        {
            Menu_BlockSettings(player);
        });

        MainMenu.AddMenuOption("Build Settings", (player, menuOption) =>
        {
            Menu_Settings(player);
        });

        MenuManager.OpenChatMenu(player, MainMenu);
    }

    /* Menu_Commands */

    private static void Menu_Commands(CCSPlayerController player)
    {
        ChatMenu CommandsMenu = new("Block Commands");

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
        });

        MenuManager.OpenChatMenu(player, CommandsMenu);
    }

    private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        ChatMenu RotateMenu = new($"Rotate Block ({Instance.playerData[player.Slot].RotationValue} Units)");

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

        MenuManager.OpenChatMenu(player, RotateMenu);
    }

    private static void RotateValuesMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        ChatMenu RotateValuesMenu = new($"Rotate Values");

        foreach (float rotateValueOption in rotateValues)
        {
            RotateValuesMenu.AddMenuOption(rotateValueOption.ToString() + " Units", (player, option) =>
            {
                Instance.playerData[player.Slot].RotationValue = rotateValueOption;

                Utils.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });
        }

        MenuManager.OpenChatMenu(player, RotateValuesMenu);
    }

    /* Menu_Commands */

    /* Menu_BlockSettings */

    private static void Menu_BlockSettings(CCSPlayerController player)
    {
        ChatMenu BlockMenu = new("Block Settings");

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

            TeamMenuOptions(player, teamValues);
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

        MenuManager.OpenChatMenu(player, BlockMenu);
    }

    private static void TypeMenuOptions(CCSPlayerController player)
    {
        ChatMenu TypeMenu = new($"Select Type ({Instance.playerData[player.Slot].BlockType})");

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockSizes)property.GetValue(Plugin.BlockModels)!;

            string blockName = block.Title;

            TypeMenu.AddMenuOption(blockName, (player, menuOption) =>
            {
                if (block.Title == "Pistol" ||
                    block.Title == "Sniper" ||
                    block.Title == "Rifle" ||
                    block.Title == "SMG" ||
                    block.Title == "Shotgun/Heavy"
                )
                {
                    GunTypeMenu(player, block.Title);
                    return;
                }

                Instance.Command_BlockType(player, blockName);

                Menu_BlockSettings(player);
            });
        }
        MenuManager.OpenChatMenu(player, TypeMenu);
    }

    private static void GunTypeMenu(CCSPlayerController player, string gunType)
    {
        ChatMenu GunTypeMenu = new($"Select {gunType}");

        if (WeaponList.Categories.ContainsKey(gunType))
        {
            var weaponsInCategory = WeaponList.Categories[gunType];

            foreach (var weaponID in weaponsInCategory)
            {
                var weapon = WeaponList.Weapons.FirstOrDefault(w => w.Designer == weaponID);

                if (weapon != null)
                {
                    GunTypeMenu.AddMenuOption(weapon.Name, (player, menuOption) =>
                    {
                        foreach (var property in typeof(BlockModels).GetProperties())
                        {
                            var model = (BlockSizes)property.GetValue(Plugin.BlockModels)!;

                            if (string.Equals(model.Title, gunType, StringComparison.OrdinalIgnoreCase))
                            {
                                Instance.playerData[player.Slot].BlockType = $"{model.Title}.{weapon.Name}";
                                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}{model.Title}.{weapon.Name}");

                                Menu_BlockSettings(player);
                                return;
                            }
                        }
                    });
                }
            }
        }

        MenuManager.OpenChatMenu(player, GunTypeMenu);
    }

    private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
    {
        ChatMenu SizeMenu = new($"Select Size ({Instance.playerData[player.Slot].BlockSize})");

        foreach (string sizeValue in sizeValues)
        {
            SizeMenu.AddMenuOption(sizeValue, (player, option) =>
            {
                Instance.playerData[player.Slot].BlockSize = sizeValue;

                Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, SizeMenu);
    }

    private static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
    {
        ChatMenu TeamMenu = new($"Select Team ({Instance.playerData[player.Slot].BlockTeam})");

        foreach (string teamValue in teamValues)
        {
            TeamMenu.AddMenuOption(teamValue, (player, option) =>
            {
                Instance.playerData[player.Slot].BlockTeam = teamValue;

                Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, TeamMenu);
    }

    private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
    {
        ChatMenu GridMenu = new($"Select Grid ({(Instance.playerData[player.Slot].Grid ? "ON" : "OFF")} - {Instance.playerData[player.Slot].GridValue})");

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

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, GridMenu);
    }

    private static void TransparencyMenuOptions(CCSPlayerController player)
    {
        ChatMenu TransparencyMenu = new($"Select Transparency ({Instance.playerData[player.Slot].BlockTransparency})");

        foreach (var value in Utils.AlphaMapping.Keys)
        {
            TransparencyMenu.AddMenuOption(value, (player, menuOption) =>
            {
                Instance.playerData[player.Slot].BlockTransparency = value;

                Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                Instance.Command_TransparenyBlock(player, value);

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, TransparencyMenu);
    }

    private static void ColorMenuOptions(CCSPlayerController player)
    {
        ChatMenu ColorMenu = new($"Select Color ({Instance.playerData[player.Slot].BlockColor})");

        foreach (var color in Utils.ColorMapping.Keys)
        {
            ColorMenu.AddMenuOption(color, (player, menuOption) =>
            {
                Instance.Command_BlockColor(player, color);

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, ColorMenu);
    }

    /* Menu_BlockSettings */

    /* Menu_Settings */

    private static void Menu_Settings(CCSPlayerController player)
    {
        ChatMenu SettingsMenu = new("Settings");

        SettingsMenu.AddMenuOption("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_BuildMode(player);

            Menu_Settings(player);
        });

        SettingsMenu.AddMenuOption("Godmode: " + (Instance.playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_Godmode(player);

            Menu_Settings(player);
        });

        SettingsMenu.AddMenuOption("Noclip: " + (Instance.playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
        {
            Instance.Command_Noclip(player);

            Menu_Settings(player);
        });

        SettingsMenu.AddMenuOption("Save Blocks", (player, menuOption) =>
        {
            Instance.Command_SaveBlocks(player);

            Menu_Settings(player);
        });

        SettingsMenu.AddMenuOption("Clear Blocks", (player, menuOption) =>
        {
            ChatMenu ConfirmMenu = new("Confirm");

            ConfirmMenu.AddMenuOption("NO - keep blocks", (player, menuOption) =>
            {
                Menu_Settings(player);
            });

            ConfirmMenu.AddMenuOption("YES - remove blocks", (player, menuOption) =>
            {
                Instance.Command_ClearBlocks(player);

                Menu_Settings(player);
            });

            MenuManager.OpenChatMenu(player, ConfirmMenu);
        });

        MenuManager.OpenChatMenu(player, SettingsMenu);
    }

    /* Menu_Settings */
}