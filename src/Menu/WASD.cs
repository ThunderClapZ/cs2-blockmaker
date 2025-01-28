using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public static class MenuWASD
{
    private static Plugin Instance = Plugin.Instance;
    private static Dictionary<int, PlayerData> playerData = Instance.playerData;

    public static void OpenMenu(CCSPlayerController player)
    {
        IWasdMenu MainMenu = WasdManager.CreateMenu("Block Maker");

        MainMenu.Add($"Block Commands", (player, menuOption) =>
        {
            Menu_Commands(player);
        });

        MainMenu.Add($"Block Settings", (player, menuOption) =>
        {
            Menu_BlockSettings(player);
        });

        MainMenu.Add("Build Settings", (player, menuOption) =>
        {
            Menu_Settings(player);
        });

        WasdManager.OpenMainMenu(player, MainMenu);
    }

    /* Menu_Commands */

    private static void Menu_Commands(CCSPlayerController player)
    {
        IWasdMenu CommandsMenu = WasdManager.CreateMenu("Block Commands");

        CommandsMenu.Add("Create", (player, menuOption) =>
        {
            Commands.CreateBlock(player);
        });

        CommandsMenu.Add("Delete", (player, menuOption) =>
        {
            Commands.DeleteBlock(player);
        });

        CommandsMenu.Add("Rotate", (player, menuOption) =>
        {
            float[] rotateValues = Instance.Config.Settings.Building.RotationValues;
            string[] rotateOptions = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

            RotateMenuOptions(player, rotateOptions, rotateValues);
        });

        CommandsMenu.Add("Convert", (player, menuOption) =>
        {
            Commands.ConvertBlock(player);
        });

        CommandsMenu.Add("Copy", (player, menuOption) =>
        {
            Commands.CopyBlock(player);
        });

        WasdManager.OpenMainMenu(player, CommandsMenu);
    }

    private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        IWasdMenu RotateMenu = WasdManager.CreateMenu($"Rotate Block ({playerData[player.Slot].RotationValue} Units)");

        RotateMenu.Add($"Select Units", (player, option) =>
        {
            RotateValuesMenuOptions(player, rotateOptions, rotateValues);
        });

        foreach (string rotateOption in rotateOptions)
        {
            RotateMenu.Add(rotateOption, (player, option) =>
            {
                Commands.RotateBlock(player, rotateOption);
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
                playerData[player.Slot].RotationValue = rotateValueOption;

                Utils.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });
        }

        WasdManager.OpenMainMenu(player, RotateValuesMenu);
    }

    /* Menu_Commands */

    /* Menu_BlockSettings */

    private static void Menu_BlockSettings(CCSPlayerController player)
    {
        IWasdMenu BlockMenu = WasdManager.CreateMenu("Block Settings");

        BlockMenu.Add($"Type: {playerData[player.Slot].BlockType}", (player, menuOption) =>
        {
            TypeMenuOptions(player);
        });

        BlockMenu.Add($"Size: {playerData[player.Slot].BlockSize}", (player, menuOption) =>
        {
            string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

            SizeMenuOptions(player, sizeValues);
        });

        BlockMenu.Add($"Team: {playerData[player.Slot].BlockTeam}", (player, menuOption) =>
        {
            string[] teamValues = { "Both", "T", "CT" };

            TeamMenuOptions(player, teamValues);
        });

        BlockMenu.Add($"Grid: {playerData[player.Slot].GridValue} Units", (player, menuOption) =>
        {
            float[] gridValues = Instance.Config.Settings.Building.GridValues;

            GridMenuOptions(player, gridValues);
        });

        BlockMenu.Add($"Transparency: {playerData[player.Slot].BlockTransparency}", (player, menuOption) =>
        {
            TransparencyMenuOptions(player);
        });

        BlockMenu.Add($"Color: {playerData[player.Slot].BlockColor}", (player, menuOption) =>
        {
            ColorMenuOptions(player);
        });

        WasdManager.OpenMainMenu(player, BlockMenu);
    }

    private static void TypeMenuOptions(CCSPlayerController player)
    {
        IWasdMenu TypeMenu = WasdManager.CreateMenu($"Select Type ({playerData[player.Slot].BlockType})");

        var blockmodels = Files.BlockModels;

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockModel)property.GetValue(blockmodels)!;

            string blockName = block.Title;

            TypeMenu.Add(blockName, (player, menuOption) =>
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
        WasdManager.OpenMainMenu(player, TypeMenu);
    }

    private static void GunTypeMenu(CCSPlayerController player, string gunType)
    {
        IWasdMenu GunTypeMenu = WasdManager.CreateMenu($"Select {gunType}");

        if (WeaponList.Categories.ContainsKey(gunType))
        {
            var weaponsInCategory = WeaponList.Categories[gunType];

            foreach (var weaponID in weaponsInCategory)
            {
                var weapon = WeaponList.Weapons.FirstOrDefault(w => w.Designer == weaponID);

                if (weapon != null)
                {
                    GunTypeMenu.Add(weapon.Name, (player, menuOption) =>
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

        WasdManager.OpenMainMenu(player, GunTypeMenu);
    }

    private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
    {
        IWasdMenu SizeMenu = WasdManager.CreateMenu($"Select Size ({playerData[player.Slot].BlockSize})");

        SizeMenu.Add($"Pole: {(playerData[player.Slot].Pole ? "ON" : "OFF")}", (player, option) =>
        {
            Commands.Pole(player);

            SizeMenuOptions(player, sizeValues);
        });

        foreach (string sizeValue in sizeValues)
        {
            SizeMenu.Add(sizeValue, (player, option) =>
            {
                playerData[player.Slot].BlockSize = sizeValue;

                Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                Menu_BlockSettings(player);
            });
        }

        WasdManager.OpenMainMenu(player, SizeMenu);
    }

    private static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
    {
        IWasdMenu TeamMenu = WasdManager.CreateMenu($"Select Team ({playerData[player.Slot].BlockTeam})");

        foreach (string teamValue in teamValues)
        {
            TeamMenu.Add(teamValue, (player, option) =>
            {
                playerData[player.Slot].BlockTeam = teamValue;

                Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                Menu_BlockSettings(player);
            });
        }

        WasdManager.OpenMainMenu(player, TeamMenu);
    }

    private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
    {
        IWasdMenu GridMenu = WasdManager.CreateMenu($"Select Grid ({(playerData[player.Slot].Grid ? "ON" : "OFF")} - {playerData[player.Slot].GridValue})");

        GridMenu.Add($"Toggle Grid", (player, option) =>
        {
            Commands.Grid(player, "");

            GridMenuOptions(player, gridValues);
        });

        foreach (float gridValue in gridValues)
        {
            GridMenu.Add(gridValue.ToString() + " Units", (player, option) =>
            {
                Commands.Grid(player, gridValue.ToString());

                Menu_BlockSettings(player);
            });
        }

        WasdManager.OpenMainMenu(player, GridMenu);
    }

    private static void TransparencyMenuOptions(CCSPlayerController player)
    {
        IWasdMenu TransparencyMenu = WasdManager.CreateMenu($"Select Transparency ({playerData[player.Slot].BlockTransparency})");

        foreach (var value in Utils.AlphaMapping.Keys)
        {
            TransparencyMenu.Add(value, (player, menuOption) =>
            {
                playerData[player.Slot].BlockTransparency = value;

                Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                Commands.TransparenyBlock(player, value);

                Menu_BlockSettings(player);
            });
        }

        WasdManager.OpenMainMenu(player, TransparencyMenu);
    }

    private static void ColorMenuOptions(CCSPlayerController player)
    {
        IWasdMenu ColorMenu = WasdManager.CreateMenu($"Select Color ({playerData[player.Slot].BlockColor})");

        foreach (var color in Utils.ColorMapping.Keys)
        {
            ColorMenu.Add(color, (player, menuOption) =>
            {
                Commands.BlockColor(player, color);

                Menu_BlockSettings(player);
            });
        }

        WasdManager.OpenMainMenu(player, ColorMenu);
    }

    /* Menu_BlockSettings */

    /* Menu_Settings */

    private static void Menu_Settings(CCSPlayerController player)
    {
        IWasdMenu SettingsMenu = WasdManager.CreateMenu("Settings");

        SettingsMenu.Add("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Commands.BuildMode(player);

            Menu_Settings(player);
        });

        SettingsMenu.Add("Godmode: " + (playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
        {
            Commands.Godmode(player);

            Menu_Settings(player);
        });

        SettingsMenu.Add("Noclip: " + (playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
        {
            Commands.Noclip(player);

            Menu_Settings(player);
        });

        SettingsMenu.Add("Save Blocks", (player, menuOption) =>
        {
            Commands.SaveBlocks(player);

            Menu_Settings(player);
        });

        SettingsMenu.Add("Clear Blocks", (player, menuOption) =>
        {
            IWasdMenu ConfirmMenu = WasdManager.CreateMenu("Confirm");

            ConfirmMenu.Add("NO - keep blocks", (player, menuOption) =>
            {
                Menu_Settings(player);
            });

            ConfirmMenu.Add("YES - remove blocks", (player, menuOption) =>
            {
                Commands.ClearBlocks(player);

                Menu_Settings(player);
            });

            WasdManager.OpenMainMenu(player, ConfirmMenu);
        });

        SettingsMenu.Add("Manage Builders", (player, menuOption) =>
        {
            IWasdMenu BuildersMenu = WasdManager.CreateMenu("Manage Builders");

            foreach (var target in Utilities.GetPlayers().Where(t => t.SteamID != player.SteamID && t.SteamID > 0))
            {
                BuildersMenu.Add(target.PlayerName, (player, menuOption) =>
                {
                    Commands.ManageBuilder(player, target.SteamID.ToString());
                });
            }

            WasdManager.OpenMainMenu(player, BuildersMenu);
        });

        WasdManager.OpenMainMenu(player, SettingsMenu);
    }

    /* Menu_Settings */
}