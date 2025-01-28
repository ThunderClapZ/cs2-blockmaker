using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

public static class MenuHTML
{
    private static Plugin Instance = Plugin.Instance;
    private static Dictionary<int, PlayerData> playerData = Instance.playerData;

    public static void OpenMenu(CCSPlayerController player)
    {
        CenterHtmlMenu MainMenu = new("Block Maker", Instance);

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

        MenuManager.OpenCenterHtmlMenu(Instance, player, MainMenu);
    }

    /* Menu_Commands */

    private static void Menu_Commands(CCSPlayerController player)
    {
        CenterHtmlMenu CommandsMenu = new("Block Commands", Instance);

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
            float[] rotateValues = Instance.Config.Settings.Building.RotationValues;
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

        MenuManager.OpenCenterHtmlMenu(Instance, player, CommandsMenu);
    }

    private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        CenterHtmlMenu RotateMenu = new($"Rotate Block ({playerData[player.Slot].RotationValue} Units)", Instance);

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

        MenuManager.OpenCenterHtmlMenu(Instance, player, RotateMenu);
    }

    private static void RotateValuesMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
    {
        CenterHtmlMenu RotateValuesMenu = new($"Rotate Values", Instance);

        foreach (float rotateValueOption in rotateValues)
        {
            RotateValuesMenu.AddMenuOption(rotateValueOption.ToString() + " Units", (player, option) =>
            {
                playerData[player.Slot].RotationValue = rotateValueOption;

                Utils.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, RotateValuesMenu);
    }

    /* Menu_Commands */

    /* Menu_BlockSettings */

    private static void Menu_BlockSettings(CCSPlayerController player)
    {
        CenterHtmlMenu BlockMenu = new("Block Settings", Instance);

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
            float[] gridValues = Instance.Config.Settings.Building.GridValues;

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

        MenuManager.OpenCenterHtmlMenu(Instance, player, BlockMenu);
    }

    private static void TypeMenuOptions(CCSPlayerController player)
    {
        CenterHtmlMenu TypeMenu = new($"Select Type ({playerData[player.Slot].BlockType})", Instance);

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
        MenuManager.OpenCenterHtmlMenu(Instance, player, TypeMenu);
    }

    private static void GunTypeMenu(CCSPlayerController player, string gunType)
    {
        CenterHtmlMenu GunTypeMenu = new($"Select {gunType}", Instance);

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

        MenuManager.OpenCenterHtmlMenu(Instance, player, GunTypeMenu);
    }

    private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
    {
        CenterHtmlMenu SizeMenu = new($"Select Size ({playerData[player.Slot].BlockSize})", Instance);

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

        MenuManager.OpenCenterHtmlMenu(Instance, player, SizeMenu);
    }

    private static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
    {
        CenterHtmlMenu TeamMenu = new($"Select Team ({playerData[player.Slot].BlockTeam})", Instance);

        foreach (string teamValue in teamValues)
        {
            TeamMenu.AddMenuOption(teamValue, (player, option) =>
            {
                playerData[player.Slot].BlockTeam = teamValue;

                Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, TeamMenu);
    }

    private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
    {
        CenterHtmlMenu GridMenu = new($"Select Grid ({(playerData[player.Slot].Grid ? "ON" : "OFF")} - {playerData[player.Slot].GridValue})", Instance);

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

        MenuManager.OpenCenterHtmlMenu(Instance, player, GridMenu);
    }

    private static void TransparencyMenuOptions(CCSPlayerController player)
    {
        CenterHtmlMenu TransparencyMenu = new($"Select Transparency ({playerData[player.Slot].BlockTransparency})", Instance);

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

        MenuManager.OpenCenterHtmlMenu(Instance, player, TransparencyMenu);
    }

    private static void ColorMenuOptions(CCSPlayerController player)
    {
        CenterHtmlMenu ColorMenu = new($"Select Color ({playerData[player.Slot].BlockColor})", Instance);

        foreach (var color in Utils.ColorMapping.Keys)
        {
            ColorMenu.AddMenuOption(color, (player, menuOption) =>
            {
                Commands.BlockColor(player, color);

                Menu_BlockSettings(player);
            });
        }

        MenuManager.OpenCenterHtmlMenu(Instance, player, ColorMenu);
    }

    /* Menu_BlockSettings */

    /* Menu_Settings */

    private static void Menu_Settings(CCSPlayerController player)
    {
        CenterHtmlMenu SettingsMenu = new("Settings", Instance);

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
            CenterHtmlMenu ConfirmMenu = new("Confirm", Instance);

            ConfirmMenu.AddMenuOption("NO - keep blocks", (player, menuOption) =>
            {
                Menu_Settings(player);
            });

            ConfirmMenu.AddMenuOption("YES - remove blocks", (player, menuOption) =>
            {
                Commands.ClearBlocks(player);

                Menu_Settings(player);
            });

            MenuManager.OpenCenterHtmlMenu(Instance, player, ConfirmMenu);
        });

        SettingsMenu.AddMenuOption("Manage Builders", (player, menuOption) =>
        {
            CenterHtmlMenu BuildersMenu = new("Manage Builders", Instance);

            foreach (var target in Utilities.GetPlayers().Where(t => t.SteamID != player.SteamID && t.SteamID > 0))
            {
                BuildersMenu.AddMenuOption(target.PlayerName, (player, menuOption) =>
                {
                    Commands.ManageBuilder(player, target.SteamID.ToString());
                });
            }

            MenuManager.OpenCenterHtmlMenu(Instance, player, BuildersMenu);
        });

        MenuManager.OpenCenterHtmlMenu(Instance, player, SettingsMenu);
    }

    /* Menu_Settings */
}