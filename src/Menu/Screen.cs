using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2ScreenMenuAPI;

public static partial class Menu
{
    public static class Screen
    {
        private static Plugin Instance = Plugin.Instance;
        private static Dictionary<int, PlayerData> playerData = Instance.playerData;

        public static void Open(CCSPlayerController player)
        {
            ScreenMenu Menu = new ScreenMenu("Block Maker", Instance);

            Menu.AddOption($"Block Commands", (player, menuOption) =>
            {
                Menu_Commands(player);
            });

            Menu.AddOption($"Block Settings", (player, menuOption) =>
            {
                Menu_BlockSettings(player);
            });

            Menu.AddOption("Build Settings", (player, menuOption) =>
            {
                Menu_Settings(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        /* Menu_Commands */

        private static void Menu_Commands(CCSPlayerController player)
        {
            ScreenMenu Menu = new ScreenMenu("Block Commands", Instance);

            Menu.AddOption("Create", (player, menuOption) =>
            {
                Commands.CreateBlock(player);
            });

            Menu.AddOption("Delete", (player, menuOption) =>
            {
                Commands.DeleteBlock(player);
            });

            Menu.AddOption("Rotate", (player, menuOption) =>
            {
                float[] rotateValues = Instance.Config.Settings.Building.RotationValues;
                string[] rotateOptions = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });

            Menu.AddOption("Convert", (player, menuOption) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.AddOption("Copy", (player, menuOption) =>
            {
                Commands.CopyBlock(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
        {
            ScreenMenu Menu = new ScreenMenu($"Rotate Block ({playerData[player.Slot].RotationValue} Units)", Instance);

            Menu.AddOption($"Select Units", (player, option) =>
            {
                RotateValuesMenuOptions(player, rotateOptions, rotateValues);
            });

            foreach (string rotateOption in rotateOptions)
            {
                Menu.AddOption(rotateOption, (player, option) =>
                {
                    Commands.RotateBlock(player, rotateOption);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void RotateValuesMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
        {
            ScreenMenu Menu = new ScreenMenu($"Rotate Values", Instance);

            foreach (float rotateValueOption in rotateValues)
            {
                Menu.AddOption(rotateValueOption.ToString() + " Units", (player, option) =>
                {
                    playerData[player.Slot].RotationValue = rotateValueOption;

                    Utils.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                    RotateMenuOptions(player, rotateOptions, rotateValues);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        private static void Menu_BlockSettings(CCSPlayerController player)
        {
            ScreenMenu Menu = new ScreenMenu("Block Settings", Instance);

            Menu.AddOption($"Type: {playerData[player.Slot].BlockType}", (player, menuOption) =>
            {
                TypeMenuOptions(player);
            });

            Menu.AddOption($"Size: {playerData[player.Slot].BlockSize}", (player, menuOption) =>
            {
                string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

                SizeMenuOptions(player, sizeValues);
            });

            Menu.AddOption($"Team: {playerData[player.Slot].BlockTeam}", (player, menuOption) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, teamValues);
            });

            Menu.AddOption($"Transparency: {playerData[player.Slot].BlockTransparency}", (player, menuOption) =>
            {
                TransparencyMenuOptions(player);
            });

            Menu.AddOption($"Color: {playerData[player.Slot].BlockColor}", (player, menuOption) =>
            {
                ColorMenuOptions(player);
            });

            Menu.AddOption($"Properties", (player, menuOption) =>
            {
                PropertiesMenuOptions(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void TypeMenuOptions(CCSPlayerController player)
        {
            ScreenMenu Menu = new ScreenMenu($"Select Type ({playerData[player.Slot].BlockType})", Instance);

            var blockmodels = Files.Models.Props;

            foreach (var property in typeof(BlockModels).GetProperties())
            {
                var block = (BlockModel)property.GetValue(blockmodels)!;

                string blockName = block.Title;

                Menu.AddOption(blockName, (player, menuOption) =>
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

            Menu.AddOption("Teleport", (player, menuOption) =>
            {
                playerData[player.Slot].BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void GunTypeMenu(CCSPlayerController player, string gunType)
        {
            ScreenMenu Menu = new ScreenMenu($"Select {gunType}", Instance);

            if (WeaponList.Categories.ContainsKey(gunType))
            {
                var weaponsInCategory = WeaponList.Categories[gunType];

                foreach (var weaponID in weaponsInCategory)
                {
                    var weapon = WeaponList.Weapons.FirstOrDefault(w => w.Designer == weaponID);

                    if (weapon != null)
                    {
                        Menu.AddOption(weapon.Name, (player, menuOption) =>
                        {
                            foreach (var property in typeof(BlockModels).GetProperties())
                            {
                                var model = (BlockModel)property.GetValue(Files.Models.Props)!;

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

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
        {
            ScreenMenu Menu = new ScreenMenu($"Select Size ({playerData[player.Slot].BlockSize})", Instance);

            Menu.AddOption($"Pole: {(playerData[player.Slot].Pole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                SizeMenuOptions(player, sizeValues);
            });

            foreach (string sizeValue in sizeValues)
            {
                Menu.AddOption(sizeValue, (player, option) =>
                {
                    playerData[player.Slot].BlockSize = sizeValue;

                    Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
        {
            ScreenMenu Menu = new ScreenMenu($"Select Team ({playerData[player.Slot].BlockTeam})", Instance);

            foreach (string teamValue in teamValues)
            {
                Menu.AddOption(teamValue, (player, option) =>
                {
                    playerData[player.Slot].BlockTeam = teamValue;

                    Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void TransparencyMenuOptions(CCSPlayerController player)
        {
            ScreenMenu Menu = new ScreenMenu($"Select Transparency ({playerData[player.Slot].BlockTransparency})", Instance);

            foreach (var value in Utils.AlphaMapping.Keys)
            {
                Menu.AddOption(value, (player, menuOption) =>
                {
                    playerData[player.Slot].BlockTransparency = value;

                    Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                    Commands.TransparenyBlock(player, value);

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void ColorMenuOptions(CCSPlayerController player)
        {
            ScreenMenu Menu = new ScreenMenu($"Select Color ({playerData[player.Slot].BlockColor})", Instance);

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.AddOption(color, (player, menuOption) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void PropertiesMenuOptions(CCSPlayerController player)
        {
            var entity = player.GetBlockAimTarget();

            if (entity?.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            {
                Utils.PrintToChat(player, "Could not find a block to edit properties");
                Menu_BlockSettings(player);
                return;
            }

            if (Blocks.Props.TryGetValue(entity, out var block))
            {
                ScreenMenu Menu = new ScreenMenu($"Properties ({block.Name})", Instance);

                var properties = block.Properties;

                PropertyMenuOption(Menu, "Reset", 1, player, entity);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity);

                MenuAPI.OpenMenu(Instance, player, Menu);
            }
        }

        private static void PropertyMenuOption(ScreenMenu Menu, string property, float value, CCSPlayerController player, CBaseProp entity)
        {
            if (value != 0)
            {
                string title = $": {value}";

                if (property == "Reset")
                    title = "";

                if (property == "OnTop")
                    title = Blocks.Props[entity].Properties.OnTop ? ": Enabled" : ": Disabled";

                Menu.AddOption($"{property}{title}", (player, menuOption) =>
                {
                    playerData[player.Slot].PropertyType = property;
                    playerData[player.Slot].PropertyEntity[property] = entity;

                    if (property == "Reset" || property == "OnTop")
                    {
                        Commands.Properties(player, property, property);
                        Menu_BlockSettings(player);
                    }
                    else
                    {
                        Utils.PrintToChat(player, $"Write your desired number in the chat");
                        Menu_BlockSettings(player);
                    }
                });
            }
        }

        /* Menu_BlockSettings */

        /* Menu_Settings */

        private static void Menu_Settings(CCSPlayerController player)
        {
            ScreenMenu Menu = new ScreenMenu("Build Settings", Instance);

            Menu.AddOption("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.BuildMode(player);

                Menu_Settings(player);
            });

            Menu.AddOption("Godmode: " + (playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.Godmode(player);

                Menu_Settings(player);
            });

            Menu.AddOption("Noclip: " + (playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.Noclip(player);

                Menu_Settings(player);
            });

            Menu.AddOption($"Grid: {playerData[player.Slot].GridValue} Units", (player, menuOption) =>
            {
                float[] gridValues = Instance.Config.Settings.Building.GridValues;

                GridMenuOptions(player, gridValues);
            });

            Menu.AddOption("Save Blocks", (player, menuOption) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player);
            });

            Menu.AddOption("Clear Blocks", (player, menuOption) =>
            {
                ScreenMenu ConfirmMenu = new ScreenMenu("Confirm", Instance);

                ConfirmMenu.AddOption("NO - keep blocks", (player, menuOption) =>
                {
                    Menu_Settings(player);
                });

                ConfirmMenu.AddOption("YES - remove blocks", (player, menuOption) =>
                {
                    Commands.ClearBlocks(player);

                    Menu_Settings(player);
                });

                MenuAPI.OpenMenu(Instance, player, ConfirmMenu);
            });

            Menu.AddOption("Manage Builders", (player, menuOption) =>
            {
                ScreenMenu BuildersMenu = new ScreenMenu("Manage Builders", Instance);

                foreach (var target in Utilities.GetPlayers().Where(t => t.SteamID != player.SteamID && t.SteamID > 0))
                {
                    BuildersMenu.AddOption(target.PlayerName, (player, menuOption) =>
                    {
                        Commands.ManageBuilder(player, target.SteamID.ToString());
                    });
                }

                MenuAPI.OpenMenu(Instance, player, BuildersMenu);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
        {
            ScreenMenu Menu = new ScreenMenu($"Grid Options ({playerData[player.Slot].GridValue} Units)", Instance);

            Menu.AddOption($"Grid: " + (playerData[player.Slot].Grid ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Grid(player, "");

                GridMenuOptions(player, gridValues);
            });

            Menu.AddOption($"Snap: " + (playerData[player.Slot].Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                GridMenuOptions(player, gridValues);
            });

            foreach (float gridValue in gridValues)
            {
                Menu.AddOption(gridValue.ToString() + " Units", (player, option) =>
                {
                    Commands.Grid(player, gridValue.ToString());

                    GridMenuOptions(player, gridValues);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        /* Menu_Settings */
    }
}