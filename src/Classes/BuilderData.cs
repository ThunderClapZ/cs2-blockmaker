using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public class BuilderData
{
    public string BlockType = "Platform";
    public bool BlockPole = false;
    public string BlockSize = "Normal";
    public string BlockTeam = "Both";
    public string BlockColor = "None";
    public string BlockTransparency = "100%";
    public Blocks.Effect BlockEffect = new("None", "");
    public string LightColor = "White";
    public string LightBrightness = "5";
    public string LightDistance = "1000";
    public bool Grid = false;
    public float GridValue = 32f;
    public float SnapValue = 0f;
    public float RotationValue = 90f;
    public float PositionValue = 8f;
    public string MoveAngle = "X+";
    public bool Snapping = false;
    public bool Noclip = false;
    public bool Godmode = false;
    public string ChatInput = "";
    public Dictionary<string, CBaseEntity> PropertyEntity = new();
}

public class BuildingData
{
    public CBaseProp block = null!;
    public Vector offset = new();
    public int distance = 0;
    public List<CBeam> beams = new();
    public bool lockedmessage = false;
}