using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static VectorUtils;

public partial class Blocks
{
    public static string savedPath = "";

    public static void Save()
    {
        if (!File.Exists(savedPath))
        {
            using (FileStream fs = File.Create(savedPath))
            {
                instance.Logger.LogInformation($"File does not exist, creating one ({savedPath})");
                fs.Close();
            }
        }

        try
        {
            var blockDataList = new List<SaveBlockData>();

            foreach (var entry in UsedBlocks)
            {
                var prop = entry.Key;
                var data = entry.Value;

                if (prop != null && prop.IsValid)
                {
                    blockDataList.Add(new SaveBlockData
                    {
                        Name = data.Name,
                        Model = data.Model,
                        Size = data.Size,
                        Color = data.Color,
                        Position = new VectorDTO(prop.AbsOrigin!),
                        Rotation = new QAngleDTO(prop.AbsRotation!)
                    });
                }
            }

            if (blockDataList.Count() == 0 || instance.GetPlacedBlocksCount() == 0)
            {
                instance.PrintToChatAll($"{ChatColors.Red}No blocks to save");
                return;
            }

            var jsonString = JsonSerializer.Serialize(blockDataList, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(savedPath, jsonString);

            if (instance.Config.Sounds.Building.Enabled)
                instance.PlaySoundAll(instance.Config.Sounds.Building.Save);

            instance.PrintToChatAll($"Saved {ChatColors.White}{instance.GetPlacedBlocksCount()} {ChatColors.Grey}Block{(instance.GetPlacedBlocksCount() == 1 ? "" : "s")} on {ChatColors.White}{instance.GetMapName()}");

            instance.Logger.LogInformation($"Saved {instance.GetPlacedBlocksCount()} Block{(instance.GetPlacedBlocksCount() == 1 ? "" : "s")} on {instance.GetMapName()}");
        }
        catch
        {
            instance.Logger.LogError("Failed to save blocks :(");
            return;
        }
    }
}
