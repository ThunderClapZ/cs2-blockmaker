using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text.Json;

public partial class Blocks
{
    public static void Create(CCSPlayerController player)
    {
        var hitPoint = RayTrace.TraceShape(new Vector(player.PlayerPawn.Value!.AbsOrigin!.X, player.PlayerPawn.Value!.AbsOrigin!.Y, player.PlayerPawn.Value!.AbsOrigin!.Z + player.PlayerPawn.Value.CameraServices!.OldPlayerViewOffsetZ), player.PlayerPawn.Value!.EyeAngles!, false, true);

        if (hitPoint == null && !hitPoint.HasValue)
        {
            instance.PrintToChat(player, $"Create Block: {ChatColors.Red}Distance too large between block and aim location");
            return;
        }

        var playerData = instance.playerData[player.Slot];

        string selectedBlock = playerData.BlockType;

        if (string.IsNullOrEmpty(selectedBlock))
        {
            instance.PrintToChat(player, $"Create Block: Select a Block first");
            return;
        }

        string blockmodel = instance.GetModelFromSelectedBlock(player, playerData.BlockSize);

        try
        {
            CreateBlock(selectedBlock, blockmodel, playerData.BlockSize, RayTrace.Vector3toVector(hitPoint.Value), new QAngle(), playerData.BlockColor, playerData.BlockTransparency, playerData.BlockTeam);

            if (instance.Config.Sounds.Building.Enabled)
                player.PlaySound(instance.Config.Sounds.Building.Create);

            instance.PrintToChat(player, $"{ChatColors.Green}Created block");
        }
        catch
        {
            instance.PrintToChat(player, $"Create Block: {ChatColors.Red}Failed to create block");
            return;
        }
    }

    public static Dictionary<CBaseProp, BlockData> UsedBlocks = new Dictionary<CBaseProp, BlockData>();
    public static void CreateBlock(string type, string model, string size, Vector position, QAngle rotation, string color = "None", string transparency = "0%", string team = "Both")
    {
        var block = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");

        if (block != null && block.IsValid)
        {
            block.Entity!.Name = "blockmaker_" + type;
            block.EnableUseOutput = true;
            block.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            block.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

            var clr = Plugin.GetColor(color);
            int alpha = Plugin.GetAlpha(transparency);
            block.Render = Color.FromArgb(alpha, clr.R, clr.G, clr.B);
            Utilities.SetStateChanged(block, "CBaseModelEntity", "m_clrRender");

            block.SetModel(model);
            block.DispatchSpawn();
            block.AcceptInput("DisableMotion", block, block);
            block.AcceptInput("SetScale", block, block, Plugin.GetSize(size).ToString());

            var pos = new Vector(position.X, position.Y, position.Z);
            var rot = new QAngle(rotation.X, rotation.Y, rotation.Z);
            block.Teleport(pos, rot);

            CreateTrigger(block, size);

            UsedBlocks[block] = new BlockData(block, type, model, size, color, transparency, team);
        }

        else instance.Logger.LogError("(CreateBlock) failed to create block");
    }

    public static Dictionary<CEntityInstance, CEntityInstance> BlockTriggers = new Dictionary<CEntityInstance, CEntityInstance>();
    public static void CreateTrigger(CBaseEntity block, string size)
    {
        var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");

        if (trigger != null && trigger.IsValid)
        {
            trigger.Entity!.Name = "blockmaker_" + block.Entity!.Name + "_trigger";
            trigger.Spawnflags = 1;
            trigger.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);

            trigger.DispatchSpawn();
            trigger.AcceptInput("SetScale", trigger, trigger, Plugin.GetSize(size).ToString());
            trigger.Teleport(block.AbsOrigin, block.AbsRotation);

            block.AcceptInput("SetParent", trigger, block, "!activator");

            BlockTriggers.Add(trigger, block);
        }

        else instance.Logger.LogError("(CreateTrigger) failed to create trigger");
    }

    public static void Spawn()
    {
        bool isValidJson = instance.IsValidJson(savedPath);

        if (isValidJson)
        {
            var jsonString = File.ReadAllText(savedPath);

            var blockDataList = JsonSerializer.Deserialize<List<SaveBlockData>>(jsonString);

            if (jsonString == null || blockDataList == null || jsonString.ToString() == "[]")
            {
                instance.PrintToChatAll($"{ChatColors.Red}Failed to spawn Blocks. File for {instance.GetMapName()} is empty or invalid");
                return;
            }

            foreach (var blockData in blockDataList)
            {
                var position = new Vector(blockData.Position.X, blockData.Position.Y, blockData.Position.Z);
                var rotation = new QAngle(blockData.Rotation.Pitch, blockData.Rotation.Yaw, blockData.Rotation.Roll);

                CreateBlock(blockData.Name, blockData.Model, blockData.Size, position, rotation, blockData.Color, blockData.Transparency, blockData.Team);
            }
        }
        else
        {
            instance.PrintToChatAll($"{ChatColors.Red}Failed to spawn Blocks. File for {instance.GetMapName()} is empty or invalid");
            instance.Logger.LogError($"Failed to spawn Blocks. File for {instance.GetMapName()} is empty or invalid");
        }
    }
}
