using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json;

public partial class Blocks
{
    public static void Save()
    {
        var path = Files.blocksPath;

        if (!File.Exists(path))
        {
            using (FileStream fs = File.Create(path))
            {
                Utils.Log($"Creating and saving file ({path})");
                fs.Close();
            }
        }

        try
        {
            var blockDataList = new List<SaveBlockData>();

            foreach (var entry in BlocksEntities)
            {
                var block = entry.Key;
                var data = entry.Value;

                if (block != null && block.IsValid)
                {
                    blockDataList.Add(new SaveBlockData
                    {
                        Name = data.Name,
                        Model = data.Model,
                        Size = data.Size,
                        Team = data.Team,
                        Color = data.Color,
                        Transparency = data.Transparency,
                        Position = new VectorUtils.VectorDTO(block.AbsOrigin!),
                        Rotation = new VectorUtils.QAngleDTO(block.AbsRotation!)
                    });
                }
            }

            int blocks = Utils.GetPlacedBlocksCount();

            if (blockDataList.Count() == 0 || blocks == 0)
            {
                Utils.PrintToChatAll($"{ChatColors.Red}No blocks to save");
                return;
            }

            string jsonString = JsonSerializer.Serialize(blockDataList, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, jsonString);

            if (config.Sounds.Building.Enabled)
                Utils.PlaySoundAll(config.Sounds.Building.Save);

            Utils.PrintToChatAll($"Saved {ChatColors.White}{blocks} {ChatColors.Grey}Block{(blocks == 1 ? "" : "s")} on {ChatColors.White}{Utils.GetMapName()}");

            Utils.Log($"Saved {blocks} Block{(blocks == 1 ? "" : "s")} on {Utils.GetMapName()}");
        }
        catch
        {
            Utils.Log("Failed to save blocks :(");
            return;
        }
    }
}
