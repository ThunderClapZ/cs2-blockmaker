using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
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

        string selectedBlock = instance.playerData[player.Slot].BlockType;

        if (string.IsNullOrEmpty(selectedBlock))
        {
            instance.PrintToChat(player, $"Create Block: Select a Block first");
            return;
        }

        string blockmodel = instance.GetModelFromSelectedBlock(player, instance.playerData[player.Slot].BlockSize);

        try
        {
            CreateBlock(selectedBlock, blockmodel, instance.playerData[player.Slot].BlockSize, RayTrace.Vector3toVector(hitPoint.Value), new QAngle(), instance.playerData[player.Slot].BlockColor);

            if (instance.Config.Sounds.Building.Enabled)
                player.PlaySound(instance.Config.Sounds.Building.Create);

            instance.PrintToChat(player, $"Create Block: Created type: {ChatColors.White}{instance.playerData[player.Slot].BlockType}{ChatColors.Grey}, size: {ChatColors.White}{instance.playerData[player.Slot].BlockSize}");
        }
        catch
        {
            instance.PrintToChat(player, $"Create Block: Failed to create block");
            return;
        }
    }

    public static Dictionary<CBaseProp, BlockData> UsedBlocks = new Dictionary<CBaseProp, BlockData>();
    public static void CreateBlock(string blockType, string blockModel, string blockSize, Vector blockPosition, QAngle blockRotation, string blockColor = "default")
    {
        var block = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");

        if (block != null && block.IsValid)
        {
            block.Entity!.Name = "blockmaker_" + blockType;
            block.EnableUseOutput = true;
            block.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            block.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

            block.Render = Plugin.GetColor(blockColor);
            Utilities.SetStateChanged(block, "CBaseModelEntity", "m_clrRender");

            block.SetModel(blockModel);
            block.DispatchSpawn();
            block.AcceptInput("DisableMotion", block, block);
            block.Teleport(new Vector(blockPosition.X, blockPosition.Y, blockPosition.Z), new QAngle(blockRotation.X, blockRotation.Y, blockRotation.Z));

            CreateTrigger(block);

            UsedBlocks[block] = new BlockData(block, blockType, blockModel, blockSize, blockColor);
        }

        else instance.Logger.LogError("(CreateBlock) failed to create block");
    }

    public static Dictionary<CEntityInstance, CEntityInstance> BlockTriggers = new Dictionary<CEntityInstance, CEntityInstance>();
    public static void CreateTrigger(CBaseEntity block)
    {
        var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");

        if (trigger != null && trigger.IsValid)
        {
            trigger.Entity!.Name = "blockmaker_" + block.Entity!.Name + "_trigger";
            trigger.Spawnflags = 1;
            trigger.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);

            trigger.DispatchSpawn();
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
                CreateBlock(blockData.Name, blockData.Model, blockData.Size, new Vector(blockData.Position.X, blockData.Position.Y, blockData.Position.Z), new QAngle(blockData.Rotation.Pitch, blockData.Rotation.Yaw, blockData.Rotation.Roll), blockData.Color);
        }
        else
        {
            instance.PrintToChatAll($"{ChatColors.Red}Failed to spawn Blocks. File for {instance.GetMapName()} is empty or invalid");
            instance.Logger.LogError($"Failed to spawn Blocks. File for {instance.GetMapName()} is empty or invalid");
        }
    }
}
