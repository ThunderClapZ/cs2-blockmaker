using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json.Serialization;
using System.Text.Json;
using static Files;

public partial class Blocks
{
    public class Property
    {
        public float Cooldown { get; set; } = 0;
        public float Value { get; set; } = 0;
        public float Duration { get; set; } = 0;
        public bool OnTop { get; set; } = true;
        public bool Locked { get; set; } = false;
        public string Builder { get; set; } = "";
    }

    public class Data
    {
        public Data
        (
            CBaseProp block,
            string type,
            bool pole,
            string size = "Normal",
            string color = "None",
            string transparency = "100%",
            string team = "Both",
            string effect = "None",
            Property properties = null!

        )
        {
            Entity = block;
            Type = type;
            Pole = pole;
            Size = size;
            Team = team;
            Color = color;
            Transparency = transparency;
            Effect = effect;
            Properties = properties;
        }

        public CBaseProp Entity;
        public string Type { get; set; }
        public bool Pole { get; set; }
        public string Size { get; set; }
        public string Team { get; set; }
        public string Color { get; set; }
        public string Transparency { get; set; }
        public string Effect { get; set; }
        public Property Properties { get; set; }
    }

    public class SaveData
    {
        public string Type { get; set; } = "";
        public bool Pole { get; set; } = false;
        public string Size { get; set; } = "";
        public string Team { get; set; } = "";
        public string Color { get; set; } = "";
        public string Transparency { get; set; } = "";
        public string Effect { get; set; } = "";
        public Property Properties { get; set; } = new Property();
        public VectorUtils.VectorDTO Position { get; set; } = new VectorUtils.VectorDTO(Vector.Zero);
        public VectorUtils.QAngleDTO Rotation { get; set; } = new VectorUtils.QAngleDTO(QAngle.Zero);
    }

    public static class Properties
    {
        public static readonly Dictionary<string, Property> BlockDefaultProperties = new()
            {
                { Models.Entities.Bhop.Title, new Property { Duration = 0.25f, Cooldown = 1.0f } },
                { Models.Entities.Health.Title, new Property { Value = 4.0f, Cooldown = 0.75f } },
                { Models.Entities.Grenade.Title, new Property { Cooldown = 60.0f } },
                { Models.Entities.Gravity.Title, new Property { Duration = 4.0f, Value = 0.4f, Cooldown = 5.0f } },
                { Models.Entities.Frost.Title, new Property { Cooldown = 60.0f } },
                { Models.Entities.Flash.Title, new Property { Cooldown = 60.0f } },
                { Models.Entities.Fire.Title, new Property { Duration = 5.0f, Value = 8.0f, Cooldown = 5.0f } },
                { Models.Entities.Delay.Title, new Property { Duration = 1.0f, Cooldown = 1.5f } },
                { Models.Entities.Damage.Title, new Property { Value = 8.0f, Cooldown = 0.75f } },
                { Models.Entities.Stealth.Title, new Property { Duration = 7.5f, Cooldown = 60.0f } },
                { Models.Entities.Speed.Title, new Property { Duration = 3.0f, Value = 2.0f, Cooldown = 60.0f } },
                { Models.Entities.SpeedBoost.Title, new Property { Duration = 300.0f, Value = 650.0f } },
                { Models.Entities.Camouflage.Title, new Property { Duration = 10.0f, Cooldown = 60.0f } },
                { Models.Entities.Slap.Title, new Property { Value = 2.0f } },
                { Models.Entities.Random.Title, new Property { Cooldown = 60f } },
                { Models.Entities.Invincibility.Title, new Property { Duration = 5.0f, Cooldown = 60.0f } },
                { Models.Entities.Trampoline.Title, new Property { Value = 500.0f } },
                { Models.Entities.Death.Title, new Property { OnTop = false } },
                { Models.Entities.Honey.Title, new Property { Value = 0.3f } },
                { Models.Entities.Platform.Title, new Property() },
                { Models.Entities.NoFallDmg.Title, new Property { OnTop = false } },
                { Models.Entities.Ice.Title, new Property() },
                { Models.Entities.Nuke.Title, new Property() },
                { Models.Entities.Glass.Title, new Property() },
                { Models.Entities.Pistol.Title, new Property{  Value = 1f, Cooldown = 999f } },
                { Models.Entities.Rifle.Title, new Property{ Value = 1f, Cooldown = 999f } },
                { Models.Entities.Sniper.Title, new Property{ Value = 1f, Cooldown = 999f } },
                { Models.Entities.ShotgunHeavy.Title, new Property{ Value = 1f, Cooldown = 999f } },
                { Models.Entities.SMG.Title, new Property{ Value = 1f, Cooldown = 999f } },
            };

        public static Dictionary<string, Property> BlockProperties { get; set; } = new Dictionary<string, Property>();

        public static void Load()
        {
            string directoryPath = Path.GetDirectoryName(Plugin.Instance.Config.GetConfigPath())!;
            string correctedPath = directoryPath.Replace("/BlockMaker.json", "");

            var propertiesPath = Path.Combine(correctedPath, "default_properties.json");

            if (!string.IsNullOrEmpty(propertiesPath))
            {
                if (!File.Exists(propertiesPath))
                {
                    using (FileStream fs = File.Create(propertiesPath))
                        fs.Close();

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    string jsonContent = JsonSerializer.Serialize(BlockDefaultProperties, options);

                    File.WriteAllText(propertiesPath, jsonContent);
                }

                if (File.Exists(propertiesPath))
                {
                    string jsonContent = File.ReadAllText(propertiesPath);
                    BlockProperties = JsonSerializer.Deserialize<Dictionary<string, Property>>(jsonContent) ?? new Dictionary<string, Property>();
                }
            }
        }
    }
}