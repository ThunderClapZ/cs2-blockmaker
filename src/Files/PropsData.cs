using System.Text.Json;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Modules.Extensions;

public static partial class Files
{
    public static class PropsData
    {
        public static void Save()
        {
            var blocksPath = Path.Combine(mapsFolder, "blocks.json");
            var teleportsPath = Path.Combine(mapsFolder, "teleports.json");

            try
            {
                var blocksList = new List<SaveBlockData>();
                var teleportsList = new List<TeleportPairDTO>();

                // Save blocks
                foreach (var prop in Blocks.Props)
                {
                    var block = prop.Key;
                    var data = prop.Value;

                    if (block != null && block.IsValid)
                    {
                        blocksList.Add(new SaveBlockData
                        {
                            Name = data.Name,
                            Model = data.Model,
                            Size = data.Size,
                            Team = data.Team,
                            Color = data.Color,
                            Transparency = data.Transparency,
                            Properties = data.Properties,
                            Position = new VectorUtils.VectorDTO(block.AbsOrigin!),
                            Rotation = new VectorUtils.QAngleDTO(block.AbsRotation!)
                        });
                    }
                }

                // Save teleports
                foreach (var teleport in Blocks.Teleports)
                {
                    if (teleport.Entry != null && teleport.Exit != null)
                    {
                        teleportsList.Add(new TeleportPairDTO
                        {
                            Entry = new SaveTeleportData
                            {
                                Name = teleport.Entry.Name,
                                Model = teleport.Entry.Model,
                                Position = new VectorUtils.VectorDTO(teleport.Entry.Entity.AbsOrigin!),
                                Rotation = new VectorUtils.QAngleDTO(teleport.Entry.Entity.AbsRotation!)
                            },
                            Exit = new SaveTeleportData
                            {
                                Name = teleport.Exit.Name,
                                Model = teleport.Exit.Model,
                                Position = new VectorUtils.VectorDTO(teleport.Exit.Entity.AbsOrigin!),
                                Rotation = new VectorUtils.QAngleDTO(teleport.Exit.Entity.AbsRotation!)
                            }
                        });
                    }
                }

                int blocksCount = blocksList.Count;
                int teleportsCount = teleportsList.Count;

                // Save blocks to blocks.json
                if (blocksCount > 0)
                {
                    string blocksJson = JsonSerializer.Serialize(blocksList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(blocksPath, blocksJson);
                }

                // Save teleports to teleports.json
                if (teleportsCount > 0)
                {
                    string teleportsJson = JsonSerializer.Serialize(teleportsList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(teleportsPath, teleportsJson);
                }

                if (Plugin.Instance.Config.Sounds.Building.Enabled)
                    Utils.PlaySoundAll(Plugin.Instance.Config.Sounds.Building.Save);

                int blocks = Utils.GetPlacedBlocksCount();
                var s = blocks == 1 ? "" : "s";

                Utils.PrintToChatAll($"Saved {ChatColors.White}{blocks} {ChatColors.Grey}block{s} on {ChatColors.White}{Server.MapName}");
            }
            catch (Exception ex)
            {
                Utils.Log($"Failed to save data: {ex.Message}");
            }
        }

        public static void Load()
        {
            var blocksPath = Path.Combine(mapsFolder, "blocks.json");
            var teleportsPath = Path.Combine(mapsFolder, "teleports.json");

            // spawn blocks
            if (File.Exists(blocksPath) && Utils.IsValidJson(blocksPath))
            {
                var blocksJson = File.ReadAllText(blocksPath);
                var blocksList = JsonSerializer.Deserialize<List<SaveBlockData>>(blocksJson);

                if (blocksList != null && blocksList.Count > 0)
                {
                    foreach (var blockData in blocksList)
                    {
                        var position = new Vector(blockData.Position.X, blockData.Position.Y, blockData.Position.Z);
                        var rotation = new QAngle(blockData.Rotation.Pitch, blockData.Rotation.Yaw, blockData.Rotation.Roll);

                        Blocks.CreateBlock(
                            blockData.Name,
                            blockData.Model,
                            blockData.Size,
                            position,
                            rotation,
                            blockData.Color,
                            blockData.Transparency,
                            blockData.Team,
                            blockData.Properties
                        );
                    }
                }
            }
            else Utils.Log($"Failed to spawn Blocks. File for {Server.MapName} is empty or invalid");

            // spawn teleports
            if (File.Exists(teleportsPath) && Utils.IsValidJson(teleportsPath))
            {
                var teleportsJson = File.ReadAllText(teleportsPath);
                var teleportsList = JsonSerializer.Deserialize<List<TeleportPairDTO>>(teleportsJson);

                if (teleportsList != null && teleportsList.Count > 0)
                {
                    foreach (var teleportPairData in teleportsList)
                    {
                        var entryPosition = new Vector(teleportPairData.Entry.Position.X, teleportPairData.Entry.Position.Y, teleportPairData.Entry.Position.Z);
                        var entryRotation = new QAngle(teleportPairData.Entry.Rotation.Pitch, teleportPairData.Entry.Rotation.Yaw, teleportPairData.Entry.Rotation.Roll);

                        var entryEntity = Blocks.CreateTeleportEntity(entryPosition, entryRotation, teleportPairData.Entry.Name);

                        var exitPosition = new Vector(teleportPairData.Exit.Position.X, teleportPairData.Exit.Position.Y, teleportPairData.Exit.Position.Z);
                        var exitRotation = new QAngle(teleportPairData.Exit.Rotation.Pitch, teleportPairData.Exit.Rotation.Yaw, teleportPairData.Exit.Rotation.Roll);

                        var exitEntity = Blocks.CreateTeleportEntity(exitPosition, exitRotation, teleportPairData.Exit.Name);

                        if (entryEntity != null && exitEntity != null)
                            Blocks.Teleports.Add(new TeleportPair(entryEntity, exitEntity));
                    }
                }
            }
        }

        public static class Properties
        {
            public static readonly Dictionary<string, BlockData_Properties> BlockDefaultProperties = new Dictionary<string, BlockData_Properties>
            {
                { Models.Props.Bhop.Title, new BlockData_Properties { Duration = 0.25f, Cooldown = 1.5f } },
                { Models.Props.Health.Title, new BlockData_Properties { Value = 2.0f, Cooldown = 0.5f } },
                { Models.Props.Grenade.Title, new BlockData_Properties { Cooldown = 60.0f } },
                { Models.Props.Gravity.Title, new BlockData_Properties { Duration = 4.0f, Value = 0.4f, Cooldown = 5.0f } },
                { Models.Props.Frost.Title, new BlockData_Properties { Cooldown = 60.0f } },
                { Models.Props.Flash.Title, new BlockData_Properties { Cooldown = 60.0f } },
                { Models.Props.Fire.Title, new BlockData_Properties { Duration = 5.0f, Value = 8.0f, Cooldown = 5.0f } },
                { Models.Props.Delay.Title, new BlockData_Properties { Duration = 1.0f, Cooldown = 1.5f } },
                { Models.Props.Damage.Title, new BlockData_Properties { Value = 5.0f, Cooldown = 0.5f } },
                { Models.Props.Stealth.Title, new BlockData_Properties { Duration = 10.0f, Cooldown = 60.0f } },
                { Models.Props.Speed.Title, new BlockData_Properties { Duration = 3.0f, Value = 2.0f, Cooldown = 60.0f } },
                { Models.Props.SpeedBoost.Title, new BlockData_Properties { Value = 650.0f } },
                { Models.Props.Camouflage.Title, new BlockData_Properties { Duration = 1.0f, Cooldown = 60.0f } },
                { Models.Props.Slap.Title, new BlockData_Properties { Value = 2.0f } },
                { Models.Props.Random.Title, new BlockData_Properties { Cooldown = 60f } },
                { Models.Props.Invincibility.Title, new BlockData_Properties { Duration = 5.0f, Cooldown = 60.0f } },
                { Models.Props.Trampoline.Title, new BlockData_Properties { Value = 500.0f} },
                { Models.Props.Death.Title, new BlockData_Properties { OnTop = false } },
                { Models.Props.Platform.Title, new BlockData_Properties() },
                { Models.Props.NoFallDmg.Title, new BlockData_Properties() },
                { Models.Props.Ice.Title, new BlockData_Properties() },
                { Models.Props.Nuke.Title, new BlockData_Properties() },
                { Models.Props.Glass.Title, new BlockData_Properties() },
                { Models.Props.Pistol.Title, new BlockData_Properties() },
                { Models.Props.Rifle.Title, new BlockData_Properties() },
                { Models.Props.Sniper.Title, new BlockData_Properties() },
                { Models.Props.ShotgunHeavy.Title, new BlockData_Properties() },
                { Models.Props.SMG.Title, new BlockData_Properties() },
            };

            public static Dictionary<string, BlockData_Properties> BlockProperties { get; set; } = new Dictionary<string, BlockData_Properties>();

            public static void Load()
            {
                string directoryPath = Path.GetDirectoryName(Plugin.Instance.Config.GetConfigPath())!;
                string correctedPath = directoryPath.Replace("/BlockMaker.json", "");

                var propertiesPath = Path.Combine(correctedPath, "default_properties.json");

                if (!string.IsNullOrEmpty(propertiesPath))
                {
                    if (!File.Exists(propertiesPath))
                    {
                        using (FileStream fs = File.Create(propertiesPath))
                            fs.Close();

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };

                        string jsonContent = JsonSerializer.Serialize(BlockDefaultProperties, options);

                        File.WriteAllText(propertiesPath, jsonContent);
                    }

                    if (File.Exists(propertiesPath))
                    {
                        string jsonContent = File.ReadAllText(propertiesPath);
                        BlockProperties = JsonSerializer.Deserialize<Dictionary<string, BlockData_Properties>>(jsonContent) ?? new Dictionary<string, BlockData_Properties>();
                    }
                }
            }
        }
    }
}