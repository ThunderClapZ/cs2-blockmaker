using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

public partial class Blocks
{
    public class Light
    {
        public string Title { get; set; }
        public string RGB { get; set; }

        public Light(string title, string rgb)
        {
            Title = title;
            RGB = rgb;
        }
    }

    public static Dictionary<CBaseProp, LightData> Lights = new();
    public static void CreateLight(string color = "White", string brightness = "5", string distance = "1000", Vector? position = null, QAngle? rotation = null)
    {
        var light = Utilities.CreateEntityByName<CDynamicLight>("light_dynamic");
        if (light != null && light.IsValid && light.Entity != null)
        {
            Color clr = Utils.GetColor(color);
            light.Entity.Name = "blockmaker_light";
            light.Radius = 256;

            light.Teleport(position, rotation);
            light.DispatchSpawn();
            light.AcceptInput("brightness", light, light, brightness);
            light.AcceptInput("distance", light, light, distance);
            light.AcceptInput("color", light, light, $"{clr.R} {clr.G} {clr.B}");
            light.AcceptInput("TurnOn");

            var entity = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");
            if (entity != null && entity.IsValid && entity.Entity != null)
            {
                entity.Entity.Name = "blockmaker_light_entity";
                entity.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);

                if (!config.Settings.Building.Lights.HideModel || instance.buildMode)
                    entity.SetModel(config.Settings.Building.Lights.Model);

                else entity.Render = Color.Transparent;

                entity.Teleport(position, rotation);
                entity.DispatchSpawn();
                entity.AcceptInput("DisableMotion");

                light.AcceptInput("FollowEntity", entity, light, "!activator");

                Lights[entity] = new(light, color, brightness, distance);
            }
        }
    }
}
