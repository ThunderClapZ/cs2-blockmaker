using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public class BlockData_Properties
{
    public float Cooldown { get; set; } = 0;
    public float Value { get; set; } = 0;
    public float Duration { get; set; } = 0;
}

public class BlockData
{
    public BlockData
    (
        CBaseProp block,
        string name,
        string model,
        string size = "Normal",
        string color = "None",
        string transparency = "100%",
        string team = "Both",
        BlockData_Properties properties = null!

    )
    {
        Entity = block;
        Name = name;
        Model = model;
        Size = size;
        Team = team;
        Color = color;
        Transparency = transparency;
        Properties = properties;
    }

    public CBaseProp Entity;
    public string Name { get; set; }
    public string Model { get; set; }
    public string Size { get; set; }
    public string Team { get; set; }
    public string Color { get; set; }
    public string Transparency { get; set; }
    public BlockData_Properties Properties { get; set; }
}

public class SaveBlockData
{
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public string Size { get; set; } = "";
    public string Team { get; set; } = "";
    public string Color { get; set; } = "";
    public string Transparency { get; set; } = "";
    public BlockData_Properties Properties { get; set; } = new BlockData_Properties();
    public VectorUtils.VectorDTO Position { get; set; } = new VectorUtils.VectorDTO(Vector.Zero);
    public VectorUtils.QAngleDTO Rotation { get; set; } = new VectorUtils.QAngleDTO(QAngle.Zero);
}