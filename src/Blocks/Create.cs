using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

public partial class Blocks
{
    public static void Create(CCSPlayerController player)
    {
        var playerData = instance.playerData[player.Slot];

        if (playerData.BlockType == "Teleport")
        {
            CreateTeleport(player);
            return;
        }

        var pawn = player.Pawn()!;
        Vector position = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.CameraServices!.OldPlayerViewOffsetZ);

        var hitPoint = RayTrace.TraceShape(position, pawn.EyeAngles!);
        if (hitPoint == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a valid location to create block");
            return;
        }

        try
        {
            CreateBlock(
                playerData.BlockType,
                playerData.BlockPole,
                playerData.BlockSize,
                hitPoint,
                new QAngle(),
                playerData.BlockColor,
                playerData.BlockTransparency,
                playerData.BlockTeam
            );

            if (config.Sounds.Building.Enabled)
            {
                var sound = config.Sounds.Building.Create;
                player.PlaySound(sound.Event, sound.Volume);
            }

            Utils.PrintToChat(player, $"Created -" +
                $" type: {ChatColors.White}{playerData.BlockType}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{playerData.BlockSize}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{playerData.BlockColor}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{playerData.BlockTeam}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{playerData.BlockTransparency}");
        }
        catch
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Failed to create block");
            return;
        }
    }

    public static Dictionary<CBaseEntity, BlockData> Props = new Dictionary<CBaseEntity, BlockData>();
    public static void CreateBlock(
        string type,
        bool pole,
        string size,
        Vector position,
        QAngle rotation,
        string color = "None",
        string transparency = "100%",
        string team = "Both",
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
            block.DispatchSpawn();
            block.AcceptInput("DisableMotion");
            block.AcceptInput("SetScale", block, block, Utils.GetSize(size).ToString());
            block.Teleport(position, rotation);

            CreateTrigger(block, size);

            if (properties == null || (properties.Cooldown == 0 && properties.Duration == 0 && properties.Value == 0 && properties.Locked != true))
            {
                if (Files.PropsData.Properties.BlockDefaultProperties.TryGetValue(type.Split('.')[0], out var defaultProperties))
                    properties = new BlockData_Properties
                    {
                        Cooldown = defaultProperties.Cooldown,
                        Value = defaultProperties.Value,
                        Duration = defaultProperties.Duration,
                        OnTop = defaultProperties.OnTop,
                    };

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
                };
            }

            Props[block] = new BlockData(block, type, pole, size, color, transparency, team, properties);
        }
    }

    public static Dictionary<CEntityInstance, CBaseProp> Triggers = new Dictionary<CEntityInstance, CBaseProp>();
    private static void CreateTrigger(CBaseProp block, string size)
    {
        var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");

        if (trigger != null && trigger.IsValid && trigger.Entity != null)
        {
            trigger.Entity.Name = block.Entity!.Name + "_trigger";
            trigger.Spawnflags = 1;
            trigger.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);

            trigger.DispatchSpawn();
            trigger.AcceptInput("SetScale", trigger, trigger, Utils.GetSize(size).ToString());
            trigger.Teleport(block.AbsOrigin, block.AbsRotation);

            block.AcceptInput("SetParent", trigger, block, "!activator");

            Triggers.Add(trigger, block);
        }
    }
}
