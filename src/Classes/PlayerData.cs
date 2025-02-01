using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public class PlayerData
{
    public bool Builder = false;
    public bool Pole = false;
    public string BlockType = "Platform";
    public string BlockSize = "Normal";
    public string BlockTeam = "Both";
    public string BlockColor = "None";
    public string BlockTransparency = "100%";
    public bool Grid = false;
    public float GridValue = 32f;
    public float RotationValue = 30f;
    public bool Snapping = false;
    public bool Noclip = false;
    public bool Godmode = false;
    public string PropertyType = "";
    public Dictionary<string, CBaseEntity> PropertyEntity = new();
}

public class BuildingData
{
    public CBaseProp block = null!;
    public Vector offset = new();
    public QAngle EyeAngles = new();
    public int distance;
    public List<CBeam> beams = new();
}