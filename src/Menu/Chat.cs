using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

public static partial class Menu
{
    public static class Chat
    {
        public static void Open(CCSPlayerController player)
        {
            ChatMenu Menu = new("Block Maker");

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

            MenuManager.OpenChatMenu(player, Menu);
        }

        /* Menu_Commands */

        static void Menu_Commands(CCSPlayerController player)
        {
            ChatMenu Menu = new("Block Commands");

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

            Menu.AddMenuOption("Convert", (player, option) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.AddMenuOption("Copy", (player, option) =>
            {
                Commands.CopyBlock(player);
            });

            Menu.AddMenuOption("Lock", (player, option) =>
            {
                Commands.LockBlock(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void PositionMenuOptions(CCSPlayerController player, string[] options, bool rotate)
        {
            var playerData = Instance.playerData[player.Slot];

            float value = rotate ? playerData.RotationValue : playerData.PositionValue;
            string title = $"{(rotate ? "Rotate" : "Move")} Block ({value} Units)";

            ChatMenu Menu = new(title);

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

            MenuManager.OpenChatMenu(player, Menu);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        static void Menu_BlockSettings(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new("Block Settings");

            Menu.AddMenuOption($"Type: {playerData.BlockType}", (player, option) =>
            {
                TypeMenuOptions(player);
            });

            Menu.AddMenuOption($"Size: {playerData.BlockSize}", (player, option) =>
            {
                string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

                SizeMenuOptions(player, sizeValues);
            });

            Menu.AddMenuOption($"Team: {playerData.BlockTeam}", (player, option) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, teamValues);
            });

            Menu.AddMenuOption($"Transparency: {playerData.BlockTransparency}", (player, option) =>
            {
                TransparencyMenuOptions(player);
            });

            Menu.AddMenuOption($"Color: {playerData.BlockColor}", (player, option) =>
            {
                ColorMenuOptions(player);
            });

            Menu.AddMenuOption($"Properties", (player, option) =>
            {
                PropertiesMenuOptions(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void TypeMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new($"Select Type ({playerData.BlockType})");

            var blockmodels = Files.Models.Props;

            foreach (var property in typeof(BlockModels).GetProperties())
            {
                var block = (BlockModel)property.GetValue(blockmodels)!;

                string blockName = block.Title;

                Menu.AddMenuOption(blockName, (player, option) =>
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

            Menu.AddMenuOption("Teleport", (player, option) =>
            {
                playerData.BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void GunTypeMenu(CCSPlayerController player, string gunType)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new($"Select {gunType}");

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

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new($"Select Size ({playerData.BlockSize})");

            Menu.AddMenuOption($"Pole: {(playerData.BlockPole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                SizeMenuOptions(player, sizeValues);
            });

            foreach (string sizeValue in sizeValues)
            {
                Menu.AddMenuOption(sizeValue, (player, option) =>
                {
                    playerData.BlockSize = sizeValue;

                    Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                    Menu_BlockSettings(player);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new($"Select Team ({playerData.BlockTeam})");

            foreach (string teamValue in teamValues)
            {
                Menu.AddMenuOption(teamValue, (player, option) =>
                {
                    playerData.BlockTeam = teamValue;

                    Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                    Commands.TeamBlock(player, teamValue);

                    Menu_BlockSettings(player);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void TransparencyMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new($"Select Transparency ({playerData.BlockTransparency})");

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

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void ColorMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new($"Select Color ({playerData.BlockColor})");

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.AddMenuOption(color, (player, option) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
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
                ChatMenu Menu = new($"Properties ({block.Type})");

                playerData.ChatInput = "";
                playerData.PropertyEntity.Clear();

                var properties = block.Properties;

                PropertyMenuOption(Menu, "Reset", 1, player, entity);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity);

                MenuManager.OpenChatMenu(player, Menu);
            }
        }

        static void PropertyMenuOption(ChatMenu Menu, string property, float value, CCSPlayerController player, CBaseProp entity)
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

            ChatMenu Menu = new("Build Settings");

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

            Menu.AddMenuOption("Save Blocks", (player, option) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption("Clear Blocks", (player, option) =>
            {
                ChatMenu ConfirmMenu = new("Confirm");

                ConfirmMenu.AddMenuOption("NO - keep blocks", (player, option) =>
                {
                    Menu_Settings(player);
                });

                ConfirmMenu.AddMenuOption("YES - remove blocks", (player, option) =>
                {
                    Commands.ClearBlocks(player);

                    Menu_Settings(player);
                });

                MenuManager.OpenChatMenu(player, ConfirmMenu);
            });

            Menu.AddMenuOption("Manage Builders", (player, option) =>
            {
                ChatMenu BuildersMenu = new("Manage Builders");

                foreach (var target in Utilities.GetPlayers().Where(x => x != player))
                {
                    BuildersMenu.AddMenuOption(target.PlayerName, (player, option) =>
                    {
                        Commands.ManageBuilder(player, target.SteamID.ToString());
                    });
                }

                MenuManager.OpenChatMenu(player, BuildersMenu);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        static void GridMenuOptions(CCSPlayerController player)
        {
            var playerData = Instance.playerData[player.Slot];

            ChatMenu Menu = new($"Grid Options ({playerData.GridValue} Units)");

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

            Menu.AddMenuOption($"Snap: " + (playerData.Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                GridMenuOptions(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        /* Menu_Settings */
    }
}