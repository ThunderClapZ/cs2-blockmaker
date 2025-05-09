using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2TraceRay.Class;
using CS2TraceRay.Enum;
using System.Drawing;

public partial class Lights
{
    public class Config_Light
    {
        public string Title { get; set; }
        public string RGB { get; set; }

        public Config_Light(string title, string rgb)
        {
            Title = title;
            RGB = rgb;
        }
    }

    public class Data
    {
        public Data
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

    public class SaveData
    {
        public string Color { get; set; } = "";
        public string Brightness { get; set; } = "";
        public string Distance { get; set; } = "";
        public VectorUtils.VectorDTO Position { get; set; } = new();
        public VectorUtils.QAngleDTO Rotation { get; set; } = new();
    }

    private static Plugin Instance = Plugin.Instance;
    private static Config Config = Instance.Config;

    public static Dictionary<CBaseProp, Data> Entities = new();

    public static void Create(CCSPlayerController player)
    {
        var BuilderData = Instance.BuilderData[player.Slot];

        CGameTrace? trace = TraceRay.TraceShape(player.GetEyePosition()!, player.PlayerPawn.Value?.EyeAngles!, TraceMask.MaskShot, player);
        if (trace == null || !trace.HasValue || trace.Value.Position.Length() == 0)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a valid location to create light");
            return;
        }

        var endPos = trace.Value.Position;

        CreateEntity(BuilderData.LightColor, BuilderData.LightBrightness, BuilderData.LightDistance, new(endPos.X, endPos.Y, endPos.Z), player.AbsRotation);
        Utils.PrintToChat(player, $"Created Light -" +
            $" color: {ChatColors.White}{BuilderData.LightColor}{ChatColors.Grey}," +
            $" brightness: {ChatColors.White}{BuilderData.LightBrightness}{ChatColors.Grey}," +
            $" distance: {ChatColors.White}{BuilderData.LightDistance}"
        );
    }

    public static void CreateEntity(string color = "White", string brightness = "5", string distance = "1000", Vector? position = null, QAngle? rotation = null)
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

                if (!Config.Settings.Lights.HideModel || Instance.buildMode)
                    entity.SetModel(Instance.Config.Settings.Lights.Model);

                else entity.Render = Color.Transparent;

                entity.Teleport(position, rotation);
                entity.DispatchSpawn();
                entity.AcceptInput("DisableMotion");

                light.AcceptInput("FollowEntity", entity, light, "!activator");

                Entities[entity] = new(light, color, brightness, distance);
            }
        }
    }

    public static bool Delete(CCSPlayerController player, bool message = true)
    {
        var entity = player.GetBlockAim();

        if (entity != null && Entities.TryGetValue(entity, out var light))
        {
            light.Entity.Remove();
            Entities.Remove(entity);
            entity.Remove();

            if (message)
                Utils.PrintToChat(player, $"Deleted Light -" +
                    $" color: {ChatColors.White}{light.Color}{ChatColors.Grey}," +
                    $" brightness: {ChatColors.White}{light.Brightness}{ChatColors.Grey}," +
                    $" distance: {ChatColors.White}{light.Distance}"
                );

            return true;
        }
        else
        {
            if (message)
                Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a light to delete");

            return false;
        }
    }

    public static void Settings(CCSPlayerController player, string type, string input)
    {
        var data = Instance.BuilderData[player.Slot];

        switch (type)
        {
            case "LightBrightness":
                data.LightBrightness = input;
                Utils.PrintToChat(player, $"LightBrightness Value: {ChatColors.White}{input}");
                if (Delete(player, false))
                    Create(player);
                break;
            case "LightDistance":
                data.LightDistance = input;
                Utils.PrintToChat(player, $"LightDistance Value: {ChatColors.White}{input}");
                if (Delete(player, false))
                    Create(player);
                break;
            default:
                Utils.PrintToChat(player, $"{ChatColors.Red}Unknown property type: {type}");
                break;
        }

        data.ChatInput = "";
    }
}
