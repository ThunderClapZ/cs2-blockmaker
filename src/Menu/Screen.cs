using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
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
                Menu_Commands(player, Menu);
            });

            Menu.AddOption($"Block Settings", (player, option) =>
            {
                Menu_BlockSettings(player, Menu);
            });

            Menu.AddOption("Build Settings", (player, option) =>
            {
                Menu_Settings(player, Menu);
            });

            Menu.Open(player);
        }

        /* Menu_Commands */

        static void Menu_Commands(CCSPlayerController player, ScreenMenu parent)
        {
            ScreenMenu Menu = new("Block Commands", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

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

                PositionMenuOptions(player, options, true, parent);
            });

            Menu.AddOption("Move", (player, option) =>
            {
                string[] options = { "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                PositionMenuOptions(player, options, false, parent);
            });

            Menu.AddOption("Copy", (player, option) =>
            {
                Commands.CopyBlock(player);
            });

            Menu.AddOption("Lock", (player, option) =>
            {
                Commands.LockBlock(player);
            });

            Menu.AddOption("Convert", (player, option) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.Open(player);
        }

        static void PositionMenuOptions(CCSPlayerController player, string[] options, bool rotate, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            float value = rotate ? playerData.RotationValue : playerData.PositionValue;
            string title = $"{(rotate ? "Rotate" : "Move")} Block ({value} Units)";

            ScreenMenu Menu = new(title, Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            Menu.AddOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = rotate ? "Rotation" : "Position";
                Utils.PrintToChat(player, $"Write your desired number in the chat");
                PositionMenuOptions(player, options, rotate, parent);
            });

            foreach (string input in options)
            {
                Menu.AddOption(input, (player, option) =>
                {
                    if (rotate) Commands.RotateBlock(player, input);
                    else Commands.PositionBlock(player, input);
                });
            }

            Menu.Open(player);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        static void Menu_BlockSettings(CCSPlayerController player, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new("Block Settings", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            Menu.AddOption($"Type: {playerData.BlockType}", (player, option) =>
            {
                TypeMenuOptions(player, parent);
            });

            Menu.AddOption($"Size: {playerData.BlockSize}", (player, option) =>
            {
                var sizeValues = Instance.Config.Settings.Building.BlockSizes.ToArray();
                int currentIndex = Array.FindIndex(sizeValues, s => s.Title == playerData.BlockSize);
                int nextIndex = (currentIndex + 1) % sizeValues.Length;
                playerData.BlockSize = sizeValues[nextIndex].Title;

                Menu_BlockSettings(player, parent);
            });

            Menu.AddOption($"Pole: {(playerData.BlockPole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                Menu_BlockSettings(player, parent);
            });

            Menu.AddOption($"Team: {playerData.BlockTeam}", (player, option) =>
            {
                string[] teamValues = { "Both", "T", "CT" };
                int currentIndex = Array.IndexOf(teamValues, playerData.BlockTeam);
                int nextIndex = (currentIndex + 1) % teamValues.Length;
                playerData.BlockTeam = teamValues[nextIndex];
                Commands.TeamBlock(player, teamValues[nextIndex]);

                Menu_BlockSettings(player, parent);
            });

            Menu.AddOption($"Properties", (player, option) =>
            {
                var entity = player.GetBlockAimTarget();

                if (entity?.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
                {
                    Utils.PrintToChat(player, "Could not find a block to edit properties");
                    Menu_BlockSettings(player, parent);
                    return;
                }

                playerData.ChatInput = "";
                playerData.PropertyEntity.Clear();

                PropertiesMenuOptions(player, entity, parent);
            });

            Menu.AddOption($"Transparency: {playerData.BlockTransparency}", (player, option) =>
            {
                TransparencyMenuOptions(player, parent);
            });

            Menu.AddOption($"Color: {playerData.BlockColor}", (player, option) =>
            {
                ColorMenuOptions(player, parent);
            });

            Menu.Open(player);
        }

        static void TypeMenuOptions(CCSPlayerController player, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Type ({playerData.BlockType})", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            var blockmodels = Files.Models.Props;

            foreach (var block in blockmodels.GetAllBlocks())
            {
                string blockName = block.Title;

                Menu.AddOption(blockName, (player, option) =>
                {
                    if (blockName == blockmodels.Pistol.Title ||
                        blockName == blockmodels.Sniper.Title ||
                        blockName == blockmodels.Rifle.Title ||
                        blockName == blockmodels.SMG.Title ||
                        blockName == blockmodels.ShotgunHeavy.Title
                    )
                    {
                        GunTypeMenu(player, block.Title, parent);
                        return;
                    }

                    Commands.BlockType(player, blockName);

                    Menu_BlockSettings(player, parent);
                });
            }

            Menu.AddOption("Teleport", (player, option) =>
            {
                playerData.BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player, parent);
            });

            Menu.Open(player);
        }

        static void GunTypeMenu(CCSPlayerController player, string gunType, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select {gunType}", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

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
                            var blockModels = Files.Models.Props;
                            foreach (var model in blockModels.GetAllBlocks())
                            {
                                if (string.Equals(model.Title, gunType, StringComparison.OrdinalIgnoreCase))
                                {
                                    playerData.BlockType = $"{model.Title}.{weapon.Name}";
                                    Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}{model.Title}.{weapon.Name}");

                                    Menu_BlockSettings(player, parent);
                                    return;
                                }
                            }
                        });
                    }
                }
            }

            Menu.Open(player);
        }

        static void TransparencyMenuOptions(CCSPlayerController player, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Transparency ({playerData.BlockTransparency})", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            foreach (var value in Utils.AlphaMapping.Keys)
            {
                Menu.AddOption(value, (player, option) =>
                {
                    playerData.BlockTransparency = value;

                    Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                    Commands.TransparenyBlock(player, value);

                    Menu_BlockSettings(player, parent);
                });
            }

            Menu.Open(player);
        }

        static void ColorMenuOptions(CCSPlayerController player, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Select Color ({playerData.BlockColor})", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.AddOption(color, (player, option) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player, parent);
                });
            }

            Menu.Open(player);
        }

        static void PropertiesMenuOptions(CCSPlayerController player, CBaseEntity entity, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            if (Blocks.Props.TryGetValue(entity, out var block))
            {
                ScreenMenu Menu = new($"Properties ({block.Type})", Instance)
                {
                    IsSubMenu = true,
                    ParentMenu = parent,
                };

                var properties = block.Properties;

                PropertyMenuOption(Menu, "Reset", 1, player, entity, parent);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity, parent);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity, parent);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity, parent);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity, parent);

                Menu.Open(player);
            }
        }

        static void PropertyMenuOption(ScreenMenu Menu, string property, float value, CCSPlayerController player, CBaseEntity entity, ScreenMenu parent)
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
                        PropertiesMenuOptions(player, entity, parent);
                    }
                    else
                    {
                        Utils.PrintToChat(player, $"Write your desired number in the chat");
                        PropertiesMenuOptions(player, entity, parent);
                    }
                });
            }
        }

        /* Menu_BlockSettings */

        /* Menu_Settings */

        static void Menu_Settings(CCSPlayerController player, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new("Build Settings", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            Menu.AddOption("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.BuildMode(player);

                Menu_Settings(player, parent);
            });

            Menu.AddOption("Godmode: " + (playerData.Godmode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Godmode(player);

                Menu_Settings(player, parent);
            });

            Menu.AddOption("Noclip: " + (playerData.Noclip ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Noclip(player);

                Menu_Settings(player, parent);
            });

            Menu.AddOption($"Grid Settings", (player, option) =>
            {
                GridMenuOptions(player, parent);
            });

            Menu.AddOption($"Snap Settings", (player, option) =>
            {
                SnapMenuOptions(player, parent);
            });

            Menu.AddOption("Save Blocks", (player, option) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player, parent);
            });

            Menu.AddOption("Clear Blocks", (player, option) =>
            {
                ScreenMenu ConfirmMenu = new("Confirm", Instance)
                {
                    IsSubMenu = true,
                    ParentMenu = parent,
                };

                ConfirmMenu.AddOption("NO - keep blocks", (player, option) =>
                {
                    Menu_Settings(player, parent);
                });

                ConfirmMenu.AddOption("YES - remove blocks", (player, option) =>
                {
                    Commands.ClearBlocks(player);

                    Menu_Settings(player, parent);
                });

                ConfirmMenu.Open(player);
            });

            Menu.AddOption("Manage Builders", (player, option) =>
            {
                ScreenMenu BuildersMenu = new("Manage Builders", Instance)
                {
                    IsSubMenu = true,
                    ParentMenu = parent,
                };

                var targets = Utilities.GetPlayers().Where(x => x != player);
                if (targets.Count() <= 0)
                {
                    Utils.PrintToChat(player, $"{ChatColors.Red}No players available");
                    Menu.Open(player);
                    return;
                }

                foreach (var target in targets)
                {
                    BuildersMenu.AddOption(target.PlayerName, (player, option) =>
                    {
                        Commands.ManageBuilder(player, target.SteamID.ToString());
                    });
                }

                BuildersMenu.Open(player);
            });

            Menu.Open(player);
        }

        static void GridMenuOptions(CCSPlayerController player, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Grid Options ({playerData.GridValue} Units)", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            Menu.AddOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = "Grid";

                Utils.PrintToChat(player, $"Write your desired number in the chat");
                GridMenuOptions(player, parent);
            });

            Menu.AddOption($"Grid: " + (playerData.Grid ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Grid(player, "");

                GridMenuOptions(player, parent);
            });

            Menu.Open(player);
        }

        static void SnapMenuOptions(CCSPlayerController player, ScreenMenu parent)
        {
            var playerData = Instance.playerData[player.Slot];

            ScreenMenu Menu = new($"Snap Options ({playerData.SnapValue} Units)", Instance)
            {
                IsSubMenu = true,
                ParentMenu = parent,
            };

            Menu.AddOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = "Snap";

                Utils.PrintToChat(player, $"Write your desired number in the chat");
                SnapMenuOptions(player, parent);
            });

            Menu.AddOption($"Snap: " + (playerData.Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                SnapMenuOptions(player, parent);
            });

            Menu.Open(player);
        }

        /* Menu_Settings */
    }
}