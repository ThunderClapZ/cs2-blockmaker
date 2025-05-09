using System.Text.Json.Serialization;
using System.Text.Json;
using CounterStrikeSharp.API.Modules.Extensions;

public static partial class Files
{
    public static class Models
    {
        public static BlockModels Entities { get; set; } = new BlockModels();
        public static void Load()
        {
            string directoryPath = Path.GetDirectoryName(Plugin.Instance.Config.GetConfigPath())!;
            string correctedPath = directoryPath.Replace("/BlockMaker.json", "");

            var modelsPath = Path.Combine(correctedPath, "models.json");

            if (!string.IsNullOrEmpty(modelsPath))
            {
                if (!File.Exists(modelsPath))
                {
                    using (FileStream fs = File.Create(modelsPath))
                        fs.Close();

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    string jsonContent = JsonSerializer.Serialize(Entities, options);

                    File.WriteAllText(modelsPath, jsonContent);
                }
            }

            if (!string.IsNullOrEmpty(modelsPath) && File.Exists(modelsPath))
            {
                string jsonContent = File.ReadAllText(modelsPath);
                Entities = JsonSerializer.Deserialize<BlockModels>(jsonContent) ?? new BlockModels();

                Blocks.LoadTitles();
            }
        }
    }
}