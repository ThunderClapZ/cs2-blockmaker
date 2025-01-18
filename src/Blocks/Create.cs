using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public partial class Blocks
{
    public static Dictionary<CBaseProp, BlockData> UsedBlocks = new Dictionary<CBaseProp, BlockData>();

    public static void CreateBlock(CCSPlayerController player)
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
            CreateEntity(selectedBlock, blockmodel, instance.playerData[player.Slot].BlockSize, RayTrace.Vector3toVector(hitPoint.Value), new QAngle());

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

    public static void CreateEntity(string blockType, string blockModel, string blockSize, Vector blockPosition, QAngle blockRotation)
    {
        var block = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override")!;

        if (block != null && block.IsValid)
        {
            block.DispatchSpawn();

            block.SetModel(blockModel);

            block.Entity!.Name = blockType;
            block.EnableUseOutput = true;
            block.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(block.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));

            block.AcceptInput("DisableMotion", block, block);
            block.Teleport(new Vector(blockPosition.X, blockPosition.Y, blockPosition.Z), new QAngle(blockRotation.X, blockRotation.Y, blockRotation.Z));

            block.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

            Plugin.StartTouch(block);

            UsedBlocks[block] = new BlockData(block, blockType, blockModel, blockSize);
        }

        else instance.Logger.LogError("(CreateBlock) failed to create block");
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
                instance.PrintToChatAll($"{ChatColors.Red}{noSpawnBlocksMessage()}");
                return;
            }

            foreach (var blockData in blockDataList)
                CreateEntity(blockData.Name, blockData.Model, blockData.Size, new Vector(blockData.Position.X, blockData.Position.Y, blockData.Position.Z), new QAngle(blockData.Rotation.Pitch, blockData.Rotation.Yaw, blockData.Rotation.Roll));
        }
        else
        {
            instance.PrintToChatAll($"{ChatColors.Red}{noSpawnBlocksMessage()}");
            instance.Logger.LogError(noSpawnBlocksMessage());
        }
    }

    private static string noSpawnBlocksMessage()
    {
        return $"Failed to spawn Blocks. File for {instance.GetMapName()} is empty or invalid";
    }
}
