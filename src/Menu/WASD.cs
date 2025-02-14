using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using WASDSharedAPI;

public static partial class Menu
{
    public static class WASD
    {
        public static IWasdMenuManager WasdManager = new PluginCapability<IWasdMenuManager>("wasdmenu:manager").Get()!;

        private static Plugin Instance = Plugin.Instance;
        private static Dictionary<int, PlayerData> playerData = Instance.playerData;

        public static void Open(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu("Block Maker");

            Menu.Add($"Block Commands", (player, menuOption) =>
            {
                Menu_Commands(player);
            });

            Menu.Add($"Block Settings", (player, menuOption) =>
            {
                Menu_BlockSettings(player);
            });

            Menu.Add("Build Settings", (player, menuOption) =>
            {
                Menu_Settings(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        /* Menu_Commands */

        private static void Menu_Commands(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu("Block Commands");

            Menu.Add("Create", (player, menuOption) =>
            {
                Commands.CreateBlock(player);
            });

            Menu.Add("Delete", (player, menuOption) =>
            {
                Commands.DeleteBlock(player);
            });

            Menu.Add("Rotate", (player, menuOption) =>
            {
                float[] rotateValues = Instance.Config.Settings.Building.RotationValues;
                string[] rotateOptions = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });

            Menu.Add("Convert", (player, menuOption) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.Add("Copy", (player, menuOption) =>
            {
                Commands.CopyBlock(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Rotate Block ({playerData[player.Slot].RotationValue} Units)");

            Menu.Add($"Select Units", (player, option) =>
            {
                RotateValuesMenuOptions(player, rotateOptions, rotateValues);
            });

            foreach (string rotateOption in rotateOptions)
            {
                Menu.Add(rotateOption, (player, option) =>
                {
                    Commands.RotateBlock(player, rotateOption);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void RotateValuesMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Rotate Values");

            foreach (float rotateValueOption in rotateValues)
            {
                Menu.Add(rotateValueOption.ToString() + " Units", (player, option) =>
                {
                    playerData[player.Slot].RotationValue = rotateValueOption;

                    Utils.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                    RotateMenuOptions(player, rotateOptions, rotateValues);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        private static void Menu_BlockSettings(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu("Block Settings");

            Menu.Add($"Type: {playerData[player.Slot].BlockType}", (player, menuOption) =>
            {
                TypeMenuOptions(player);
            });

            Menu.Add($"Size: {playerData[player.Slot].BlockSize}", (player, menuOption) =>
            {
                string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

                SizeMenuOptions(player, sizeValues);
            });

            Menu.Add($"Team: {playerData[player.Slot].BlockTeam}", (player, menuOption) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, teamValues);
            });

            Menu.Add($"Transparency: {playerData[player.Slot].BlockTransparency}", (player, menuOption) =>
            {
                TransparencyMenuOptions(player);
            });

            Menu.Add($"Color: {playerData[player.Slot].BlockColor}", (player, menuOption) =>
            {
                ColorMenuOptions(player);
            });

            Menu.Add($"Properties", (player, menuOption) =>
            {
                PropertiesMenuOptions(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void TypeMenuOptions(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Type ({playerData[player.Slot].BlockType})");

            var blockmodels = Files.Models.Props;

            foreach (var property in typeof(BlockModels).GetProperties())
            {
                var block = (BlockModel)property.GetValue(blockmodels)!;

                string blockName = block.Title;

                Menu.Add(blockName, (player, menuOption) =>
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

            Menu.Add("Teleport", (player, menuOption) =>
            {
                playerData[player.Slot].BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void GunTypeMenu(CCSPlayerController player, string gunType)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select {gunType}");

            if (WeaponList.Categories.ContainsKey(gunType))
            {
                var weaponsInCategory = WeaponList.Categories[gunType];

                foreach (var weaponID in weaponsInCategory)
                {
                    var weapon = WeaponList.Weapons.FirstOrDefault(w => w.Designer == weaponID);

                    if (weapon != null)
                    {
                        Menu.Add(weapon.Name, (player, menuOption) =>
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

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Size ({playerData[player.Slot].BlockSize})");

            Menu.Add($"Pole: {(playerData[player.Slot].Pole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                SizeMenuOptions(player, sizeValues);
            });

            foreach (string sizeValue in sizeValues)
            {
                Menu.Add(sizeValue, (player, option) =>
                {
                    playerData[player.Slot].BlockSize = sizeValue;

                    Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Team ({playerData[player.Slot].BlockTeam})");

            foreach (string teamValue in teamValues)
            {
                Menu.Add(teamValue, (player, option) =>
                {
                    playerData[player.Slot].BlockTeam = teamValue;

                    Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void TransparencyMenuOptions(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Transparency ({playerData[player.Slot].BlockTransparency})");

            foreach (var value in Utils.AlphaMapping.Keys)
            {
                Menu.Add(value, (player, menuOption) =>
                {
                    playerData[player.Slot].BlockTransparency = value;

                    Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                    Commands.TransparenyBlock(player, value);

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void ColorMenuOptions(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Color ({playerData[player.Slot].BlockColor})");

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.Add(color, (player, menuOption) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
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
                IWasdMenu Menu = WasdManager.CreateMenu($"Properties ({block.Name})");

                var properties = block.Properties;

                PropertyMenuOption(Menu, "Reset", 1, player, entity);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity);

                WasdManager.OpenMainMenu(player, Menu);
            }
        }

        private static void PropertyMenuOption(IWasdMenu Menu, string property, float value, CCSPlayerController player, CBaseProp entity)
        {
            if (value != 0)
            {
                string title = $": {value}";

                if (property == "Reset")
                    title = "";

                if (property == "OnTop")
                    title = Blocks.Props[entity].Properties.OnTop ? ": Enabled" : ": Disabled";

                Menu.Add($"{property}{title}", (player, menuOption) =>
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
            IWasdMenu Menu = WasdManager.CreateMenu("Build Settings");

            Menu.Add("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.BuildMode(player);

                Menu_Settings(player);
            });

            Menu.Add("Godmode: " + (playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.Godmode(player);

                Menu_Settings(player);
            });

            Menu.Add("Noclip: " + (playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.Noclip(player);

                Menu_Settings(player);
            });

            Menu.Add($"Grid: {playerData[player.Slot].GridValue} Units", (player, menuOption) =>
            {
                float[] gridValues = Instance.Config.Settings.Building.GridValues;

                GridMenuOptions(player, gridValues);
            });

            Menu.Add("Save Blocks", (player, menuOption) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player);
            });

            Menu.Add("Clear Blocks", (player, menuOption) =>
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

            Menu.Add("Manage Builders", (player, menuOption) =>
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

            WasdManager.OpenMainMenu(player, Menu);
        }

        private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Grid Options ({playerData[player.Slot].GridValue} Units)");

            Menu.Add($"Grid: " + (playerData[player.Slot].Grid ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Grid(player, "");

                GridMenuOptions(player, gridValues);
            });

            Menu.Add($"Snap: " + (playerData[player.Slot].Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                GridMenuOptions(player, gridValues);
            });

            foreach (float gridValue in gridValues)
            {
                Menu.Add(gridValue.ToString() + " Units", (player, option) =>
                {
                    Commands.Grid(player, gridValue.ToString());

                    GridMenuOptions(player, gridValues);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        /* Menu_Settings */
    }
}