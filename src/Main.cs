using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;

public partial class Plugin : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Block Maker";
    public override string ModuleVersion => "0.1.8";
    public override string ModuleAuthor => "exkludera";

    public static Plugin Instance = new();
    public Dictionary<int, PlayerData> playerData = new();
    public bool buildMode = false;

    public override void Load(bool hotReload)
    {
        Instance = this;

        RegisterEvents();

        Files.Load();

        Commands.Load();

        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                playerData[player.Slot] = new PlayerData();

                if (Utils.HasPermission(player) || Files.Builders.steamids.Contains(player.SteamID.ToString()))
                    playerData[player.Slot].Builder = true;
            }

            Files.mapsFolder = Path.Combine(ModuleDirectory, "maps", Server.MapName);
            Directory.CreateDirectory(Files.mapsFolder);

            Blocks.Clear();

            Files.PropsData.Load();
        }
    }

    public override void Unload(bool hotReload)
    {
        UnregisterEvents();

        Commands.Unload();

        Blocks.Clear();
    }

    public Config Config { get; set; } = new();
    public void OnConfigParsed(Config config)
    {
        Config = config;
        Config.Settings.Prefix = StringExtensions.ReplaceColorTags(config.Settings.Prefix);

        buildMode = config.Settings.Building.BuildMode.Enable;
    }
}