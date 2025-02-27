using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

public partial class Blocks
{
    private static Plugin instance = Plugin.Instance;
    private static Config config = instance.Config;
    private static Config_Sounds.Sounds_Blocks sounds = instance.Config.Sounds.Blocks;
    private static BlockModels blockModels = Files.Models.Props;

    private static Dictionary<string, Action<CCSPlayerController, CBaseEntity>> blockActions = null!;

    public static void LoadTitles()
    {
        blockActions = new Dictionary<string, Action<CCSPlayerController, CBaseEntity>>
        {
            { blockModels.Random.Title, Action_Random },
            { blockModels.Bhop.Title, Action_Bhop },
            { blockModels.Gravity.Title, Action_Gravity },
            { blockModels.Health.Title, Action_Health },
            { blockModels.Grenade.Title, Action_Grenade },
            { blockModels.Frost.Title, Action_Frost },
            { blockModels.Flash.Title, Action_Flash },
            { blockModels.Fire.Title, Action_Fire },
            { blockModels.Delay.Title, Action_Delay },
            { blockModels.Death.Title, Action_Death },
            { blockModels.Damage.Title, Action_Damage },
            { blockModels.Speed.Title, Action_Speed },
            { blockModels.SpeedBoost.Title, Action_SpeedBoost },
            { blockModels.Slap.Title, Action_Slap },
            { blockModels.Nuke.Title, Action_Nuke },
            { blockModels.Stealth.Title, Action_Stealth },
            { blockModels.Invincibility.Title, Action_Invincibility },
            { blockModels.Camouflage.Title, Action_Camouflage },
            { blockModels.Trampoline.Title, Action_Trampoline },
            { blockModels.Honey.Title, Action_Honey },
        };
    }

    public static void Actions(CCSPlayerController player, CBaseEntity block)
    {
        if (block == null || block.Entity == null)
            return;

        string entityName = block.Entity.Name;

        if (string.IsNullOrEmpty(entityName) || !entityName.StartsWith("blockmaker"))
            return;

        if (!PlayerCooldowns.ContainsKey(player.Slot))
            PlayerCooldowns[player.Slot] = new();

        if (!CooldownsTimers.ContainsKey(player.Slot))
            CooldownsTimers[player.Slot] = new();

        if (BlockCooldown(player, block))
            return;

        foreach (var weaponType in WeaponList.Categories.Keys)
        {
            if (entityName.Contains(weaponType, StringComparison.OrdinalIgnoreCase))
            {
                Action_Weapons(player, block, entityName.Substring(entityName.LastIndexOf('.') + 1));
                return;
            }
        }

        if (blockActions.TryGetValue(Props[block].Type, out var action))
            action(player, block);

        else Utils.PrintToChat(player, $"{ChatColors.Red}Error: No action found for {entityName}");
    }

    private static BlockData_Properties Properties(CBaseEntity entity)
    {
        return Props.TryGetValue(entity, out var block)
        ? block.Properties
        : new BlockData_Properties();
    }

    private static void ActivatedMessage(CCSPlayerController player, string blocktitle)
    {
        Server.NextFrame(() => Utils.PrintToChat(player, $"{ChatColors.White}{blocktitle} {ChatColors.Grey}activated"));
    }
    private static void DeactivatedMessage(CCSPlayerController player, string blocktitle)
    {
        Server.NextFrame(() => Utils.PrintToChat(player, $"{ChatColors.White}{blocktitle} {ChatColors.Grey}has worn off"));
    }

    private static void Action_Random(CCSPlayerController player, CBaseEntity block)
    {
        var availableActions = blockActions.Where(kvp =>
            kvp.Key != blockModels.Random.Title &&
            kvp.Key != blockModels.Bhop.Title &&
            kvp.Key != blockModels.Delay.Title
        ).ToList();

        var randomAction = availableActions[new Random().Next(availableActions.Count)];

        Server.NextFrame(() =>
        {
            Utils.PrintToChat(player, $"You got {ChatColors.White}{randomAction.Key} {ChatColors.Grey}from the {ChatColors.White}{blockModels.Random.Title} {ChatColors.Grey}block");
            randomAction.Value(player, block);
        });

        BlockCooldownTimer(player, block, Properties(block).Cooldown);
    }

    public static List<CBaseEntity> TempTimers = new();
    private static void Action_Bhop(CCSPlayerController player, CBaseEntity block)
    {
        string model = block.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

        Vector pos = new(block.AbsOrigin!.X, block.AbsOrigin.Y, block.AbsOrigin.Z);
        QAngle rotation = new(block.AbsRotation!.X, block.AbsRotation.Y, block.AbsRotation.Z);
 
        var usedBlock = Props[block.As<CBaseProp>()];

        var duration = Properties(block).Duration;
        var cooldown = Properties(block).Cooldown;

        if (TempTimers.Contains(block))
            return;

        TempTimers.Add(block);

        instance.AddTimer(duration, () =>
        {
            if (block.IsValid && Props.ContainsKey(block.As<CBaseProp>()))
            {
                var tempblock = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override")!;

                tempblock.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
                tempblock.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

                Color clr = Utils.GetColor(usedBlock.Color);
                tempblock.Render = Color.FromArgb(75, clr.R, clr.G, clr.B);
                Utilities.SetStateChanged(tempblock, "CBaseModelEntity", "m_clrRender");

                tempblock.SetModel(model);
                tempblock.DispatchSpawn();

                tempblock.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                tempblock.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                Utilities.SetStateChanged(tempblock, "CCollisionProperty", "m_CollisionGroup");
                Utilities.SetStateChanged(tempblock, "VPhysicsCollisionAttribute_t", "m_nCollisionGroup");

                tempblock.AcceptInput("DisableMotion", tempblock, tempblock);
                tempblock.AcceptInput("SetScale", tempblock, tempblock, Utils.GetSize(usedBlock.Size).ToString());
                tempblock.Teleport(pos, rotation);

                block.Remove();
                Props.Remove(block.As<CBaseProp>());

                instance.AddTimer(cooldown, () =>
                {
                    tempblock.Remove();

                    CreateBlock(
                        usedBlock.Type,
                        usedBlock.Pole,
                        usedBlock.Size,
                        pos,
                        rotation,
                        usedBlock.Color,
                        usedBlock.Transparency,
                        usedBlock.Team,
                        usedBlock.Properties
                    );

                    if (TempTimers.Contains(block))
                        TempTimers.Remove(block);
                });
            }
        });
    }

    private static void Action_Delay(CCSPlayerController player, CBaseEntity block)
    {
        string model = block.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

        Vector pos = new(block.AbsOrigin!.X, block.AbsOrigin.Y, block.AbsOrigin.Z);
        QAngle rotation = new(block.AbsRotation!.X, block.AbsRotation.Y, block.AbsRotation.Z);

        var usedBlock = Props[block.As<CBaseProp>()];

        var duration = Properties(block).Duration;
        var cooldown = Properties(block).Cooldown;

        if (TempTimers.Contains(block))
            return;

        TempTimers.Add(block);

        instance.AddTimer(duration, () =>
        {
            if (block.IsValid && Props.ContainsKey(block.As<CBaseProp>()))
            {
                var tempblock = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override")!;

                tempblock.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
                tempblock.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

                Color clr = Utils.GetColor(usedBlock.Color);
                tempblock.Render = Color.FromArgb(75, clr.R, clr.G, clr.B);
                Utilities.SetStateChanged(tempblock, "CBaseModelEntity", "m_clrRender");

                tempblock.SetModel(model);
                tempblock.DispatchSpawn();

                tempblock.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                tempblock.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                Utilities.SetStateChanged(tempblock, "CCollisionProperty", "m_CollisionGroup");
                Utilities.SetStateChanged(tempblock, "VPhysicsCollisionAttribute_t", "m_nCollisionGroup");

                tempblock.AcceptInput("DisableMotion", tempblock, tempblock);
                tempblock.AcceptInput("SetScale", tempblock, tempblock, Utils.GetSize(usedBlock.Size).ToString());
                tempblock.Teleport(pos, rotation);

                block.Remove();
                Props.Remove(block.As<CBaseProp>());

                instance.AddTimer(cooldown, () =>
                {
                    tempblock.Remove();

                    CreateBlock(
                        usedBlock.Type,
                        usedBlock.Pole,
                        usedBlock.Size,
                        pos,
                        rotation,
                        usedBlock.Color,
                        usedBlock.Transparency,
                        usedBlock.Team,
                        usedBlock.Properties
                    );

                    if (TempTimers.Contains(block))
                        TempTimers.Remove(block);
                });
            }
        });
    }

    private static void Action_Gravity(CCSPlayerController player, CBaseEntity block)
    {
        var title = blockModels.Gravity.Title;
        var settings = Properties(block);
        var gravity = player.GravityScale;

        player.SetGravity(settings.Value);

        ActivatedMessage(player, title);

        instance.AddTimer(settings.Duration, () =>
        {
            player.SetGravity(gravity);

            DeactivatedMessage(player, title);
        });

        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_Health(CCSPlayerController player, CBaseEntity block)
    {
        var pawn = player.Pawn();
        if (pawn == null) return;

        if (player.Health >= player.MaxHealth)
            return;

        var settings = Properties(block);

        player.Health((int)+settings.Value);

        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_Grenade(CCSPlayerController player, CBaseEntity block)
    {
        player.GiveWeapon("weapon_hegrenade");

        BlockCooldownTimer(player, block, Properties(block).Cooldown);
    }

    private static void Action_Frost(CCSPlayerController player, CBaseEntity block)
    {
        player.GiveWeapon("weapon_smokegrenade");

        BlockCooldownTimer(player, block, Properties(block).Cooldown);
    }

    private static void Action_Flash(CCSPlayerController player, CBaseEntity block)
    {
        player.GiveWeapon("weapon_flashbang");

        BlockCooldownTimer(player, block, Properties(block).Cooldown);
    }

    private static void Action_Fire(CCSPlayerController player, CBaseEntity block)
    {
        var settings = Properties(block);

        var fire = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;

        fire.EffectName = "particles/burning_fx/env_fire_medium.vpcf";
        fire.DispatchSpawn();
        fire.AcceptInput("Start");
        fire.AcceptInput("FollowEntity", player.Pawn(), player.Pawn(), "!activator");

        player.Health((int)-settings.Value);
        Server.NextFrame(() => player.PlaySound(sounds.Damage.Event, sounds.Damage.Volume));
        var firetimer = instance.AddTimer(1.0f, () =>
        {
            player.Health((int)-settings.Value);
        }, TimerFlags.REPEAT);

        instance.AddTimer(settings.Duration, () =>
        {
            if (firetimer != null)
                firetimer.Kill();

            if (fire.IsValid)
            {
                fire.AcceptInput("Stop");
                fire.Remove();
            }
        });

        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_Death(CCSPlayerController player, CBaseEntity block)
    {
        var pawn = player.Pawn();
        if (pawn == null) return;

        BlockCooldownTimer(player, block, 1.0f);

        if (pawn.TakesDamage == false)
            return;

        player.CommitSuicide(false, true);
    }

    private static void Action_Damage(CCSPlayerController player, CBaseEntity block)
    {
        var settings = Properties(block);

        player.Health((int)-settings.Value);
  
        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_Weapons(CCSPlayerController player, CBaseEntity block, string weapon)
    {
        var gun = WeaponList.Weapons.FirstOrDefault(w => w.Name.Equals(weapon, StringComparison.OrdinalIgnoreCase));

        if (gun != null)
        {
            var weaponCategory = WeaponList.Categories.FirstOrDefault(category =>
                category.Value.Contains(gun.Designer)).Key;

            if (!string.IsNullOrEmpty(weaponCategory))
            {
                int weaponGroup = weaponCategory == Files.Models.Props.Pistol.Title ? 2 : 1;

                var hasGroupWeapon = player.PlayerPawn.Value?.WeaponServices?.MyWeapons
                    .Any(w => WeaponList.Categories
                    .Where(cat => (cat.Key == Files.Models.Props.Pistol.Title ? 2 : 1) == weaponGroup)
                    .SelectMany(cat => cat.Value)
                    .Contains(w.Value?.DesignerName)) ?? false;

                if (hasGroupWeapon)
                    return;
            }

            var designer = gun.Designer;

            player.GiveWeapon(designer);

            if (WeaponList.SpecialWeapons.TryGetValue(designer, out var special))
                designer = special;

            player.FindWeapon(designer).SetAmmo(1, 0);

            Utils.PrintToChatAll($"{ChatColors.LightPurple}{player.PlayerName} {ChatColors.Grey}equipped a {ChatColors.White}{weapon}");

            BlockCooldownTimer(player, block, 999);
        }

        else Utils.PrintToChatAll($"{ChatColors.Red}The weapon '{weapon}' does not exist in the weapon list.");
    }

    private static void Action_Speed(CCSPlayerController player, CBaseEntity block)
    {
        var velocity = player.PlayerPawn.Value!.VelocityModifier;
        var title = blockModels.Speed.Title;
        var settings = Properties(block);
        var sound = sounds.Speed;

        player.SetVelocity(settings.Value);
        player.PlaySound(sound.Event, sound.Volume);
        ActivatedMessage(player, title);

        instance.AddTimer(settings.Duration, () =>
        {
            player.SetVelocity(velocity);
            DeactivatedMessage(player, title);
        });

        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_SpeedBoost(CCSPlayerController player, CBaseEntity block)
    {
        CCSPlayerPawn pawn = player.Pawn()!;
        QAngle viewAngles = pawn.EyeAngles;

        float angleYaw = viewAngles.Y * (float)Math.PI / 180f;
        float boost = Properties(block).Value;

        Vector vel = new(pawn.AbsVelocity.X, pawn.AbsVelocity.Y, pawn.AbsVelocity.Z);

        vel.X = (float)Math.Cos(angleYaw) * boost;
        vel.Y = (float)Math.Sin(angleYaw) * boost;
        vel.Z = 300;

        pawn.Teleport(null, null, vel);

        BlockCooldownTimer(player, block, 0.25f);
    }


    private static void Action_Slap(CCSPlayerController player, CBaseEntity block)
    {
        player.Slap((int)Properties(block).Value);

        BlockCooldownTimer(player, block, 0.25f);
    }

    public static bool nuked;
    private static void Action_Nuke(CCSPlayerController player, CBaseEntity block)
    {
        if (nuked)
            return;

        CsTeam teamToNuke = 0;
        string teamName = "";

        if (player.IsT())
        {
            teamToNuke = CsTeam.CounterTerrorist;
            teamName = "Counter-Terrorist";
        }
        else if (player.IsCT())
        {
            teamToNuke = CsTeam.Terrorist;
            teamName = "Terrorist";
        }

        var playersToNuke = Utilities.GetPlayers().Where(p => p.Team == teamToNuke);

        foreach (var playerToNuke in playersToNuke)
            playerToNuke.CommitSuicide(false, true);

        Utils.PrintToChatAll($"{ChatColors.LightPurple}{player.PlayerName} {ChatColors.Grey}has nuked the {teamName} team");

        Utils.PlaySoundAll(sounds.Nuke.Event, sounds.Nuke.Volume);

        nuked = true;
    }

    private static void Action_Stealth(CCSPlayerController player, CBaseEntity block)
    {
        var title = blockModels.Stealth.Title;
        var settings = Properties(block);

        player.SetInvis(true);
        player.PlaySound(sounds.Stealth.Event, sounds.Stealth.Volume);
        player.ColorScreen(Color.FromArgb(150, 75, 75, 75), settings.Duration / 2, settings.Duration, EntityExtends.FadeFlags.FADE_OUT);
        ActivatedMessage(player, title);

        instance.AddTimer(settings.Duration, () =>
        {
            player.SetInvis(false);
            DeactivatedMessage(player, title);
        });

        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_Invincibility(CCSPlayerController player, CBaseEntity block)
    {
        var pawn = player.Pawn();
        if (pawn == null) return;

        var title = blockModels.Invincibility.Title;
        var settings = Properties(block);
        var takesdamage = pawn.TakesDamage;
        var sound = sounds.Invincibility;

        takesdamage = false;
        player.PlaySound(sound.Event, sound.Volume);
        player.ColorScreen(Color.FromArgb(50, 255, 0, 255), settings.Duration / 2, settings.Duration, EntityExtends.FadeFlags.FADE_OUT);
        ActivatedMessage(player, title);

        instance.AddTimer(settings.Duration, () =>
        {
            takesdamage = true;
            DeactivatedMessage(player, title);
        });

        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_Camouflage(CCSPlayerController player, CBaseEntity block)
    {
        var pawn = player.Pawn();
        if (pawn == null) return;

        var title = blockModels.Camouflage.Title;
        var settings = Properties(block);
        var sound = sounds.Camouflage;
        var model = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName!;

        if (player.IsT()) player.SetModel(config.Settings.Blocks.CamouflageT);
        else if (player.IsCT()) player.SetModel(config.Settings.Blocks.CamouflageCT);

        player.PlaySound(sounds.Camouflage.Event, sounds.Camouflage.Volume);
        player.ColorScreen(Color.FromArgb(50, 0, 255, 0), settings.Duration / 2, settings.Duration, EntityExtends.FadeFlags.FADE_OUT);
        ActivatedMessage(player, title);

        instance.AddTimer(settings.Duration, () =>
        {
            player.SetModel(model);
            DeactivatedMessage(player, title);
        });

        BlockCooldownTimer(player, block, settings.Cooldown);
    }

    private static void Action_Trampoline(CCSPlayerController player, CBaseEntity block)
    {
        CCSPlayerPawn pawn = player.Pawn()!;

        Vector vel = new(pawn.AbsVelocity.X, pawn.AbsVelocity.Y, pawn.AbsVelocity.Z);

        vel.Z = Properties(block).Value;

        pawn.Teleport(null, null, vel);

        BlockCooldownTimer(player, block, 0.25f);
    }

    private static void Action_Honey(CCSPlayerController player, CBaseEntity block)
    {
        player.SetVelocity(Properties(block).Value);

        BlockCooldownTimer(player, block, 0.1f);
    }

    public static void Test(CCSPlayerController player)
    {
        var block = player.GetBlockAimTarget();

        if (block == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to test");
            return;
        }

        if (block.Entity == null || !block.Entity.Name.StartsWith("blockmaker"))
            return;

        Utils.PrintToChat(player, $"Testing - {ChatColors.White}{block.Entity!.Name.Replace("blockmaker_", "")}");
        Actions(player, block);
    }
}