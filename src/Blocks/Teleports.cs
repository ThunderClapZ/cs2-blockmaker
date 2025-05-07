using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

public partial class Blocks
{
    public static List<TeleportPair> Teleports = new List<TeleportPair>();
    private static Dictionary<CCSPlayerController, bool> isNextTeleport = new();
    public static void CreateTeleport(CCSPlayerController player)
    {
        var BuilderData = instance.BuilderData[player.Slot];
        var playerPawn = player.PlayerPawn.Value!;
        var position = new Vector(playerPawn.AbsOrigin!.X, playerPawn.AbsOrigin.Y, playerPawn.AbsOrigin.Z + playerPawn.Collision.Maxs.Z / 2);
        var rotation = playerPawn.AbsRotation!;

        if (!isNextTeleport.ContainsKey(player))
            isNextTeleport.Add(player, false);

        try
        {
            string teleportType = isNextTeleport[player] ? "Exit" : "Entry";
            var teleportData = CreateTeleportEntity(position, rotation, teleportType, BuilderData.BlockColor, BuilderData.BlockTransparency, BuilderData.BlockTeam);

            if (teleportData != null)
            {
                Utils.PrintToChat(player, $"Created teleport ({teleportType})");

                if (isNextTeleport[player])
                {
                    var incompletePair = Teleports.FirstOrDefault(p => p.Exit == null);

                    if (incompletePair != null)
                    {
                        incompletePair.Exit = teleportData;
                        Utils.PrintToChat(player, $"Paired teleports");
                    }
                    else
                    {
                        Teleports.Add(new TeleportPair(null!, teleportData));
                        Utils.PrintToChat(player, $"Pairing failed when creating a new exit teleport");
                    }
                }
                else Teleports.Add(new TeleportPair(teleportData, null!));

                isNextTeleport[player] = !isNextTeleport[player];
            }
            else Utils.PrintToChat(player, $"Failed to create {teleportType} teleport");

        }
        catch (Exception ex)
        {
            instance.Logger.LogError($"Exception: {ex}");
        }
    }

    public static TeleportData? CreateTeleportEntity(Vector position, QAngle rotation, string name, string color = "None", string transparency = "100%", string team = "Both", BlockData_Properties? properties = null)
    {
        var teleport = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");

        var config = instance.Config.Settings.Teleports;

        var entryModel = config.Entry.Model;
        var exitModel = config.Exit.Model;

        var entryColor = config.Entry.Color;
        var exitColor = config.Exit.Color;

        if (teleport != null && teleport.IsValid && teleport.Entity != null)
        {
            teleport.Entity.Name = "blockmaker_Teleport_" + name;
            teleport.EnableUseOutput = true;

            teleport.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            teleport.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;
            teleport.Render = Utils.ParseColor(name == "Entry" ? entryColor : exitColor);

            teleport.SetModel(name == "Entry" ? entryModel : exitModel);
            teleport.DispatchSpawn();

            teleport.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
            teleport.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;

            teleport.AcceptInput("DisableMotion", teleport, teleport);
            teleport.Teleport(position, rotation);

            CreateTeleportTrigger(teleport);

            var teleportData = new TeleportData(teleport, name);

            return teleportData;
        }
        else
        {
            Utils.Log("(CreateTeleport) Failed to create teleport");
            return null;
        }
    }

    private static void CreateTeleportTrigger(CBaseProp teleport)
    {
        var trigger = Utilities.CreateEntityByName<CTriggerMultiple>("trigger_multiple");

        if (trigger != null && trigger.IsValid && trigger.Entity != null)
        {
            trigger.Entity.Name = teleport.Entity!.Name + "_trigger";
            trigger.Spawnflags = 1;
            trigger.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            trigger.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            trigger.Collision.SolidFlags = 0;
            trigger.Collision.CollisionGroup = 14;

            trigger.SetModel(teleport.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
            trigger.DispatchSpawn();
            trigger.Teleport(teleport.AbsOrigin, teleport.AbsRotation);
            trigger.AcceptInput("FollowEntity", teleport, trigger, "!activator");
            trigger.AcceptInput("Enable");

            Triggers.Add(trigger, teleport);
        }

        else Utils.Log("(CreateTrigger) Failed to create trigger");
    }
}
