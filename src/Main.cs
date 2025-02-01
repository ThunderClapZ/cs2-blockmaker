using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;

public partial class Plugin : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Block Maker";
    public override string ModuleVersion => "0.1.2";
    public override string ModuleAuthor => "exkludera";

    public static Plugin Instance { get; set; } = new();
    public Dictionary<int, PlayerData> playerData = new Dictionary<int, PlayerData>();
    public bool buildMode = false;

    public override void Load(bool hotReload)
    {
        Instance = this;

        RegisterEvents();

        Files.Load();

        Commands.Load();

        Menu.Load(hotReload);

        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                playerData[player.Slot] = new();

                if (Utils.HasPermission(player))
                    playerData[player.Slot].Builder = true;
            }

            Files.mapsFolder = Path.Combine(ModuleDirectory, "maps", Utils.GetMapName());
            Directory.CreateDirectory(Files.mapsFolder);

            Blocks.Clear();

            Blocks.Spawn();
        }
    }

    public override void Unload(bool hotReload)
    {
        UnregisterEvents();

        Commands.Unload();

        Menu.Unload();

        Blocks.Clear();
    }

    public Config Config { get; set; } = new Config();
    public void OnConfigParsed(Config config)
    {
        Config = config;
        Config.Settings.Prefix = StringExtensions.ReplaceColorTags(config.Settings.Prefix);

        buildMode = config.Settings.Building.BuildMode;
    }
}