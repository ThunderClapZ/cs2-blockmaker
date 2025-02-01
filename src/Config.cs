using CounterStrikeSharp.API.Core;

public class Config : BasePluginConfig
{
    public Config_Settings Settings { get; set; } = new Config_Settings();
    public Config_Commands Commands { get; set; } = new Config_Commands();
    public Config_Sounds Sounds { get; set; } = new Config_Sounds();
}

public class Config_Settings
{
    public string Prefix { get; set; } = "{purple}[BlockMaker]{default}";
    public string Menu { get; set; } = "html";

    public class Settings_Building
    {
        public bool BuildMode { get; set; } = true;
        public bool BuildModeConfig { get; set; } = false;
        public bool AutoSave { get; set; } = false;
        public int SaveTime { get; set; } = 300;
        public string BlockGrabColor { get; set; } = "255,255,255,128";
        public float[] GridValues { get; set; } = { 16f, 32f, 64f, 128f, 256f };
        public float[] RotationValues { get; set; } = { 15f, 30f, 45f, 60f, 90f, 120f };

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
        public string EntryModel { get; set; } = "models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl";
        public string EntryColor { get; set; } = "0,255,0,200";
        public string ExitModel { get; set; } = "models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl";
        public string ExitColor { get; set; } = "255,0,0,200";
    }
    public Settings_Teleports Teleports { get; set; } = new Settings_Teleports();
}

public class Config_Sounds
{
    public class Sounds_Blocks
    {
        public string Speed { get; set; } = "sounds/bootsofspeed.vsnd";
        public string Camouflage { get; set; } = "sounds/camouflage.vsnd";
        public string Damage { get; set; } = "sounds/dmg.vsnd";
        public string Health { get; set; } = "sounds/heartbeat.vsnd";
        public string Invincibility { get; set; } = "sounds/invincibility.vsnd";
        public string Nuke { get; set; } = "sounds/nuke.vsnd";
        public string Stealth { get; set; } = "sounds/stealth.vsnd";
        public string Teleport { get; set; } = "sounds/teleport.vsnd";
    }
    public Sounds_Blocks Blocks { get; set; } = new Sounds_Blocks();

    public class Sounds_Building
    {
        public bool Enabled { get; set; } = true;
        public string Create { get; set; } = "sounds/buttons/blip1.vsnd";
        public string Delete { get; set; } = "sounds/buttons/blip2.vsnd";
        public string Place { get; set; } = "sounds/buttons/latchunlocked2.vsnd";
        public string Rotate { get; set; } = "sounds/buttons/button9.vsnd";
        public string Save { get; set; } = "sounds/buttons/bell1.vsnd";
    }
    public Sounds_Building Building { get; set; } = new Sounds_Building();
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
        public string BlockType { get; set; } = "type";
        public string BlockColor { get; set; } = "color";
        public string CopyBlock { get; set; } = "copy";
        public string ConvertBlock { get; set; } = "convert";
        public string SaveBlocks { get; set; } = "save";
        public string Snapping { get; set; } = "snap";
        public string Grid { get; set; } = "grid";
        public string Noclip { get; set; } = "nc";
        public string Godmode { get; set; } = "godmode";
        public string TestBlock { get; set; } = "test";
    }
    public Commands_Building Building { get; set; } = new Commands_Building();
}