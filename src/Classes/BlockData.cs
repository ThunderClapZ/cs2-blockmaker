using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static VectorUtils;

public class BlockData
{
    public BlockData(CBaseProp block, string blockTitle, string blockModel, string blockSize, string blockColor = "default")
    {
        Entity = block;
        Name = blockTitle;
        Model = blockModel;
        Size = blockSize;
        Color = blockColor;
    }

    public CBaseProp Entity;
    public string Name { get; private set; }
    public string Model { get; private set; }
    public string Size { get; private set; }
    public string Color { get; private set; }
}

public class SaveBlockData
{
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public string Size { get; set; } = "";
    public string Color { get; set; } = "";
    public VectorDTO Position { get; set; } = new VectorDTO(Vector.Zero);
    public QAngleDTO Rotation { get; set; } = new QAngleDTO(QAngle.Zero);
}
