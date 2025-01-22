using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

public partial class Blocks
{
    private static Plugin instance = Plugin.Instance;
    private static Dictionary<string, Action<CCSPlayerController, CBaseEntity>> blockActions = null!;
    private static Settings.Settings_Blocks settings = instance.Config.Settings.Blocks;
    private static Sounds.Sounds_Blocks sounds = instance.Config.Sounds.Blocks;

    public static void Load()
    {
        var block = Plugin.BlockModels;
        blockActions = new Dictionary<string, Action<CCSPlayerController, CBaseEntity>>
        {
            { block.Random.Title, Action_Random },
            { block.Bhop.Title, Action_Bhop },
            { block.Gravity.Title, Action_Gravity },
            { block.Health.Title, Action_Health },
            { block.Grenade.Title, Action_Grenade },
            { block.Frost.Title, Action_Frost },
            { block.Flash.Title, Action_Flash },
            { block.Fire.Title, Action_Fire },
            { block.Delay.Title, Action_Delay },
            { block.Death.Title, Action_Death },
            { block.Damage.Title, Action_Damage },
            { block.Deagle.Title, Action_Deagle },
            { block.AWP.Title, Action_AWP },
            { block.Speed.Title, Action_Speed },
            { block.Slap.Title, Action_Slap },
            { block.Nuke.Title, Action_Nuke },
            { block.Stealth.Title, Action_Stealth },
            { block.Invincibility.Title, Action_Invincibility },
            { block.Camouflage.Title, Action_Camouflage },
            /*
            { block.Trampoline.Title, Action_Trampoline },
            { block.NoFallDmg.Title, Action_NoFallDmg },
            { block.SpeedBoost.Title, Action_SpeedBoost },
            { block.Platform.Title, Action_Platform },
            { block.Honey.Title, Action_Honey },
            { block.Glass.Title, Action_Glass },
            { block.TBarrier.Title, Action_TBarrier },
            { block.CTBarrier.Title, Action_CTBarrier },
            { block.Ice.Title, Action_Ice },
            { block.NoSlowDown.Title, Action_NoSlowDown }
            */
        };
    }

    public static Dictionary<int, BlocksCooldown> blocksCooldown = new Dictionary<int, BlocksCooldown>();
    public static Dictionary<CCSPlayerController, List<Timer>> Timers = new();
    public static void BlockCooldownTimer(CCSPlayerController player, string block, float timer, bool message = true)
    {
        if (timer <= 0)
            return;

        var cdtimer = instance.AddTimer(timer, () =>
        {
            if (!player.IsAlive()) return;

            var cooldownProperty = blocksCooldown[player.Slot].GetType().GetField(block);

            if (cooldownProperty != null && cooldownProperty.FieldType == typeof(bool))
            {
                bool cooldown = (bool)cooldownProperty.GetValue(blocksCooldown[player.Slot])!;

                if (cooldown)
                {
                    cooldownProperty.SetValue(blocksCooldown[player.Slot], false);

                    if (message)
                        instance.PrintToChat(player, $"{ChatColors.White}{block} {ChatColors.Grey}block is no longer on cooldown");
                }
            }

            else instance.PrintToChat(player, $"{ChatColors.Red}Error: could not reset cooldown for {block} block");
        });

        Timers[player].Add(cdtimer);
    }

    public static void Actions(CCSPlayerController player, CBaseEntity block)
    {
        if (!blocksCooldown.ContainsKey(player.Slot))
            blocksCooldown[player.Slot] = new BlocksCooldown();

        if (!Timers.ContainsKey(player))
            Timers[player] = new List<Timer>();

        if (block == null || block.Entity == null)
            return;

        string entityName = block.Entity.Name;

        if (string.IsNullOrEmpty(entityName))
            return;

        string[] blockName = entityName.Split('_');

        if (blockActions.TryGetValue(blockName[1], out var action))
            action(player, block);

        else instance.PrintToChat(player, $"{ChatColors.Red}Error: No action found for {blockName[1]} block");
    }

    private static void Action_Random(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Random)
            return;

        var availableActions = blockActions.Where(kvp => kvp.Key != Plugin.BlockModels.Random.Title).ToList();

        var randomAction = availableActions[new Random().Next(availableActions.Count)];

        randomAction.Value(player, block);

        instance.PrintToChat(player, $"You got {ChatColors.White}{randomAction.Key} {ChatColors.Grey}from the {ChatColors.White}{block.Entity!.Name} {ChatColors.Grey}block");

        blocksCooldown[player.Slot].Random = true;

        BlockCooldownTimer(player, "Random", settings.Random.Cooldown);
    }

    private static void Action_Bhop(CCSPlayerController player, CBaseEntity block)
    {
        string model = block.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
        Vector pos = new Vector(block.AbsOrigin!.X, block.AbsOrigin.Y, block.AbsOrigin.Z);
        QAngle rotation = new QAngle(block.AbsRotation!.X, block.AbsRotation.Y, block.AbsRotation.Z);
        string size = UsedBlocks[block.As<CBaseProp>()].Size;
        string color = UsedBlocks[block.As<CBaseProp>()].Color;

        instance.AddTimer(settings.Bhop.Duration, () =>
        {
            if (block.IsValid && UsedBlocks.ContainsKey(block.As<CBaseProp>()))
            {
                var tempblock = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override")!;

                tempblock.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
                tempblock.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

                Color clr = Plugin.GetColor(color);
                tempblock.Render = Color.FromArgb(75, clr.R, clr.G, clr.B);
                Utilities.SetStateChanged(tempblock, "CBaseModelEntity", "m_clrRender");

                tempblock.SetModel(model);
                tempblock.DispatchSpawn();

                tempblock.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                tempblock.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                Utilities.SetStateChanged(tempblock, "CCollisionProperty", "m_CollisionGroup");
                Utilities.SetStateChanged(tempblock, "VPhysicsCollisionAttribute_t", "m_nCollisionGroup");

                tempblock.AcceptInput("DisableMotion", tempblock, tempblock);
                tempblock.Teleport(pos, rotation);

                block.Remove();
                UsedBlocks.Remove(block.As<CBaseProp>());

                instance.AddTimer(settings.Bhop.Cooldown, () =>
                {
                    tempblock.Remove();
                    CreateBlock("Bhop", model, size, pos, rotation, color);
                });
            }
        });
    }

    private static void Action_Gravity(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Gravity)
            return;

        var gravity = player.GravityScale;

        player.SetGravity(settings.Gravity.Value);

        instance.AddTimer(settings.Gravity.Duration, () =>
        {
            player.SetGravity(gravity);
            instance.PrintToChat(player, $"{block.Entity!.Name} has worn off");
        });

        blocksCooldown[player.Slot].Gravity = true;
        BlockCooldownTimer(player, "Gravity", settings.Gravity.Cooldown);
    }

    private static void Action_Health(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Health)
            return;

        if (player.Pawn()!.Health >= player.Pawn()!.MaxHealth)
            return;

        player.Health(+2);
        player.PlaySound(sounds.Health);

        blocksCooldown[player.Slot].Health = true;
        BlockCooldownTimer(player, "Health", settings.Health.Cooldown, false);
    }

    private static void Action_Grenade(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Grenade)
            return;

        player.GiveWeapon("weapon_hegrenade");

        blocksCooldown[player.Slot].Grenade = true;
        BlockCooldownTimer(player, "Grenade", settings.Grenade.Cooldown);
    }

    private static void Action_Frost(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Frost)
            return;

        player.GiveWeapon("weapon_smokegrenade");

        blocksCooldown[player.Slot].Frost = true;
        BlockCooldownTimer(player, "Frost", settings.Frost.Cooldown);
    }

    private static void Action_Flash(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Flash)
            return;

        player.GiveWeapon("weapon_flashbang");

        blocksCooldown[player.Slot].Flash = true;
        BlockCooldownTimer(player, "Flash", settings.Flash.Cooldown);
    }

    private static void Action_Fire(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Fire)
            return;

        var fire = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;

        fire.EffectName = "particles/burning_fx/env_fire_medium.vpcf";

        fire.DispatchSpawn();
        fire.AcceptInput("Start");
        fire.AcceptInput("FollowEntity", player.Pawn(), player.Pawn(), "!activator");

        player.Health((int)- settings.Fire.Value);

        var firetimer = instance.AddTimer(1.0f, () =>
        {
            player.Health((int)- settings.Fire.Value);
        }, TimerFlags.REPEAT);

        instance.AddTimer(settings.Fire.Duration, () =>
        {
            if (firetimer != null)
                firetimer.Kill();

            if (fire.IsValid)
            {
                fire.AcceptInput("Stop");
                fire.Remove();
            }
        });

        blocksCooldown[player.Slot].Fire = true;
        BlockCooldownTimer(player, "Fire", settings.Fire.Cooldown, false);
    }

    private static void Action_Delay(CCSPlayerController player, CBaseEntity block)
    {
        string model = block.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
        Vector pos = new Vector(block.AbsOrigin!.X, block.AbsOrigin.Y, block.AbsOrigin.Z);
        QAngle rotation = new QAngle(block.AbsRotation!.X, block.AbsRotation.Y, block.AbsRotation.Z);
        string size = UsedBlocks[block.As<CBaseProp>()].Size;
        string color = UsedBlocks[block.As<CBaseProp>()].Color;

        instance.AddTimer(settings.Delay.Duration, () =>
        {
            if (block.IsValid && UsedBlocks.ContainsKey(block.As<CBaseProp>()))
            {
                var tempblock = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override")!;

                tempblock.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
                tempblock.ShadowStrength = instance.Config.Settings.Blocks.DisableShadows ? 0.0f : 1.0f;

                Color clr = Plugin.GetColor(color);
                tempblock.Render = Color.FromArgb(75, clr.R, clr.G, clr.B);
                Utilities.SetStateChanged(tempblock, "CBaseModelEntity", "m_clrRender");

                tempblock.SetModel(model);
                tempblock.DispatchSpawn();

                tempblock.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                tempblock.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DISSOLVING;
                Utilities.SetStateChanged(tempblock, "CCollisionProperty", "m_CollisionGroup");
                Utilities.SetStateChanged(tempblock, "VPhysicsCollisionAttribute_t", "m_nCollisionGroup");

                tempblock.AcceptInput("DisableMotion", tempblock, tempblock);
                tempblock.Teleport(pos, rotation);

                block.Remove();
                UsedBlocks.Remove(block.As<CBaseProp>());

                instance.AddTimer(settings.Delay.Cooldown, () =>
                {
                    tempblock.Remove();
                    CreateBlock("Delay", model, size, pos, rotation, color);
                });
            }
        });
    }

    private static void Action_Death(CCSPlayerController player, CBaseEntity block)
    {
        if (player.Pawn()!.TakesDamage == false)
        {
            instance.PrintToChat(player, "looks like you avoided death");
            return;
        }

        player.CommitSuicide(false, true);
    }

    private static void Action_Damage(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Damage)
            return;

        var playerPawn = player.Pawn();

        if (playerPawn == null)
            return;

        player.Health((int)- settings.Damage.Value);
        player.PlaySound(sounds.Damage);

        blocksCooldown[player.Slot].Damage = true;
        BlockCooldownTimer(player, "Damage", settings.Damage.Cooldown, false);
    }

    private static void Action_Deagle(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Deagle)
            return;

        player.GiveWeapon("weapon_deagle");
        player.FindWeapon("weapon_deagle").SetAmmo(1, 0);

        instance.PrintToChatAll($"{ChatColors.LightPurple}{player.PlayerName} {ChatColors.Grey}equipped a Deagle");

        blocksCooldown[player.Slot].Deagle = true;
    }

    private static void Action_AWP(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].AWP)
            return;

        player.GiveWeapon("weapon_awp");
        player.FindWeapon("weapon_awp").SetAmmo(1, 0);

        instance.PrintToChatAll($"{ChatColors.LightPurple}{player.PlayerName} {ChatColors.Grey}equipped an AWP");

        blocksCooldown[player.Slot].AWP = true;
    }

    private static void Action_Speed(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Speed)
            return;

        var speed = player.Speed;

        player.Speed = settings.Speed.Value;
        player.PlaySound(sounds.Speed);

        instance.AddTimer(settings.Speed.Duration, () =>
        {
            player.Speed = speed;
            instance.PrintToChat(player, $"{block.Entity!.Name} has worn off");
        });

        blocksCooldown[player.Slot].Speed = true;
        BlockCooldownTimer(player, "Speed", settings.Speed.Cooldown);
    }

    private static void Action_Slap(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Slap)
            return;

        player.Slap((int)settings.Slap.Value);

        blocksCooldown[player.Slot].Slap = true;
        BlockCooldownTimer(player, "Slap", 0.25f, false);
    }

    private static void Action_Nuke(CCSPlayerController player, CBaseEntity block)
    {
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

        instance.PrintToChatAll($"{ChatColors.LightPurple}{player.PlayerName} {ChatColors.Grey}has nuked the {teamName} team");

        instance.PlaySoundAll(sounds.Nuke);
    }

    private static void Action_Stealth(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Stealth)
            return;

        player.SetInvis(true);
        player.PlaySound(sounds.Stealth);
        player.ColorScreen(Color.FromArgb(150, 100, 100, 100), 2.5f, 5.0f, EntityExtends.FadeFlags.FADE_OUT);

        instance.AddTimer(settings.Stealth.Duration, () =>
        {
            player.SetInvis(false);
            instance.PrintToChat(player, $"{block.Entity!.Name} has worn off");
        });

        blocksCooldown[player.Slot].Stealth = true;
        BlockCooldownTimer(player, "Stealth", settings.Stealth.Cooldown);
    }

    private static void Action_Invincibility(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Invincibility)
            return;

        player.Pawn()!.TakesDamage = false;
        player.PlaySound(sounds.Invincibility);
        player.ColorScreen(Color.FromArgb(100, 100, 0, 100), 2.5f, 5.0f, EntityExtends.FadeFlags.FADE_OUT);

        instance.AddTimer(settings.Invincibility.Duration, () =>
        {
            player.Pawn()!.TakesDamage = true;
            instance.PrintToChat(player, $"{block.Entity!.Name} has worn off");
        });

        blocksCooldown[player.Slot].Invincibility = true;
        BlockCooldownTimer(player, "Invincibility", settings.Invincibility.Cooldown);
    }

    private static void Action_Camouflage(CCSPlayerController player, CBaseEntity block)
    {
        if (blocksCooldown[player.Slot].Camouflage)
            return;

        var model = player.Pawn()!.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

        if (player.IsT())
            player.SetModel(settings.Camouflage.ModelT);
        else if (player.IsCT())
            player.SetModel(settings.Camouflage.ModelCT);

        player.PlaySound(sounds.Camouflage);

        instance.AddTimer(settings.Camouflage.Duration, () =>
        {
            player.SetModel(model);
            instance.PrintToChat(player, $"{block.Entity!.Name} has worn off");
        });

        blocksCooldown[player.Slot].Camouflage = true;
        BlockCooldownTimer(player, "Camouflage", settings.Camouflage.Cooldown);
    }

    /*
    private static void Action_NoFallDmg(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_Trampoline(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_SpeedBoost(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_Platform(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_Honey(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_Glass(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_TBarrier(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_CTBarrier(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_Ice(CCSPlayerController player, CBaseEntity block) { }
    private static void Action_NoSlowDown(CCSPlayerController player, CBaseEntity block) { }
    */
}