using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public class BlockData
{
    public BlockData(CBaseProp block, string title, string model, string size, string color = "None", string transparency = "0%", string team = "Both")
    {
        Entity = block;
        Name = title;
        Model = model;
        Size = size;
        Team = team;
        Color = color;
        Transparency = transparency;
    }

    public CBaseProp Entity;
    public string Name { get; set; }
    public string Model { get; set; }
    public string Size { get; set; }
    public string Team { get; set; }
    public string Color { get; set; }
    public string Transparency { get; set; }
}

public class SaveBlockData
{
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public string Size { get; set; } = "";
    public string Team { get; set; } = "";
    public string Color { get; set; } = "";
    public string Transparency { get; set; } = "";
    public VectorUtils.VectorDTO Position { get; set; } = new VectorUtils.VectorDTO(Vector.Zero);
    public VectorUtils.QAngleDTO Rotation { get; set; } = new VectorUtils.QAngleDTO(QAngle.Zero);
}
