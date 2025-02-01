using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public class TeleportData
{
    public TeleportData
    (
        CBaseProp teleport,
        string name,
        string model
    )
    {
        Entity = teleport;
        Name = name;
        Model = model;
    }

    public CBaseProp Entity;
    public string Name { get; set; }
    public string Model { get; set; }
}

public class SaveTeleportData
{
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public VectorUtils.VectorDTO Position { get; set; } = new VectorUtils.VectorDTO(Vector.Zero);
    public VectorUtils.QAngleDTO Rotation { get; set; } = new VectorUtils.QAngleDTO(QAngle.Zero);
}

public class TeleportPair
{
    public TeleportData Entry { get; set; }
    public TeleportData Exit { get; set; }

    public TeleportPair(TeleportData entry, TeleportData exit)
    {
        Entry = entry;
        Exit = exit;
    }
}

public class TeleportPairDTO
{
    public SaveTeleportData Entry { get; set; } = new SaveTeleportData();
    public SaveTeleportData Exit { get; set; } = new SaveTeleportData();
}