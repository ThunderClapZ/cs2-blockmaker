using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Text.Json;

public partial class Blocks
{
    public static void Create(CCSPlayerController player)
    {
        var pawn = player.Pawn()!;

        Vector position = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.CameraServices!.OldPlayerViewOffsetZ);

        var hitPoint = RayTrace.TraceShape(position, pawn.EyeAngles!, false, true);

        if (hitPoint == null && !hitPoint.HasValue)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a valid location to create block");
            return;
        }

        var playerData = instance.playerData[player.Slot];

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

    public static Dictionary<CBaseEntity, BlockData> BlocksEntities = new Dictionary<CBaseEntity, BlockData>();
    private static void CreateBlock(string type, string model, string size, Vector position, QAngle rotation, string color = "None", string transparency = "0%", string team = "Both")
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

            BlocksEntities[block] = new BlockData(block, type, model, size, color, transparency, team);
        }
    }

    public static Dictionary<CEntityInstance, CEntityInstance> BlockTriggers = new Dictionary<CEntityInstance, CEntityInstance>();
    private static void CreateTrigger(CBaseEntity block, string size)
    {
        var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");

        if (trigger != null && trigger.IsValid && trigger.Entity != null)
        {
            trigger.Entity.Name = "blockmaker_" + block.Entity!.Name + "_trigger";
            trigger.Spawnflags = 1;
            trigger.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);

            trigger.DispatchSpawn();
            trigger.AcceptInput("SetScale", trigger, trigger, Utils.GetSize(size).ToString());

            trigger.Teleport(block.AbsOrigin, block.AbsRotation);

            block.AcceptInput("SetParent", trigger, block, "!activator");

            BlockTriggers.Add(trigger, block);
        }
    }

    public static void Spawn()
    {
        bool isValidJson = Utils.IsValidJson(Files.blocksPath);

        if (isValidJson)
        {
            var jsonString = File.ReadAllText(Files.blocksPath);

            var blockDataList = JsonSerializer.Deserialize<List<SaveBlockData>>(jsonString);

            if (jsonString == null || blockDataList == null)
                return;

            foreach (var blockData in blockDataList)
            {
                var position = new Vector(blockData.Position.X, blockData.Position.Y, blockData.Position.Z);
                var rotation = new QAngle(blockData.Rotation.Pitch, blockData.Rotation.Yaw, blockData.Rotation.Roll);

                CreateBlock(blockData.Name, blockData.Model, blockData.Size, position, rotation, blockData.Color, blockData.Transparency, blockData.Team);
            }
        }
        else
        {
            Utils.PrintToChatAll($"{ChatColors.Red}Failed to spawn Blocks. File for {Utils.GetMapName()} is empty or invalid");
            Utils.Log($"Failed to spawn Blocks. File for {Utils.GetMapName()} is empty or invalid");
        }
    }
}
