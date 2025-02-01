using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Text.Json;

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
    private static void CreateBlock(string type, string model, string size, Vector position, QAngle rotation, string color = "None", string transparency = "100%", string team = "Both", BlockData_Properties? properties = null)
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

            if (properties == null || (properties.Duration == 0 && properties.Cooldown == 0 && properties.Value == 0))
            {
                string match = type.Split('.')[0];

                if (BlockDefaultProperties.ContainsKey(match))
                {
                    properties = new BlockData_Properties
                    {
                        Cooldown = BlockDefaultProperties[match].Cooldown,
                        Duration = BlockDefaultProperties[match].Duration,
                        Value = BlockDefaultProperties[match].Value
                    };
                }
                else properties = new BlockData_Properties();
            }

            Props[block] = new BlockData(block, type, model, size, color, transparency, team, properties);
        }
    }

    public static Dictionary<CEntityInstance, CEntityInstance> Triggers = new Dictionary<CEntityInstance, CEntityInstance>();
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

    public static void Spawn()
    {
        var blocksPath = Path.Combine(Files.mapsFolder, "blocks.json");
        var teleportsPath = Path.Combine(Files.mapsFolder, "teleports.json");

        // spawn blocks
        if (File.Exists(blocksPath) && Utils.IsValidJson(blocksPath))
        {
            var blocksJson = File.ReadAllText(blocksPath);
            var blocksList = JsonSerializer.Deserialize<List<SaveBlockData>>(blocksJson);

            if (blocksList != null && blocksList.Count > 0)
            {
                foreach (var blockData in blocksList)
                {
                    var position = new Vector(blockData.Position.X, blockData.Position.Y, blockData.Position.Z);
                    var rotation = new QAngle(blockData.Rotation.Pitch, blockData.Rotation.Yaw, blockData.Rotation.Roll);

                    CreateBlock(
                        blockData.Name,
                        blockData.Model,
                        blockData.Size,
                        position,
                        rotation,
                        blockData.Color,
                        blockData.Transparency,
                        blockData.Team,
                        blockData.Properties
                    );
                }
            }
        }
        else Utils.Log($"Failed to spawn Blocks. File for {Utils.GetMapName()} is empty or invalid");

        // spawn teleports
        if (File.Exists(teleportsPath) && Utils.IsValidJson(teleportsPath))
        {
            var teleportsJson = File.ReadAllText(teleportsPath);
            var teleportsList = JsonSerializer.Deserialize<List<TeleportPairDTO>>(teleportsJson);

            if (teleportsList != null && teleportsList.Count > 0)
            {
                foreach (var teleportPairData in teleportsList)
                {
                    var entryPosition = new Vector(teleportPairData.Entry.Position.X, teleportPairData.Entry.Position.Y, teleportPairData.Entry.Position.Z);
                    var entryRotation = new QAngle(teleportPairData.Entry.Rotation.Pitch, teleportPairData.Entry.Rotation.Yaw, teleportPairData.Entry.Rotation.Roll);

                    var entryEntity = CreateTeleportEntity(entryPosition, entryRotation, teleportPairData.Entry.Name);

                    var exitPosition = new Vector(teleportPairData.Exit.Position.X, teleportPairData.Exit.Position.Y, teleportPairData.Exit.Position.Z);
                    var exitRotation = new QAngle(teleportPairData.Exit.Rotation.Pitch, teleportPairData.Exit.Rotation.Yaw, teleportPairData.Exit.Rotation.Roll);

                    var exitEntity = CreateTeleportEntity(exitPosition, exitRotation, teleportPairData.Exit.Name);

                    if (entryEntity != null && exitEntity != null)
                        Teleports.Add(new TeleportPair(entryEntity, exitEntity));
                }
            }
        }
    }
}
