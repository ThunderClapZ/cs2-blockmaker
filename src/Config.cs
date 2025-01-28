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
    public bool RemoveWeapons { get; set; } = true;

    public class Settings_Building
    {
        public bool BuildMode { get; set; } = false;
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

        public class Settings_Block
        {
            public float Duration { get; set; }
            public float Cooldown { get; set; }
            public float Value { get; set; }
        }

        public Settings_Block Bhop { get; set; } = new Settings_Block { Duration = 0.25f, Cooldown = 1.5f };
        public Settings_Block Health { get; set; } = new Settings_Block { Value = 2.0f, Cooldown = 0.5f };
        public Settings_Block Grenade { get; set; } = new Settings_Block { Cooldown = 60.0f };
        public Settings_Block Gravity { get; set; } = new Settings_Block { Duration = 4.0f, Value = 0.4f, Cooldown = 5.0f };
        public Settings_Block Frost { get; set; } = new Settings_Block { Cooldown = 60.0f };
        public Settings_Block Flash { get; set; } = new Settings_Block { Cooldown = 60.0f };
        public Settings_Block Fire { get; set; } = new Settings_Block { Duration = 5.0f, Value = 8.0f, Cooldown = 5.0f };
        public Settings_Block Delay { get; set; } = new Settings_Block { Duration = 1.0f, Cooldown = 1.5f };
        public Settings_Block Damage { get; set; } = new Settings_Block { Value = 5.0f, Cooldown = 0.5f };
        public Settings_Block Stealth { get; set; } = new Settings_Block { Duration = 10.0f, Cooldown = 60.0f };
        public Settings_Block Speed { get; set; } = new Settings_Block { Duration = 3.0f, Value = 2.0f, Cooldown = 60.0f };
        public Settings_Block SpeedBoost { get; set; } = new Settings_Block { Value = 650.0f };
        public Settings_Block Slap { get; set; } = new Settings_Block { Value = 2.0f };
        public Settings_Block Random { get; set; } = new Settings_Block { Cooldown = 60.0f };
        public Settings_Block Invincibility { get; set; } = new Settings_Block { Duration = 5.0f, Cooldown = 60.0f };
        public Settings_Block Trampoline { get; set; } = new Settings_Block { Value = 500.0f };

        //public Settings_Block Death { get; set; } = new Settings_Block();
        //public Settings_Block Deagle { get; set; } = new Settings_Block();
        //public Settings_Block AWP { get; set; } = new Settings_Block();
        //public Settings_Block Nuke { get; set; } = new Settings_Block();

        //public Settings_Block NoFallDmg { get; set; } = new Settings_Block();
        //public Settings_Block Ice { get; set; } = new Settings_Block();
        //public Settings_Block Glass { get; set; } = new Settings_Block();
        //public Settings_Block TBarrier { get; set; } = new Settings_Block();
        //public Settings_Block CTBarrier { get; set; } = new Settings_Block();
        //public Settings_Block NoSlowDown { get; set; } = new Settings_Block();
        //public Settings_Block Honey { get; set; } = new Settings_Block();

        public class Settings_BlockCamouflage : Settings_Block
        {
            public string ModelT { get; set; } = "characters/models/ctm_fbi/ctm_fbi.vmdl";
            public string ModelCT { get; set; } = "characters/models/tm_leet/tm_leet_variantb.vmdl";
        }
        public Settings_BlockCamouflage Camouflage { get; set; } = new Settings_BlockCamouflage { Duration = 10.0f, Cooldown = 60.0f};
    }
    public Settings_Blocks Blocks { get; set; } = new Settings_Blocks();
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