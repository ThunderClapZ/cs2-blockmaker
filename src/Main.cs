using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using StarCore.Utils;

public partial class Plugin : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "BB";
    public override string ModuleVersion => "0.2.3";
    public override string ModuleAuthor => "exkludera";

    public static Plugin Instance = new();
    public Dictionary<int, Building.BuilderData> BuilderData = new();
    public bool buildMode = false;

    public override void Load(bool hotReload)
    {
        Instance = this;

        Events.Register();

        Commands.Load();

        Files.Load();

        if (hotReload)
        {
            // foreach (var player in Utilities.GetPlayers())
            // {
            //     if (Utils.HasPermission(player) || Files.Builders.steamids.Contains(player.SteamID.ToString()))
            //         BuilderData[player.Slot] = new Building.BuilderData { BlockType = Blocks.Models.Data.Platform.Title };
            // }
            foreach (var player in Utilities.GetPlayers())
            {
                if (!Lib.IsPlayerValid(player)) continue;
                BuilderData[player.Slot] = new Building.BuilderData { BlockType = Blocks.Models.Data.Platform.Title };
            }

            Files.mapsFolder = Path.Combine(ModuleDirectory, "maps", Server.MapName);
            Directory.CreateDirectory(Files.mapsFolder);

            Utils.Clear();

            Files.EntitiesData.LoadDefault();
        }
    }

    public override void Unload(bool hotReload)
    {
        Events.Deregister();

        Commands.Unload();

        Utils.Clear();
    }

    public Config Config { get; set; } = new();
    public void OnConfigParsed(Config config)
    {
        Config = config;
        Config.Settings.Prefix = StringExtensions.ReplaceColorTags(config.Settings.Prefix);

        buildMode = config.Settings.Building.BuildMode.Enable;
    }
}