using System.Text.Json.Serialization;
using System.Text.Json;

public class Files
{
    public static BlockModels BlockModels { get; set; } = new BlockModels();

    public static string mapsFolder = "";
    public static string modelsPath = "";
    public static string blocksPath = "";

    public static void Load()
    {
        mapsFolder = Path.Combine(Plugin.Instance.ModuleDirectory, "maps");
        Directory.CreateDirectory(mapsFolder);

        blocksPath = Path.Combine(mapsFolder, "blocks.json");

        modelsPath = Path.Combine(Plugin.Instance.ModuleDirectory, "models.json");

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

                string jsonContent = JsonSerializer.Serialize(BlockModels, options);

                File.WriteAllText(modelsPath, jsonContent);
            }
        }

        if (!string.IsNullOrEmpty(modelsPath) && File.Exists(modelsPath))
        {
            string jsonContent = File.ReadAllText(modelsPath);
            BlockModels = JsonSerializer.Deserialize<BlockModels>(jsonContent) ?? new BlockModels();
        }
    }
}