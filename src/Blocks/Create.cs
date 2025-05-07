using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2TraceRay.Class;
using CS2TraceRay.Enum;
using System.Drawing;

public partial class Blocks
{
    public static void Create(CCSPlayerController player)
    {
        var pawn = player.Pawn()!;
        Vector position = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.CameraServices!.OldPlayerViewOffsetZ);

        CGameTrace? trace = TraceRay.TraceShape(player.GetEyePosition()!, pawn.EyeAngles, TraceMask.MaskShot, player);
        if (trace == null || !trace.HasValue || trace.Value.Position.Length() == 0)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a valid location to create block");
            return;
        }

        var endPos = trace.Value.Position;
        var BuilderData = instance.BuilderData[player.Slot];

        try
        {
            CreateBlock(
                player,
                BuilderData.BlockType,
                BuilderData.BlockPole,
                BuilderData.BlockSize,
                new(endPos.X, endPos.Y, endPos.Z),
                null!,
                BuilderData.BlockColor,
                BuilderData.BlockTransparency,
                BuilderData.BlockTeam,
                BuilderData.BlockEffect?.Particle ?? "None"
            );

            if (config.Sounds.Building.Enabled)
                player.EmitSound(config.Sounds.Building.Create);

            Utils.PrintToChat(player, $"Created -" +
                $" type: {ChatColors.White}{BuilderData.BlockType}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{BuilderData.BlockSize}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{BuilderData.BlockColor}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{BuilderData.BlockTeam}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{BuilderData.BlockTransparency},"
            );
        }
        catch
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Failed to create block");
            return;
        }
    }

    public static Dictionary<CBaseEntity, BlockData> Props = new();
    public static void CreateBlock(
        CCSPlayerController? player,
        string type,
        bool pole,
        string size,
        Vector position,
        QAngle rotation,
        string color = "None",
        string transparency = "100%",
        string team = "Both",
        string effect = "None",
        BlockData_Properties? properties = null
    )
    {
        var block = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");

        if (block != null && block.IsValid && block.Entity != null)
        {
            block.Entity.Name = "blockmaker_" + type;
            block.EnableUseOutput = true;
            block.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            block.ShadowStrength = config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

            var clr = Utils.GetColor(color);
            int alpha = Utils.GetAlpha(transparency);
            block.Render = Color.FromArgb(alpha, clr.R, clr.G, clr.B);
            Utilities.SetStateChanged(block, "CBaseModelEntity", "m_clrRender");

            string model = Utils.GetModelFromSelectedBlock(type, pole);
            block.SetModel(model);

            block.Teleport(position, rotation);
            block.DispatchSpawn();
            block.AcceptInput("DisableMotion");
            block.AcceptInput("SetScale", block, block, Utils.GetSize(size).ToString());

            if (!string.IsNullOrEmpty(effect) && effect != "None")
                CreateParticle(block, effect, size);

            CreateTrigger(block, size);

            if (properties == null)
            {
                if (Files.PropsData.Properties.BlockProperties.TryGetValue(type.Split('.')[0], out var defaultProperties))
                {
                    properties = new BlockData_Properties
                    {
                        Cooldown = defaultProperties.Cooldown,
                        Value = defaultProperties.Value,
                        Duration = defaultProperties.Duration,
                        OnTop = defaultProperties.OnTop,
                        Locked = defaultProperties.Locked,
                    };
                    if (player != null) properties.Builder = $"{player.PlayerName} - {DateTime.Now:dd/MMMM/yyyy HH:mm}";
                }
                else properties = new BlockData_Properties();
            }
            else
            {
                properties = new BlockData_Properties
                {
                    Cooldown = properties.Cooldown,
                    Value = properties.Value,
                    Duration = properties.Duration,
                    OnTop = properties.OnTop,
                    Locked = properties.Locked,
                    Builder = properties.Builder,
                };
            }

            Props[block] = new BlockData(block, type, pole, size, color, transparency, team, effect, properties);
        }
    }

    public static Dictionary<CEntityInstance, CBaseProp> Triggers = new Dictionary<CEntityInstance, CBaseProp>();
    private static void CreateTrigger(CBaseProp block, string size)
    {
        var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");

        if (trigger != null && trigger.IsValid && trigger.Entity != null)
        {
            trigger.Spawnflags = 1;
            trigger.Entity.Name = block.Entity!.Name + "_trigger";
            trigger.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);

            trigger.Teleport(block.AbsOrigin, block.AbsRotation);
            trigger.AcceptInput("SetScale", trigger, trigger, Utils.GetSize(size).ToString());
            trigger.DispatchSpawn();

            block.AcceptInput("SetParent", trigger, block, "!activator");

            Triggers.Add(trigger, block);
        }
    }
}
