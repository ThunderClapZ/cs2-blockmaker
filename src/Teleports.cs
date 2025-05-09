using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

public static class Teleports
{
    public class Data
    {
        public Data
        (
            CBaseProp teleport,
            string name
        )
        {
            Entity = teleport;
            Name = name;
        }

        public CBaseProp Entity;
        public string Name { get; set; }
    }

    public class SaveData
    {
        public string Name { get; set; } = "";
        public VectorUtils.VectorDTO Position { get; set; } = new VectorUtils.VectorDTO(Vector.Zero);
        public VectorUtils.QAngleDTO Rotation { get; set; } = new VectorUtils.QAngleDTO(QAngle.Zero);
    }

    public class Pair
    {
        public Data Entry { get; set; }
        public Data Exit { get; set; }

        public Pair(Data entry, Data exit)
        {
            Entry = entry;
            Exit = exit;
        }
    }

    public class PairSaveData
    {
        public SaveData Entry { get; set; } = new SaveData();
        public SaveData Exit { get; set; } = new SaveData();
    }

    private static Plugin Instance = Plugin.Instance;
    private static Config Config = Instance.Config;

    public static List<Pair> Entities = new List<Pair>();
    public static Dictionary<CCSPlayerController, bool> isNext = new();

    public static void Create(CCSPlayerController player)
    {
        var BuilderData = Instance.BuilderData[player.Slot];
        var playerPawn = player.PlayerPawn.Value!;
        var position = new Vector(playerPawn.AbsOrigin!.X, playerPawn.AbsOrigin.Y, playerPawn.AbsOrigin.Z + playerPawn.Collision.Maxs.Z / 2);
        var rotation = playerPawn.AbsRotation!;

        if (!isNext.ContainsKey(player))
            isNext.Add(player, false);

        try
        {
            string Type = isNext[player] ? "Exit" : "Entry";
            var teleportData = CreateEntity(position, rotation, Type);

            if (teleportData != null)
            {
                Utils.PrintToChat(player, $"Created teleport ({Type})");

                if (isNext[player])
                {
                    var incompletePair = Entities.FirstOrDefault(p => p.Exit == null);

                    if (incompletePair != null)
                    {
                        incompletePair.Exit = teleportData;
                        Utils.PrintToChat(player, $"Paired teleports");
                    }
                    else
                    {
                        Entities.Add(new Pair(null!, teleportData));
                        Utils.PrintToChat(player, $"Pairing failed when creating a new exit teleport");
                    }
                }
                else Entities.Add(new Pair(teleportData, null!));

                isNext[player] = !isNext[player];
            }
            else Utils.PrintToChat(player, $"Failed to create {Type} teleport");

        }
        catch (Exception ex)
        {
            Instance.Logger.LogError($"Exception: {ex}");
        }
    }

    public static Data? CreateEntity(Vector position, QAngle rotation, string name)
    {
        var teleport = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");

        var config = Config.Settings.Teleports;

        var entryModel = config.Entry.Model;
        var exitModel = config.Exit.Model;

        var entryColor = config.Entry.Color;
        var exitColor = config.Exit.Color;

        if (teleport != null && teleport.IsValid && teleport.Entity != null)
        {
            teleport.Entity.Name = "blockmaker_Teleport_" + name;
            teleport.EnableUseOutput = true;

            teleport.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            teleport.ShadowStrength = Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;
            teleport.Render = Utils.ParseColor(name == "Entry" ? entryColor : exitColor);

            teleport.SetModel(name == "Entry" ? entryModel : exitModel);
            teleport.DispatchSpawn();

            teleport.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
            teleport.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;

            teleport.AcceptInput("DisableMotion", teleport, teleport);
            teleport.Teleport(position, rotation);

            CreateTrigger(teleport);

            var teleportData = new Data(teleport, name);

            return teleportData;
        }
        else
        {
            Utils.Log("(CreateTeleport) Failed to create teleport");
            return null;
        }
    }

    private static void CreateTrigger(CBaseProp teleport)
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

            Blocks.Triggers.Add(trigger, teleport);
        }

        else Utils.Log("(CreateTrigger) Failed to create trigger");
    }

    public static void Delete(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();
        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a teleport to delete");
            return;
        }

        var teleports = Entities.First(pair => pair.Entry.Entity == entity || pair.Exit.Entity == entity);

        if (teleports != null)
        {
            if (teleports.Entry == null || teleports.Exit == null)
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Could not delete unfinished teleport pair");
                return;
            }

            var entryEntity = teleports.Entry.Entity;
            if (entryEntity != null && entryEntity.IsValid)
            {
                entryEntity.Remove();

                var entryTrigger = Blocks.Triggers.Where(kvp => kvp.Value == entryEntity).First().Key;
                if (entryTrigger != null)
                {
                    entryTrigger.Remove();
                    Blocks.Triggers.Remove(entryTrigger);
                }
            }

            var exitEntity = teleports.Exit.Entity;
            if (exitEntity != null && exitEntity.IsValid)
            {
                exitEntity.Remove();

                var exitTrigger = Blocks.Triggers.Where(kvp => kvp.Value == exitEntity).First().Key;
                if (exitTrigger != null)
                {
                    exitTrigger.Remove();
                    Blocks.Triggers.Remove(exitTrigger);
                }
            }

            Entities.Remove(teleports);

            if (Instance.Config.Sounds.Building.Enabled)
                player.EmitSound(Instance.Config.Sounds.Building.Delete);

            Utils.PrintToChat(player, $"Deleted teleport pair");
        }
        else Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a teleport to delete");
    }
}
