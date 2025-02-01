using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json;

public partial class Blocks
{
    public static void Save()
    {
        var blocksPath = Path.Combine(Files.mapsFolder, "blocks.json");
        var teleportsPath = Path.Combine(Files.mapsFolder, "teleports.json");

        try
        {
            var blocksList = new List<SaveBlockData>();
            var teleportsList = new List<TeleportPairDTO>();

            // Save blocks
            foreach (var prop in Props)
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
            foreach (var teleport in Teleports)
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
            Utils.PrintToChatAll($"Saved {ChatColors.White}{blocks} {ChatColors.Grey}block{s} on {ChatColors.White}{Utils.GetMapName()}");
        }
        catch (Exception ex)
        {
            Utils.Log($"Failed to save data: {ex.Message}");
        }
    }
}
