using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2ScreenMenuAPI;
using CS2ScreenMenuAPI.Internal;

public static partial class Menu
{
    public static class Screen
    {
        public static void Open(CCSPlayerController player)
        {
            ScreenMenu Menu = new("Block Maker", Instance);

            Menu.AddOption($"Block Commands", (player, option) =>
            {
                Menu_Commands(player);
            });

            Menu.AddOption($"Block Settings", (player, option) =>
            {
                Menu_BlockSettings(player);
            });

            Menu.AddOption("Build Settings", (player, option) =>
            {
                Menu_Settings(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        /* Menu_Commands */

        static void Menu_Commands(CCSPlayerController player)
        {
            ScreenMenu Menu = new("Block Commands", Instance);

            Menu.AddOption("Create", (player, option) =>
            {
                Commands.CreateBlock(player);
            });

            Menu.AddOption("Delete", (player, option) =>
            {
                Commands.DeleteBlock(player);
            });

            Menu.AddOption("Rotate", (player, option) =>
            {
                string[] options = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                PositionMenuOptions(player, options, true);
            });

            Menu.AddOption("Move", (player, option) =>
            {
                string[] options = { "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                PositionMenuOptions(player, options, false);
            });

            Menu.AddOption("Convert", (player, option) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.AddOption("Copy", (player, option) =>
            {
                Commands.CopyBlock(player);
            });

            Menu.AddOption("Lock", (player, option) =>
            {
                Commands.LockBlock(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void PositionMenuOptions(CCSPlayerController player, string[] options, bool rotate)
        {
            var playerData = Instance.playerData[player.Slot];

            float value = rotate ? playerData.RotationValue : playerData.PositionValue;
            string title = $"{(rotate ? "Rotate" : "Move")} Block ({value} Units)";

            ScreenMenu Menu = new(title, Instance);

            Menu.AddOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = rotate ? "Rotation" : "Position";
                Utils.PrintToChat(player, $"Write your desired number in the chat");
                PositionMenuOptions(player, options, rotate);
            });

            foreach (string input in options)
            {
                Menu.AddOption(input, (player, option) =>
                {
                    if (rotate) Commands.RotateBlock(player, input);
                    else Commands.PositionBlock(player, input);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        static void Menu_BlockSettings(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new("Block Settings", Instance);

            Menu.AddOption($"Type: {playerData.BlockType}", (player, option) =>
            {
                TypeMenuOptions(player);
            });

            Menu.AddOption($"Size: {playerData.BlockSize}", (player, option) =>
            {
                string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

                SizeMenuOptions(player, sizeValues);
            });

            Menu.AddOption($"Team: {playerData.BlockTeam}", (player, option) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, teamValues);
            });

            Menu.AddOption($"Transparency: {playerData.BlockTransparency}", (player, option) =>
            {
                TransparencyMenuOptions(player);
            });

            Menu.AddOption($"Color: {playerData.BlockColor}", (player, option) =>
            {
                ColorMenuOptions(player);
            });

            Menu.AddOption($"Properties", (player, option) =>
            {
                PropertiesMenuOptions(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void TypeMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Type ({playerData.BlockType})", Instance);

            var blockmodels = Files.Models.Props;

            foreach (var property in typeof(BlockModels).GetProperties())
            {
                var block = (BlockModel)property.GetValue(blockmodels)!;

                string blockName = block.Title;

                Menu.AddOption(blockName, (player, option) =>
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

            Menu.AddOption("Teleport", (player, option) =>
            {
                playerData.BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void GunTypeMenu(CCSPlayerController player, string gunType)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select {gunType}", Instance);

            if (WeaponList.Categories.ContainsKey(gunType))
            {
                var weaponsInCategory = WeaponList.Categories[gunType];

                foreach (var weaponID in weaponsInCategory)
                {
                    var weapon = WeaponList.Weapons.FirstOrDefault(w => w.Designer == weaponID);

                    if (weapon != null)
                    {
                        Menu.AddOption(weapon.Name, (player, option) =>
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

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Size ({playerData.BlockSize})", Instance);

            Menu.AddOption($"Pole: {(playerData.BlockPole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                SizeMenuOptions(player, sizeValues);
            });

            foreach (string sizeValue in sizeValues)
            {
                Menu.AddOption(sizeValue, (player, option) =>
                {
                    playerData.BlockSize = sizeValue;

                    Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Team ({playerData.BlockTeam})", Instance);

            foreach (string teamValue in teamValues)
            {
                Menu.AddOption(teamValue, (player, option) =>
                {
                    playerData.BlockTeam = teamValue;

                    Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void TransparencyMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Transparency ({playerData.BlockTransparency})", Instance);

            foreach (var value in Utils.AlphaMapping.Keys)
            {
                Menu.AddOption(value, (player, option) =>
                {
                    playerData.BlockTransparency = value;

                    Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                    Commands.TransparenyBlock(player, value);

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void ColorMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Color ({playerData.BlockColor})", Instance);

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.AddOption(color, (player, option) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player);
                });
            }

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void PropertiesMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var entity = player.GetBlockAimTarget();

            if (entity?.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            {
                Utils.PrintToChat(player, "Could not find a block to edit properties");
                Menu_BlockSettings(player);
                return;
            }

            if (Blocks.Props.TryGetValue(entity, out var block))
            {
                ScreenMenu Menu = new($"Properties ({block.Type})", Instance);

                playerData.ChatInput = "";
                playerData.PropertyEntity.Clear();

                var properties = block.Properties;

                PropertyMenuOption(Menu, "Reset", 1, player, entity);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity);

                MenuAPI.OpenMenu(Instance, player, Menu);
            }
        }

        static void PropertyMenuOption(ScreenMenu Menu, string property, float value, CCSPlayerController player, CBaseProp entity)
        {
            var playerData = Instance.playerData[player.Slot];

            if (value != 0)
            {
                string title = $": {value}";

                if (property == "Reset")
                    title = "";

                if (property == "OnTop")
                    title = Blocks.Props[entity].Properties.OnTop ? ": Enabled" : ": Disabled";

                Menu.AddOption($"{property}{title}", (player, option) =>
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
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new("Build Settings", Instance);

            Menu.AddOption("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.BuildMode(player);

                Menu_Settings(player);
            });

            Menu.AddOption("Godmode: " + (playerData.Godmode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Godmode(player);

                Menu_Settings(player);
            });

            Menu.AddOption("Noclip: " + (playerData.Noclip ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Noclip(player);

                Menu_Settings(player);
            });

            Menu.AddOption($"Grid Settings", (player, option) =>
            {
                GridMenuOptions(player);
            });

            Menu.AddOption("Save Blocks", (player, option) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player);
            });

            Menu.AddOption("Clear Blocks", (player, option) =>
            {
                ScreenMenu ConfirmMenu = new("Confirm", Instance);

                ConfirmMenu.AddOption("NO - keep blocks", (player, option) =>
                {
                    Menu_Settings(player);
                });

                ConfirmMenu.AddOption("YES - remove blocks", (player, option) =>
                {
                    Commands.ClearBlocks(player);

                    Menu_Settings(player);
                });

                MenuAPI.OpenMenu(Instance, player, ConfirmMenu);
            });

            Menu.AddOption("Manage Builders", (player, option) =>
            {
                ScreenMenu BuildersMenu = new("Manage Builders", Instance);

                foreach (var target in Utilities.GetPlayers().Where(t => t.SteamID != player.SteamID && t.SteamID > 0))
                {
                    BuildersMenu.AddOption(target.PlayerName, (player, option) =>
                    {
                        Commands.ManageBuilder(player, target.SteamID.ToString());
                    });
                }

                MenuAPI.OpenMenu(Instance, player, BuildersMenu);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        static void GridMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Grid Options ({playerData.GridValue} Units)", Instance);

            Menu.AddOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = "Grid";

                Utils.PrintToChat(player, $"Write your desired number in the chat");
                GridMenuOptions(player);
            });

            Menu.AddOption($"Grid: " + (playerData.Grid ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Grid(player, "");

                GridMenuOptions(player);
            });

            Menu.AddOption($"Snap: " + (playerData.Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                GridMenuOptions(player);
            });

            MenuAPI.OpenMenu(Instance, player, Menu);
        }

        /* Menu_Settings */
    }
}