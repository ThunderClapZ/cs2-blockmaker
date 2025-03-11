using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManager;

public static partial class Menu
{
    public static class MenuManager
    {
        public static IMenuApi API = new PluginCapability<IMenuApi>("menu:nfcore").Get()!;

        public static void Open(CCSPlayerController player)
        {
            var Menu = API.GetMenu("Block Maker");

            Menu.AddMenuOption($"Block Commands", (player, option) =>
            {
                Menu_Commands(player);
            });

            Menu.AddMenuOption($"Block Settings", (player, option) =>
            {
                Menu_BlockSettings(player);
            });

            Menu.AddMenuOption("Build Settings", (player, option) =>
            {
                Menu_Settings(player);
            });

            Menu.Open(player);
        }

        /* Menu_Commands */

        static void Menu_Commands(CCSPlayerController player)
        {
            var Menu = API.GetMenu("Block Commands");

            Menu.AddMenuOption("Create", (player, option) =>
            {
                Commands.CreateBlock(player);
            });

            Menu.AddMenuOption("Delete", (player, option) =>
            {
                Commands.DeleteBlock(player);
            });

            Menu.AddMenuOption("Rotate", (player, option) =>
            {
                string[] options = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                PositionMenuOptions(player, options, true);
            });

            Menu.AddMenuOption("Move", (player, option) =>
            {
                string[] options = { "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                PositionMenuOptions(player, options, false);
            });

            Menu.AddMenuOption("Copy", (player, option) =>
            {
                Commands.CopyBlock(player);
            });

            Menu.AddMenuOption("Lock", (player, option) =>
            {
                Commands.LockBlock(player);
            });

            Menu.AddMenuOption("Convert", (player, option) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.Open(player);
        }

        static void PositionMenuOptions(CCSPlayerController player, string[] options, bool rotate)
        {
            var playerData = Instance.playerData[player.Slot];

            float value = rotate ? playerData.RotationValue : playerData.PositionValue;
            string title = $"{(rotate ? "Rotate" : "Move")} Block ({value} Units)";

            var Menu = API.GetMenu(title);

            Menu.AddMenuOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = rotate ? "Rotation" : "Position";
                Utils.PrintToChat(player, $"Write your desired number in the chat");
                PositionMenuOptions(player, options, rotate);
            });

            foreach (string input in options)
            {
                Menu.AddMenuOption(input, (player, option) =>
                {
                    if (rotate) Commands.RotateBlock(player, input);
                    else Commands.PositionBlock(player, input);
                });
            }

            Menu.Open(player);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        static void Menu_BlockSettings(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu("Block Settings");

            Menu.AddMenuOption($"Type: {playerData.BlockType}", (player, option) =>
            {
                TypeMenuOptions(player);
            });

            Menu.AddMenuOption($"Size: {playerData.BlockSize}", (player, option) =>
            {
                var sizeValues = Instance.Config.Settings.Building.BlockSizes.ToArray();
                int currentIndex = Array.FindIndex(sizeValues, s => s.Title == playerData.BlockSize);
                int nextIndex = (currentIndex + 1) % sizeValues.Length;
                playerData.BlockSize = sizeValues[nextIndex].Title;

                Menu_BlockSettings(player);
            });

            Menu.AddMenuOption($"Pole: {(playerData.BlockPole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                Menu_BlockSettings(player);
            });

            Menu.AddMenuOption($"Team: {playerData.BlockTeam}", (player, option) =>
            {
                string[] teamValues = { "Both", "T", "CT" };
                int currentIndex = Array.IndexOf(teamValues, playerData.BlockTeam);
                int nextIndex = (currentIndex + 1) % teamValues.Length;
                playerData.BlockTeam = teamValues[nextIndex];
                Commands.TeamBlock(player, teamValues[nextIndex]);

                Menu_BlockSettings(player);
            });

            Menu.AddMenuOption($"Properties", (player, option) =>
            {
                var entity = player.GetBlockAimTarget();

                if (entity?.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
                {
                    Utils.PrintToChat(player, "Could not find a block to edit properties");
                    Menu_BlockSettings(player);
                    return;
                }

                playerData.ChatInput = "";
                playerData.PropertyEntity.Clear();

                PropertiesMenuOptions(player, entity);
            });

            Menu.AddMenuOption($"Transparency: {playerData.BlockTransparency}", (player, option) =>
            {
                TransparencyMenuOptions(player);
            });

            Menu.AddMenuOption($"Color: {playerData.BlockColor}", (player, option) =>
            {
                ColorMenuOptions(player);
            });

            Menu.Open(player);
        }

        static void TypeMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu($"Select Type ({playerData.BlockType})");

            var blockmodels = Files.Models.Props;

            foreach (var block in blockmodels.GetAllBlocks())
            {
                string blockName = block.Title;

                Menu.AddMenuOption(blockName, (player, option) =>
                {
                    if (blockName == blockmodels.Pistol.Title ||
                        blockName == blockmodels.Sniper.Title ||
                        blockName == blockmodels.Rifle.Title ||
                        blockName == blockmodels.SMG.Title ||
                        blockName == blockmodels.ShotgunHeavy.Title
                    )
                    {
                        GunTypeMenu(player, block.Title);
                        return;
                    }

                    Commands.BlockType(player, blockName);

                    Menu_BlockSettings(player);
                });
            }

            Menu.AddMenuOption("Teleport", (player, option) =>
            {
                playerData.BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player);
            });

            Menu.Open(player);
        }

        static void GunTypeMenu(CCSPlayerController player, string gunType)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu($"Select {gunType}");

            if (WeaponList.Categories.ContainsKey(gunType))
            {
                var weaponsInCategory = WeaponList.Categories[gunType];

                foreach (var weaponID in weaponsInCategory)
                {
                    var weapon = WeaponList.Weapons.FirstOrDefault(w => w.Designer == weaponID);

                    if (weapon != null)
                    {
                        Menu.AddMenuOption(weapon.Name, (player, option) =>
                        {
                            var blockModels = Files.Models.Props;
                            foreach (var model in blockModels.GetAllBlocks())
                            {
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

            Menu.Open(player);
        }

        static void TransparencyMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu($"Select Transparency ({playerData.BlockTransparency})");

            foreach (var value in Utils.AlphaMapping.Keys)
            {
                Menu.AddMenuOption(value, (player, option) =>
                {
                    playerData.BlockTransparency = value;

                    Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                    Commands.TransparenyBlock(player, value);

                    Menu_BlockSettings(player);
                });
            }

            Menu.Open(player);
        }

        static void ColorMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu($"Select Color ({playerData.BlockColor})");

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.AddMenuOption(color, (player, option) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player);
                });
            }

            Menu.Open(player);
        }

        static void PropertiesMenuOptions(CCSPlayerController player, CBaseEntity entity)
        {
            var playerData = Instance.playerData[player.Slot];

            if (Blocks.Props.TryGetValue(entity, out var block))
            {
                var Menu = API.GetMenu($"Properties ({block.Type})");

                var properties = block.Properties;

                PropertyMenuOption(Menu, "Reset", 1, player, entity);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity);

                Menu.Open(player);
            }
        }

        static void PropertyMenuOption(IMenu Menu, string property, float value, CCSPlayerController player, CBaseEntity entity)
        {
            var playerData = Instance.playerData[player.Slot];

            if (value != 0)
            {
                string title = $": {value}";

                if (property == "Reset")
                    title = "";

                if (property == "OnTop")
                    title = Blocks.Props[entity].Properties.OnTop ? ": Enabled" : ": Disabled";

                Menu.AddMenuOption($"{property}{title}", (player, option) =>
                {
                    playerData.ChatInput = property;
                    playerData.PropertyEntity[property] = entity;

                    if (property == "Reset" || property == "OnTop")
                    {
                        Commands.Properties(player, property, property);
                        PropertiesMenuOptions(player, entity);
                    }
                    else
                    {
                        Utils.PrintToChat(player, $"Write your desired number in the chat");
                        PropertiesMenuOptions(player, entity);
                    }
                });
            }
        }

        /* Menu_BlockSettings */

        /* Menu_Settings */

        static void Menu_Settings(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu("Build Settings");

            Menu.AddMenuOption("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.BuildMode(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption("Godmode: " + (playerData.Godmode ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Godmode(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption("Noclip: " + (playerData.Noclip ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Noclip(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption($"Grid Settings", (player, option) =>
            {
                GridMenuOptions(player);
            });

            Menu.AddMenuOption($"Snap Settings", (player, option) =>
            {
                SnapMenuOptions(player);
            });

            Menu.AddMenuOption("Save Blocks", (player, option) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption("Clear Blocks", (player, option) =>
            {
                var ConfirmMenu = API.GetMenu("Confirm");

                ConfirmMenu.AddMenuOption("NO - keep blocks", (player, option) =>
                {
                    Menu_Settings(player);
                });

                ConfirmMenu.AddMenuOption("YES - remove blocks", (player, option) =>
                {
                    Commands.ClearBlocks(player);

                    Menu_Settings(player);
                });

                ConfirmMenu.Open(player);
            });

            Menu.AddMenuOption("Manage Builders", (player, option) =>
            {
                var BuildersMenu = API.GetMenu("Manage Builders");

                var targets = Utilities.GetPlayers().Where(x => x != player);
                if (targets.Count() <= 0)
                {
                    Utils.PrintToChat(player, $"{ChatColors.Red}No players available");
                    Menu.Open(player);
                    return;
                }

                foreach (var target in targets)
                {
                    BuildersMenu.AddMenuOption(target.PlayerName, (player, option) =>
                    {
                        Commands.ManageBuilder(player, target.SteamID.ToString());
                    });
                }

                BuildersMenu.Open(player);
            });

            Menu.Open(player);
        }

        static void GridMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu($"Grid Options ({playerData.GridValue} Units)");

            Menu.AddMenuOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = "Grid";

                Utils.PrintToChat(player, $"Write your desired number in the chat");
                GridMenuOptions(player);
            });

            Menu.AddMenuOption($"Grid: " + (playerData.Grid ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Grid(player, "");

                GridMenuOptions(player);
            });

            Menu.Open(player);
        }

        static void SnapMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            var Menu = API.GetMenu($"Snap Options ({playerData.SnapValue} Units)");

            Menu.AddMenuOption($"Select Units", (player, option) =>
            {
                playerData.ChatInput = "Snap";

                Utils.PrintToChat(player, $"Write your desired number in the chat");
                SnapMenuOptions(player);
            });

            Menu.AddMenuOption($"Snap: " + (playerData.Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                SnapMenuOptions(player);
            });

            Menu.Open(player);
        }

        /* Menu_Settings */
    }
}