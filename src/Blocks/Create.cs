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

        var hitPoint = RayTrace.TraceShape(position, pawn.EyeAngles!, false, true);
        if (hitPoint == null && !hitPoint.HasValue)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a valid location to create block");
            return;
        }

        string blockmodel = Utils.GetModelFromSelectedBlock(player, playerData.Pole);

        try
        {
            CreateBlock(playerData.BlockType, blockmodel, playerData.BlockSize, RayTrace.Vector3toVector(hitPoint.Value), new QAngle(), playerData.BlockColor, playerData.BlockTransparency, playerData.BlockTeam);

            if (config.Sounds.Building.Enabled)
                player.PlaySound(config.Sounds.Building.Create);

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
    public static void CreateBlock(string type, string model, string size, Vector position, QAngle rotation, string color = "None", string transparency = "100%", string team = "Both", BlockData_Properties? properties = null)
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

            block.SetModel(model);
            block.DispatchSpawn();
            block.AcceptInput("DisableMotion");
            block.AcceptInput("SetScale", block, block, Utils.GetSize(size).ToString());

            var pos = new Vector(position.X, position.Y, position.Z);
            var rot = new QAngle(rotation.X, rotation.Y, rotation.Z);
            block.Teleport(pos, rot);

            CreateTrigger(block, size);

            if (properties == null)
            {
                string match = type.Split('.')[0];

                var defaultProperties = Files.PropsData.Properties.BlockProperties;

                if (defaultProperties.ContainsKey(match))
                {
                    properties = new BlockData_Properties
                    {
                        Cooldown = defaultProperties[match].Cooldown,
                        Duration = defaultProperties[match].Duration,
                        Value = defaultProperties[match].Value,
                        OnTop = defaultProperties[match].OnTop
                    };
                }
                else properties = new BlockData_Properties();
            }

            Props[block] = new BlockData(block, type, model, size, color, transparency, team, properties);
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
