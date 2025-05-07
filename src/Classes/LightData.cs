using CounterStrikeSharp.API.Core;

public class LightData
{
    public LightData
    (
        CDynamicLight light,
        string color = "White",
        string brightness = "1",
        string distance = "1000"
    )
    {
        Entity = light;
        Color = color;
        Brightness = brightness;
        Distance = distance;
    }

    public CDynamicLight Entity;
    public string Color { get; set; }
    public string Brightness { get; set; }
    public string Distance { get; set; }
}

public class SaveLightData
{
    public string Color { get; set; } = "";
    public string Brightness { get; set; } = "";
    public string Distance { get; set; } = "";
    public VectorUtils.VectorDTO Position { get; set; } = new();
    public VectorUtils.QAngleDTO Rotation { get; set; } = new();
}