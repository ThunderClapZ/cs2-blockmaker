using CounterStrikeSharp.API.Core;

public class Config : BasePluginConfig
{
    public Config_Settings Settings { get; set; } = new Config_Settings();
    public Config_Commands Commands { get; set; } = new Config_Commands();
    public Config_Sounds Sounds { get; set; } = new Config_Sounds();
}

public class Config_Settings
{
    public string Prefix { get; set; } = "{purple}BlockMaker {grey}|";
    public string Menu { get; set; } = "html";

    public class Settings_Building
    {
        public bool BuildMode { get; set; } = true;
        public bool BuildModeConfig { get; set; } = false;
        public bool AutoSave { get; set; } = false;
        public int SaveTime { get; set; } = 300;
        public string BlockGrabColor { get; set; } = "255,255,255,128";

        public List<BlockSize> BlockSizes { get; set; }
        public Settings_Building()
        {
            BlockSizes = new List<BlockSize>
            {
                new BlockSize("Small", 0.5f),
                new BlockSize("Normal", 1.0f),
                new BlockSize("Large", 2.0f),
                new BlockSize("X-Large", 3.0f)
            };
        }
    }
    public Settings_Building Building { get; set; } = new Settings_Building();

    public class Settings_Blocks
    {
        public bool DisableShadows { get; set; } = true;
        public string CamouflageT { get; set; } = "characters/models/ctm_fbi/ctm_fbi.vmdl";
        public string CamouflageCT { get; set; } = "characters/models/tm_leet/tm_leet_variantb.vmdl";
        public string FireParticle { get; set; } = "particles/burning_fx/env_fire_medium.vpcf";
    }
    public Settings_Blocks Blocks { get; set; } = new Settings_Blocks();

    public class Settings_Teleports
    {
        public bool ForceAngles { get; set; } = false;
        public string EntryModel { get; set; } = "models/blockmaker/teleport/model.vmdl";
        public string EntryColor { get; set; } = "0,255,0,255";
        public string ExitModel { get; set; } = "models/blockmaker/teleport/model.vmdl";
        public string ExitColor { get; set; } = "255,0,0,255";
    }
    public Settings_Teleports Teleports { get; set; } = new Settings_Teleports();
}

public class Config_Commands
{
    public class Commands_Admin
    {
        public string Permission { get; set; } = "@css/root";
        public string BuildMode { get; set; } = "buildmode";
        public string ManageBuilder { get; set; } = "builder,builders";
    }
    public Commands_Admin Admin { get; set; } = new Commands_Admin();

    public class Commands_Building
    {
        public string BuildMenu { get; set; } = "bm,buildmenu";
        public string CreateBlock { get; set; } = "create";
        public string DeleteBlock { get; set; } = "delete";
        public string RotateBlock { get; set; } = "rotate";
        public string PositionBlock { get; set; } = "position";
        public string BlockType { get; set; } = "type";
        public string BlockColor { get; set; } = "color";
        public string CopyBlock { get; set; } = "copy";
        public string ConvertBlock { get; set; } = "convert";
        public string LockBlock { get; set; } = "lock";
        public string SaveBlocks { get; set; } = "save";
        public string Snapping { get; set; } = "snap";
        public string Grid { get; set; } = "grid";
        public string Noclip { get; set; } = "nc";
        public string Godmode { get; set; } = "godmode";
        public string TestBlock { get; set; } = "testblock";
    }
    public Commands_Building Building { get; set; } = new Commands_Building();
}

public class Config_Sounds
{
    public string SoundEvents { get; set; } = "soundevents/blockmaker.vsndevts";

    public class Sound
    {
        public string Event { get; set; } = "";
        public float Volume { get; set; } = 1.0f;
    }

    public class Sounds_Blocks
    {
        public Sound Speed { get; set; } = new Sound { Event = "speed", Volume = 1.0f };
        public Sound Camouflage { get; set; } = new Sound { Event = "camouflage", Volume = 1.0f };
        public Sound Damage { get; set; } = new Sound { Event = "damage", Volume = 1.0f };
        public Sound Health { get; set; } = new Sound { Event = "health", Volume = 1.0f };
        public Sound Invincibility { get; set; } = new Sound { Event = "invincibility", Volume = 1.0f };
        public Sound Nuke { get; set; } = new Sound { Event = "nuke", Volume = 1.0f };
        public Sound Stealth { get; set; } = new Sound { Event = "stealth", Volume = 1.0f };
        public Sound Teleport { get; set; } = new Sound { Event = "teleport", Volume = 1.0f };
    }
    public Sounds_Blocks Blocks { get; set; } = new Sounds_Blocks();

    public class Sounds_Building
    {
        public bool Enabled { get; set; } = true;
        public Sound Create { get; set; } = new Sound { Event = "create", Volume = 1.0f };
        public Sound Delete { get; set; } = new Sound { Event = "delete", Volume = 1.0f };
        public Sound Place { get; set; } = new Sound { Event = "place", Volume = 1.0f };
        public Sound Rotate { get; set; } = new Sound { Event = "rotate", Volume = 1.0f };
        public Sound Save { get; set; } = new Sound { Event = "save", Volume = 1.0f };
    }
    public Sounds_Building Building { get; set; } = new Sounds_Building();
}