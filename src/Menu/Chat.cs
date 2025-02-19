using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

public static partial class Menu
{
    public static class Chat
    {
        private static Plugin Instance = Plugin.Instance;
        private static Dictionary<int, PlayerData> playerData = Instance.playerData;

        public static void Open(CCSPlayerController player)
        {
            ChatMenu Menu = new("Block Maker");

            Menu.AddMenuOption($"Block Commands", (player, menuOption) =>
            {
                Menu_Commands(player);
            });

            Menu.AddMenuOption($"Block Settings", (player, menuOption) =>
            {
                Menu_BlockSettings(player);
            });

            Menu.AddMenuOption("Build Settings", (player, menuOption) =>
            {
                Menu_Settings(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        /* Menu_Commands */

        private static void Menu_Commands(CCSPlayerController player)
        {
            ChatMenu Menu = new("Block Commands");

            Menu.AddMenuOption("Create", (player, menuOption) =>
            {
                Commands.CreateBlock(player);
            });

            Menu.AddMenuOption("Delete", (player, menuOption) =>
            {
                Commands.DeleteBlock(player);
            });

            Menu.AddMenuOption("Rotate", (player, menuOption) =>
            {
                float[] rotateValues = Instance.Config.Settings.Building.RotationValues;
                string[] rotateOptions = { "Reset", "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                RotateMenuOptions(player, rotateOptions, rotateValues);
            });

            Menu.AddMenuOption("Move", (player, menuOption) =>
            {
                string[] moveOptions = { "X-", "X+", "Y-", "Y+", "Z-", "Z+" };

                MoveMenuOptions(player, moveOptions);
            });

            Menu.AddMenuOption("Convert", (player, menuOption) =>
            {
                Commands.ConvertBlock(player);
            });

            Menu.AddMenuOption("Copy", (player, menuOption) =>
            {
                Commands.CopyBlock(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void RotateMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
        {
            ChatMenu Menu = new($"Rotate Block ({playerData[player.Slot].RotationValue} Units)");

            Menu.AddMenuOption($"Select Units", (player, option) =>
            {
                RotateValuesMenuOptions(player, rotateOptions, rotateValues);
            });

            foreach (string rotateOption in rotateOptions)
            {
                Menu.AddMenuOption(rotateOption, (player, option) =>
                {
                    Commands.RotateBlock(player, rotateOption);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void RotateValuesMenuOptions(CCSPlayerController player, string[] rotateOptions, float[] rotateValues)
        {
            ChatMenu Menu = new($"Rotate Values");

            foreach (float rotateValueOption in rotateValues)
            {
                Menu.AddMenuOption(rotateValueOption.ToString() + " Units", (player, option) =>
                {
                    playerData[player.Slot].RotationValue = rotateValueOption;

                    Utils.PrintToChat(player, $"Selected Rotation Value: {ChatColors.White}{rotateValueOption} Units");

                    RotateMenuOptions(player, rotateOptions, rotateValues);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void MoveMenuOptions(CCSPlayerController player, string[] moveOptions)
        {
            CenterHtmlMenu Menu = new($"Move Block ({playerData[player.Slot].RotationValue} Units)", Instance);

            Menu.AddMenuOption($"Select Units", (player, option) =>
            {
                float[] gridValues = Instance.Config.Settings.Building.GridValues;

                GridMenuOptions(player, gridValues);
            });

            foreach (string moveOption in moveOptions)
            {
                Menu.AddMenuOption(moveOption, (player, option) =>
                {
                    Commands.MoveBlock(player, moveOption);
                });
            }

            MenuManager.OpenCenterHtmlMenu(Instance, player, Menu);
        }

        /* Menu_Commands */

        /* Menu_BlockSettings */

        private static void Menu_BlockSettings(CCSPlayerController player)
        {
            ChatMenu Menu = new("Block Settings");

            Menu.AddMenuOption($"Type: {playerData[player.Slot].BlockType}", (player, menuOption) =>
            {
                TypeMenuOptions(player);
            });

            Menu.AddMenuOption($"Size: {playerData[player.Slot].BlockSize}", (player, menuOption) =>
            {
                string[] sizeValues = Instance.Config.Settings.Building.BlockSizes.Select(b => b.Title).ToArray();

                SizeMenuOptions(player, sizeValues);
            });

            Menu.AddMenuOption($"Team: {playerData[player.Slot].BlockTeam}", (player, menuOption) =>
            {
                string[] teamValues = { "Both", "T", "CT" };

                TeamMenuOptions(player, teamValues);
            });

            Menu.AddMenuOption($"Transparency: {playerData[player.Slot].BlockTransparency}", (player, menuOption) =>
            {
                TransparencyMenuOptions(player);
            });

            Menu.AddMenuOption($"Color: {playerData[player.Slot].BlockColor}", (player, menuOption) =>
            {
                ColorMenuOptions(player);
            });

            Menu.AddMenuOption($"Properties", (player, menuOption) =>
            {
                PropertiesMenuOptions(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void TypeMenuOptions(CCSPlayerController player)
        {
            ChatMenu Menu = new($"Select Type ({playerData[player.Slot].BlockType})");

            var blockmodels = Files.Models.Props;

            foreach (var property in typeof(BlockModels).GetProperties())
            {
                var block = (BlockModel)property.GetValue(blockmodels)!;

                string blockName = block.Title;

                Menu.AddMenuOption(blockName, (player, menuOption) =>
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

            Menu.AddMenuOption("Teleport", (player, menuOption) =>
            {
                playerData[player.Slot].BlockType = "Teleport";
                Utils.PrintToChat(player, $"Selected Type: {ChatColors.White}Teleport");

                Menu_BlockSettings(player);
            });

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void GunTypeMenu(CCSPlayerController player, string gunType)
        {
            ChatMenu Menu = new($"Select {gunType}");

            if (WeaponList.Categories.ContainsKey(gunType))
            {
                var weaponsInCategory = WeaponList.Categories[gunType];

                foreach (var weaponID in weaponsInCategory)
                {
                    var weapon = WeaponList.Weapons.FirstOrDefault(w => w.Designer == weaponID);

                    if (weapon != null)
                    {
                        Menu.AddMenuOption(weapon.Name, (player, menuOption) =>
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

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void SizeMenuOptions(CCSPlayerController player, string[] sizeValues)
        {
            ChatMenu Menu = new($"Select Size ({playerData[player.Slot].BlockSize})");

            Menu.AddMenuOption($"Pole: {(playerData[player.Slot].BlockPole ? "ON" : "OFF")}", (player, option) =>
            {
                Commands.Pole(player);

                SizeMenuOptions(player, sizeValues);
            });

            foreach (string sizeValue in sizeValues)
            {
                Menu.AddMenuOption(sizeValue, (player, option) =>
                {
                    playerData[player.Slot].BlockSize = sizeValue;

                    Utils.PrintToChat(player, $"Selected Size: {ChatColors.White}{sizeValue}");

                    Menu_BlockSettings(player);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void TeamMenuOptions(CCSPlayerController player, string[] teamValues)
        {
            ChatMenu Menu = new($"Select Team ({playerData[player.Slot].BlockTeam})");

            foreach (string teamValue in teamValues)
            {
                Menu.AddMenuOption(teamValue, (player, option) =>
                {
                    playerData[player.Slot].BlockTeam = teamValue;

                    Utils.PrintToChat(player, $"Selected Team: {ChatColors.White}{teamValue}");

                    Menu_BlockSettings(player);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void TransparencyMenuOptions(CCSPlayerController player)
        {
            ChatMenu Menu = new($"Select Transparency ({playerData[player.Slot].BlockTransparency})");

            foreach (var value in Utils.AlphaMapping.Keys)
            {
                Menu.AddMenuOption(value, (player, menuOption) =>
                {
                    playerData[player.Slot].BlockTransparency = value;

                    Utils.PrintToChat(player, $"Selected Transparency: {ChatColors.White}{value}");

                    Commands.TransparenyBlock(player, value);

                    Menu_BlockSettings(player);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void ColorMenuOptions(CCSPlayerController player)
        {
            ChatMenu Menu = new($"Select Color ({playerData[player.Slot].BlockColor})");

            foreach (var color in Utils.ColorMapping.Keys)
            {
                Menu.AddMenuOption(color, (player, menuOption) =>
                {
                    Commands.BlockColor(player, color);

                    Menu_BlockSettings(player);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
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
                ChatMenu Menu = new($"Properties ({block.Type})");

                var properties = block.Properties;

                playerData[player.Slot].PropertyType = "";
                playerData[player.Slot].PropertyEntity.Clear();

                PropertyMenuOption(Menu, "Reset", 1, player, entity);
                PropertyMenuOption(Menu, "OnTop", 1, player, entity);
                PropertyMenuOption(Menu, "Duration", properties.Duration, player, entity);
                PropertyMenuOption(Menu, "Value", properties.Value, player, entity);
                PropertyMenuOption(Menu, "Cooldown", properties.Cooldown, player, entity);

                MenuManager.OpenChatMenu(player, Menu);
            }
        }

        private static void PropertyMenuOption(ChatMenu Menu, string property, float value, CCSPlayerController player, CBaseProp entity)
        {
            if (value != 0)
            {
                string title = $": {value}";

                if (property == "Reset")
                    title = "";

                if (property == "OnTop")
                    title = Blocks.Props[entity].Properties.OnTop ? ": Enabled" : ": Disabled";

                Menu.AddMenuOption($"{property}{title}", (player, menuOption) =>
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
            ChatMenu Menu = new("Build Settings");

            Menu.AddMenuOption("Build Mode: " + (Instance.buildMode ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.BuildMode(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption("Godmode: " + (playerData[player.Slot].Godmode ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.Godmode(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption("Noclip: " + (playerData[player.Slot].Noclip ? "ON" : "OFF"), (player, menuOption) =>
            {
                Commands.Noclip(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption($"Grid: {playerData[player.Slot].GridValue} Units", (player, menuOption) =>
            {
                float[] gridValues = Instance.Config.Settings.Building.GridValues;

                GridMenuOptions(player, gridValues);
            });

            Menu.AddMenuOption("Save Blocks", (player, menuOption) =>
            {
                Commands.SaveBlocks(player);

                Menu_Settings(player);
            });

            Menu.AddMenuOption("Clear Blocks", (player, menuOption) =>
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

            Menu.AddMenuOption("Manage Builders", (player, menuOption) =>
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

            MenuManager.OpenChatMenu(player, Menu);
        }

        private static void GridMenuOptions(CCSPlayerController player, float[] gridValues)
        {
            ChatMenu Menu = new($"Grid Options ({playerData[player.Slot].GridValue} Units)");

            Menu.AddMenuOption($"Grid: " + (playerData[player.Slot].Grid ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Grid(player, "");

                GridMenuOptions(player, gridValues);
            });

            Menu.AddMenuOption($"Snap: " + (playerData[player.Slot].Snapping ? "ON" : "OFF"), (player, option) =>
            {
                Commands.Snapping(player);

                GridMenuOptions(player, gridValues);
            });

            foreach (float gridValue in gridValues)
            {
                Menu.AddMenuOption(gridValue.ToString() + " Units", (player, option) =>
                {
                    Commands.Grid(player, gridValue.ToString());

                    GridMenuOptions(player, gridValues);
                });
            }

            MenuManager.OpenChatMenu(player, Menu);
        }

        /* Menu_Settings */
    }
}