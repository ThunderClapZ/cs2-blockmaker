public static partial class Files
{
    public static string mapsFolder = "";

    public static void Load()
    {
        mapsFolder = Path.Combine(Plugin.Instance.ModuleDirectory, "maps");
        Directory.CreateDirectory(mapsFolder);

        Models.Load();

        Builders.Load();

        Blocks.Properties.Load();
    }
}