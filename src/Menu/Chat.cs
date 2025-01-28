using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

public static class MenuChat
{
    private static Plugin Instance = Plugin.Instance;
    private static Dictionary<int, PlayerData> playerData = Instance.playerData;

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
            Commands.CreateBlock(player);
        });

        CommandsMenu.AddMenuOption("Delete", (player, menuOption) =>
        {
            Commands.DeleteBlock(player);
        });

        CommandsMenu.AddMenuOption("Rotate", (player, menuOption) =>
        {
            float[] rotateValues = Plugin.Instance.Config.Settings.Building.RotationValues;
            string[] rotateOptions = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

            RotateMenuOptions(player, rotateOptions, rotateValues);
        });

        CommandsMenu.AddMenuOption("Convert", (player, menuOption) =>
        {
            Commands.ConvertBlock(player);
        });

        CommandsMenu.AddMenuOption("Copy", (player, menuOption) =>
        {
            Commands.CopyBlock(player);
        });

        MenuManager.OpenChatMenu(player, CommandsMenu);
    }

    private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        ChatMenu RotateMenu = new($"Rotate Block ({playerData[player.Slot].RotationValue} Units)");

        RotateMenu.AddMenuOption($"Select Units", (player, option) =>
        {
            RotateValuesMenuOptions(player, rotateOptions, rotateValues);
        });

        foreach (string rotateOption in rotateOptions)
        {
            RotateMenu.AddMenuOption(rotateOption, (player, option) =>
            {
                Commands.RotateBlock(player, rotateOption);
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
                playerData[player.Slot].RotationValue = rotateValueOption;

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

        BlockMenu.AddMenuOption($"Type: {playerData[player.Slot].BlockType}", (player, menuOption) =>
        {
            TypeMenuOptions(player);
        });

        BlockMenu.AddMenuOption($"Size: {playerData[player.Slot].BlockSize}", (player, menuOption) =>
        {
            string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

            SizeMenuOptions(player, sizeValues);
        });

        BlockMenu.AddMenuOption($"Team: {playerData[player.Slot].BlockTeam}", (player, menuOption) =>
        {
            string[] teamValues = { "Both", "T", "CT" };

            TeamMenuOptions(player, teamValues);
        });

        BlockMenu.AddMenuOption($"Grid: {playerData[player.Slot].GridValue} Units", (player, menuOption) =>
        {
            float[] gridValues = Plugin.Instance.Config.Settings.Building.GridValues;

            GridMenuOptions(player, gridValues);
        });

        BlockMenu.AddMenuOption($"Transparency: {playerData[player.Slot].BlockTransparency}", (player, menuOption) =>
        {
            TransparencyMenuOptions(player);
        });

        BlockMenu.AddMenuOption($"Color: {playerData[player.Slot].BlockColor}", (player, menuOption) =>
        {
            ColorMenuOptions(player);
        });

        MenuManager.OpenChatMenu(player, BlockMenu);
    }

    private static void TypeMenuOptions(CCSPlayerController player)
    {
        ChatMenu TypeMenu = new($"Select Type ({playerData[player.Slot].BlockType})");

        var blockmodels = Files.BlockModels;

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockModel)property.GetValue(blockmodels)!;

            string blockName = block.Title;

            TypeMenu.AddMenuOption(blockName, (player, menuOption) =>
            {
                if (block.Title == blockmodels.Pistol.Title ||
                    block.Title == blockmodels.Sniper.Title ||
                    block.Title == blockmodels.Rifle.Title ||
                    block.Title == blockmodels.SMG.Title ||
                    block.Title == blockmodels.ShotgunHeavy.Title
                )
                {
                    GunTypeMenu(player, block.Title);
                    return;
                }

                Commands.BlockType(player, blockName);

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
                            var model = (BlockModel)property.GetValue(Files.BlockModels)!;

                            if (string.Equals(model.Title, gunType, StringComparison.OrdinalIgnoreCase))
                            {
                                playerData[player.Slot].BlockType = $"{model.Title}.{weapon.Name}";
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
        ChatMenu SizeMenu = new($"Select Size ({playerData[player.Slot].BlockSize})");

        SizeMenu.AddMenuOption($"Pole: {(playerData[player.Slot].Pole ? "ON" : "OFF")}", (player, option) =>
        {
            Commands.Pole(player);

            SizeMenuOptions(player, sizeValues);
        });

        foreach (string sizeValue in sizeValues)
        {
            SizeMenu.AddMenuOption(sizeValue, (player, option) =>
            {
                playerData[player.Slot].BlockSize = sizeValue;

                Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, SizeMenu);
    }

    private static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
    {
        ChatMenu TeamMenu = new($"Select Team ({playerData[player.Slot].BlockTeam})");

        foreach (string teamValue in teamValues)
        {
            TeamMenu.AddMenuOption(teamValue, (player, option) =>
            {
                playerData[player.Slot].BlockTeam = teamValue;

                Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, TeamMenu);
    }

    private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
    {
        ChatMenu GridMenu = new($"Select Grid ({(playerData[player.Slot].Grid ? "ON" : "OFF")} - {playerData[player.Slot].GridValue})");

        GridMenu.AddMenuOption($"Toggle Grid", (player, option) =>
        {
            Commands.Grid(player, "");

            GridMenuOptions(player, gridValues);
        });

        foreach (float gridValue in gridValues)
        {
            GridMenu.AddMenuOption(gridValue.ToString() + " Units", (player, option) =>
            {
                Commands.Grid(player, gridValue.ToString());

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, GridMenu);
    }

    private static void TransparencyMenuOptions(CCSPlayerController player)
    {
        ChatMenu TransparencyMenu = new($"Select Transparency ({playerData[player.Slot].BlockTransparency})");

        foreach (var value in Utils.AlphaMapping.Keys)
        {
            TransparencyMenu.AddMenuOption(value, (player, menuOption) =>
            {
                playerData[player.Slot].BlockTransparency = value;

                Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                Commands.TransparenyBlock(player, value);

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenChatMenu(player, TransparencyMenu);
    }

    private static void ColorMenuOptions(CCSPlayerController player)
    {
        ChatMenu ColorMenu = new($"Select Color ({playerData[player.Slot].BlockColor})");

        foreach (var color in Utils.ColorMapping.Keys)
        {
            ColorMenu.AddMenuOption(color, (player, menuOption) =>
            {
                Commands.BlockColor(player, color);

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
            Commands.BuildMode(player);

            Menu_Settings(player);
        });

        SettingsMenu.AddMenuOption("Godmode: " + (playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Commands.Godmode(player);

            Menu_Settings(player);
        });

        SettingsMenu.AddMenuOption("Noclip: " + (playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
        {
            Commands.Noclip(player);

            Menu_Settings(player);
        });

        SettingsMenu.AddMenuOption("Save Blocks", (player, menuOption) =>
        {
            Commands.SaveBlocks(player);

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
                Commands.ClearBlocks(player);

                Menu_Settings(player);
            });

            MenuManager.OpenChatMenu(player, ConfirmMenu);
        });

        SettingsMenu.AddMenuOption("Manage Builders", (player, menuOption) =>
        {
            ChatMenu BuildersMenu = new("Manage Builders");

            foreach (var target in Utilities.GetPlayers().Where(t => t.SteamID != player.SteamID && t.SteamID > 0))
            {
                BuildersMenu.AddMenuOption(target.PlayerName, (player, menuOption) =>
                {
                    Commands.ManageBuilder(player, target.SteamID.ToString());
                });
            }

            MenuManager.OpenChatMenu(player, BuildersMenu);
        });

        MenuManager.OpenChatMenu(player, SettingsMenu);
    }

    /* Menu_Settings */
}