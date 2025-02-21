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

        public static void Open(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu("Block Maker");

            Menu.Add($"Block Commands", (player, option) =>
            {
                Menu_Commands(player);
            });

            Menu.Add($"Block Settings", (player, option) =>
            {
                Menu_BlockSettings(player);
            });

            Menu.Add("Build Settings", (player, option) =>
            {
                Menu_Settings(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        /* Menu_Commands */

        static void Menu_Commands(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu("Block Commands");

            Menu.Add("Create", (player, option) =>
            {
                Commands.CreateBlock(player);
            });

            Menu.Add("Delete", (player, option) =>
            {
                Commands.DeleteBlock(player);
            });

            Menu.Add("Rotate", (player, option) =>
            {
                string[] options = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                PositionMenuOptions(player, options, true);
            });

            Menu.Add("Move", (player, option) =>
            {
                string[] options = { "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                PositionMenuOptions(player, options, false);
            });

            Menu.Add("Convert", (player, option) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.Add("Copy", (player, option) =>
            {
                Commands.CopyBlock(player);
            });

            Menu.Add("Lock", (player, option) =>
            {
                Commands.LockBlock(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void PositionMenuOptions(CCSPlayerController player, string[] options, bool rotate)
        {
            float value = rotate ? playerData.RotationValue : playerData.PositionValue;
            string title = $"{(rotate ? "Rotate" : "Move")} Block ({value} Units)";

            IWasdMenu Menu = WasdManager.CreateMenu(title);

            Menu.Add($"Select Units", (player, option) =>
            {
                playerData.ChatInput = rotate ? "Rotation" : "Position";
                Utils.PrintToChat(player, $"Write your desired number in the chat");
                PositionMenuOptions(player, options, rotate);
            });

            foreach (string input in options)
            {
                Menu.Add(input, (player, option) =>
                {
                    if (rotate) Commands.RotateBlock(player, input);
                    else Commands.PositionBlock(player, input);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        static void Menu_BlockSettings(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu("Block Settings");

            Menu.Add($"Type: {playerData.BlockType}", (player, option) =>
            {
                TypeMenuOptions(player);
            });

            Menu.Add($"Size: {playerData.BlockSize}", (player, option) =>
            {
                string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

                SizeMenuOptions(player, sizeValues);
            });

            Menu.Add($"Team: {playerData.BlockTeam}", (player, option) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, teamValues);
            });

            Menu.Add($"Transparency: {playerData.BlockTransparency}", (player, option) =>
            {
                TransparencyMenuOptions(player);
            });

            Menu.Add($"Color: {playerData.BlockColor}", (player, option) =>
            {
                ColorMenuOptions(player);
            });

            Menu.Add($"Properties", (player, option) =>
            {
                PropertiesMenuOptions(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void TypeMenuOptions(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Type ({playerData.BlockType})");

            var blockmodels = Files.Models.Props;

            foreach (var property in typeof(BlockModels).GetProperties())
            {
                var block = (BlockModel)property.GetValue(blockmodels)!;

                string blockName = block.Title;

                Menu.Add(blockName, (player, option) =>
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

            Menu.Add("Teleport", (player, option) =>
            {
                playerData.BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void GunTypeMenu(CCSPlayerController player, string gunType)
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
                        Menu.Add(weapon.Name, (player, option) =>
                        {
                            foreach (var property in typeof(BlockModels).GetProperties())
                            {
                                var model = (BlockModel)property.GetValue(Files.Models.Props)!;

                                if (string.Equals(model.Title, gunType, StringComparison.OrdinalIgnoreCase))
                                {
                                    playerData.BlockType = $"{model.Title}.{weapon.Name}";
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

        static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Size ({playerData.BlockSize})");

            Menu.Add($"Pole: {(playerData.BlockPole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                SizeMenuOptions(player, sizeValues);
            });

            foreach (string sizeValue in sizeValues)
            {
                Menu.Add(sizeValue, (player, option) =>
                {
                    playerData.BlockSize = sizeValue;

                    Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Team ({playerData.BlockTeam})");

            foreach (string teamValue in teamValues)
            {
                Menu.Add(teamValue, (player, option) =>
                {
                    playerData.BlockTeam = teamValue;

                    Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void TransparencyMenuOptions(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Transparency ({playerData.BlockTransparency})");

            foreach (var value in Utils.AlphaMapping.Keys)
            {
                Menu.Add(value, (player, option) =>
                {
                    playerData.BlockTransparency = value;

                    Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                    Commands.TransparenyBlock(player, value);

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void ColorMenuOptions(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Select Color ({playerData.BlockColor})");

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.Add(color, (player, option) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player);
                });
            }

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void PropertiesMenuOptions(CCSPlayerController player)
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
                IWasdMenu Menu = WasdManager.CreateMenu($"Properties ({block.Type})");

                playerData.ChatInput = "";
                playerData.PropertyEntity.Clear();

                var properties = block.Properties;

                PropertyMenuOption(Menu, "Reset", 1, player, entity);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity);

                WasdManager.OpenMainMenu(player, Menu);
            }
        }

        static void PropertyMenuOption(IWasdMenu Menu, string property, float value, CCSPlayerController player, CBaseProp entity)
        {
            if (value != 0)
            {
                string title = $": {value}";

                if (property == "Reset")
                    title = "";

                if (property == "OnTop")
                    title = Blocks.Props[entity].Properties.OnTop ? ": Enabled" : ": Disabled";

                Menu.Add($"{property}{title}", (player, option) =>
                {
                    playerData.ChatInput = property;
                    playerData.PropertyEntity[property] = entity;

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

        static void Menu_Settings(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu("Build Settings");

            Menu.Add("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.BuildMode(player);

                Menu_Settings(player);
            });

            Menu.Add("Godmode: " + (playerData.Godmode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Godmode(player);

                Menu_Settings(player);
            });

            Menu.Add("Noclip: " + (playerData.Noclip ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Noclip(player);

                Menu_Settings(player);
            });

            Menu.Add($"Grid Settings", (player, option) =>
            {
                GridMenuOptions(player);
            });

            Menu.Add("Save Blocks", (player, option) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player);
            });

            Menu.Add("Clear Blocks", (player, option) =>
            {
                IWasdMenu ConfirmMenu = WasdManager.CreateMenu("Confirm");

                ConfirmMenu.Add("NO - keep blocks", (player, option) =>
                {
                    Menu_Settings(player);
                });

                ConfirmMenu.Add("YES - remove blocks", (player, option) =>
                {
                    Commands.ClearBlocks(player);

                    Menu_Settings(player);
                });

                WasdManager.OpenMainMenu(player, ConfirmMenu);
            });

            Menu.Add("Manage Builders", (player, option) =>
            {
                IWasdMenu BuildersMenu = WasdManager.CreateMenu("Manage Builders");

                foreach (var target in Utilities.GetPlayers().Where(t => t.SteamID != player.SteamID && t.SteamID > 0))
                {
                    BuildersMenu.Add(target.PlayerName, (player, option) =>
                    {
                        Commands.ManageBuilder(player, target.SteamID.ToString());
                    });
                }

                WasdManager.OpenMainMenu(player, BuildersMenu);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        static void GridMenuOptions(CCSPlayerController player)
        {
            IWasdMenu Menu = WasdManager.CreateMenu($"Grid Options ({playerData.GridValue} Units)");

            Menu.Add($"Select Units", (player, option) =>
            {
                playerData.ChatInput = "Grid";

                Utils.PrintToChat(player, $"Write your desired number in the chat");
                GridMenuOptions(player);
            });

            Menu.Add($"Grid: " + (playerData.Grid ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Grid(player, "");

                GridMenuOptions(player);
            });

            Menu.Add($"Snap: " + (playerData.Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                GridMenuOptions(player);
            });

            WasdManager.OpenMainMenu(player, Menu);
        }

        /* Menu_Settings */
    }
}