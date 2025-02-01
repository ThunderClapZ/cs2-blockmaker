public class BlockModel
{
    public string Title { get; set; } = "";
    public string Block { get; set; } = "";
    public string Pole { get; set; } = "";
}

public class BlockSize
{
    public string Title { get; set; }
    public float Size { get; set; }

    public BlockSize(string title, float size)
    {
        Title = title;
        Size = size;
    }
}

public class BlockModels
{
    public BlockPlatform Platform { get; set; } = new BlockPlatform();
    public BlockBhop Bhop { get; set; } = new BlockBhop();
    public BlockHealth Health { get; set; } = new BlockHealth();
    public BlockGrenade Grenade { get; set; } = new BlockGrenade();
    public BlockGravity Gravity { get; set; } = new BlockGravity();
    public BlockGlass Glass { get; set; } = new BlockGlass();
    public BlockFrost Frost { get; set; } = new BlockFrost();
    public BlockFlash Flash { get; set; } = new BlockFlash();
    public BlockFire Fire { get; set; } = new BlockFire();
    public BlockDelay Delay { get; set; } = new BlockDelay();
    public BlockDeath Death { get; set; } = new BlockDeath();
    public BlockDamage Damage { get; set; } = new BlockDamage();
    public BlockPistol Pistol { get; set; } = new BlockPistol();
    public BlockRifle Rifle { get; set; } = new BlockRifle();
    public BlockSniper Sniper { get; set; } = new BlockSniper();
    public BlockSMG SMG { get; set; } = new BlockSMG();
    public BlockShotgunHeavy ShotgunHeavy { get; set; } = new BlockShotgunHeavy();
    public BlockStealth Stealth { get; set; } = new BlockStealth();
    public BlockSpeed Speed { get; set; } = new BlockSpeed();
    public BlockSpeedBoost SpeedBoost { get; set; } = new BlockSpeedBoost();
    public BlockSlap Slap { get; set; } = new BlockSlap();
    public BlockRandom Random { get; set; } = new BlockRandom();
    public BlockNuke Nuke { get; set; } = new BlockNuke();
    public BlockInvincibility Invincibility { get; set; } = new BlockInvincibility();
    public BlockIce Ice { get; set; } = new BlockIce();
    public BlockCamouflage Camouflage { get; set; } = new BlockCamouflage();
    public BlockTrampoline Trampoline { get; set; } = new BlockTrampoline();
    public BlockNoFallDmg NoFallDmg { get; set; } = new BlockNoFallDmg();
}


public class BlockPlatform : BlockModel
{
    public BlockPlatform()
    {
        Title = "Platform";
        Block = "models/blockbuilder/platform.vmdl";
        Pole = "models/blockbuilder/pole_platform.vmdl";
    }
}

public class BlockBhop : BlockModel
{
    public BlockBhop()
    {
        Title = "Bhop";
        Block = "models/blockbuilder/bhop.vmdl";
        Pole = "models/blockbuilder/pole_bhop.vmdl";
    }
}

public class BlockNoFallDmg : BlockModel
{
    public BlockNoFallDmg()
    {
        Title = "NoFallDmg";
        Block = "models/blockbuilder/nofalldmg.vmdl";
        Pole = "models/blockbuilder/pole_nofalldmg.vmdl";
    }
}

public class BlockHealth : BlockModel
{
    public BlockHealth()
    {
        Title = "Health";
        Block = "models/blockbuilder/health.vmdl";
        Pole = "models/blockbuilder/pole_health.vmdl";
    }
}

public class BlockGrenade : BlockModel
{
    public BlockGrenade()
    {
        Title = "Grenade";
        Block = "models/blockbuilder/he.vmdl";
        Pole = "models/blockbuilder/pole_he.vmdl";
    }
}

public class BlockGravity : BlockModel
{
    public BlockGravity()
    {
        Title = "Gravity";
        Block = "models/blockbuilder/gravity.vmdl";
        Pole = "models/blockbuilder/pole_gravity.vmdl";
    }
}

public class BlockGlass : BlockModel
{
    public BlockGlass()
    {
        Title = "Glass";
        Block = "models/blockbuilder/glass.vmdl";
        Pole = "models/blockbuilder/pole_glass.vmdl";
    }
}

public class BlockFrost : BlockModel
{
    public BlockFrost()
    {
        Title = "Frost";
        Block = "models/blockbuilder/frost.vmdl";
        Pole = "models/blockbuilder/pole_frost.vmdl";
    }
}

public class BlockFlash : BlockModel
{
    public BlockFlash()
    {
        Title = "Flash";
        Block = "models/blockbuilder/flash.vmdl";
        Pole = "models/blockbuilder/pole_flash.vmdl";
    }
}

public class BlockFire : BlockModel
{
    public BlockFire()
    {
        Title = "Fire";
        Block = "models/blockbuilder/fire.vmdl";
        Pole = "models/blockbuilder/pole_fire.vmdl";
    }
}

public class BlockDelay : BlockModel
{
    public BlockDelay()
    {
        Title = "Delay";
        Block = "models/blockbuilder/delay.vmdl";
        Pole = "models/blockbuilder/pole_delay.vmdl";
    }
}

public class BlockDeath : BlockModel
{
    public BlockDeath()
    {
        Title = "Death";
        Block = "models/blockbuilder/death.vmdl";
        Pole = "models/blockbuilder/pole_death.vmdl";
    }
}

public class BlockDamage : BlockModel
{
    public BlockDamage()
    {
        Title = "Damage";
        Block = "models/blockbuilder/damage.vmdl";
        Pole = "models/blockbuilder/pole_damage.vmdl";
    }
}

public class BlockPistol : BlockModel
{
    public BlockPistol()
    {
        Title = "Pistol";
        Block = "models/blockbuilder/deagle.vmdl";
        Pole = "models/blockbuilder/pole_deagle.vmdl";
    }
}

public class BlockRifle : BlockModel
{
    public BlockRifle()
    {
        Title = "Rifle";
        Block = "models/blockbuilder/awp.vmdl";
        Pole = "models/blockbuilder/pole_awp.vmdl";
    }
}

public class BlockSniper : BlockModel
{
    public BlockSniper()
    {
        Title = "Sniper";
        Block = "models/blockbuilder/awp.vmdl";
        Pole = "models/blockbuilder/pole_awp.vmdl";
    }
}

public class BlockSMG : BlockModel
{
    public BlockSMG()
    {
        Title = "SMG";
        Block = "models/blockbuilder/awp.vmdl";
        Pole = "models/blockbuilder/pole_awp.vmdl";
    }
}

public class BlockShotgunHeavy : BlockModel
{
    public BlockShotgunHeavy()
    {
        Title = "Shotgun/Heavy";
        Block = "models/blockbuilder/awp.vmdl";
        Pole = "models/blockbuilder/pole_awp.vmdl";
    }
}

public class BlockTrampoline : BlockModel
{
    public BlockTrampoline()
    {
        Title = "Trampoline";
        Block = "models/blockbuilder/tramp.vmdl";
        Pole = "models/blockbuilder/pole_tramp.vmdl";
    }
}

public class BlockStealth : BlockModel
{
    public BlockStealth()
    {
        Title = "Stealth";
        Block = "models/blockbuilder/stealth.vmdl";
        Pole = "models/blockbuilder/pole_stealth.vmdl";
    }
}

public class BlockSpeedBoost : BlockModel
{
    public BlockSpeedBoost()
    {
        Title = "SpeedBoost";
        Block = "models/blockbuilder/speedboost.vmdl";
        Pole = "models/blockbuilder/pole_speedboost.vmdl";
    }
}

public class BlockSpeed : BlockModel
{
    public BlockSpeed()
    {
        Title = "Speed";
        Block = "models/blockbuilder/speed.vmdl";
        Pole = "models/blockbuilder/pole_speed.vmdl";
    }
}

public class BlockSlap : BlockModel
{
    public BlockSlap()
    {
        Title = "Slap";
        Block = "models/blockbuilder/slap.vmdl";
        Pole = "models/blockbuilder/pole_slap.vmdl";
    }
}

public class BlockRandom : BlockModel
{
    public BlockRandom()
    {
        Title = "Random";
        Block = "models/blockbuilder/random.vmdl";
        Pole = "models/blockbuilder/pole_random.vmdl";
    }
}

public class BlockNuke : BlockModel
{
    public BlockNuke()
    {
        Title = "Nuke";
        Block = "models/blockbuilder/nuke.vmdl";
        Pole = "models/blockbuilder/pole_nuke.vmdl";
    }
}

public class BlockInvincibility : BlockModel
{
    public BlockInvincibility()
    {
        Title = "Invincibility";
        Block = "models/blockbuilder/invincibility.vmdl";
        Pole = "models/blockbuilder/pole_invincibility.vmdl";
    }
}

public class BlockIce : BlockModel
{
    public BlockIce()
    {
        Title = "Ice";
        Block = "models/blockbuilder/ice.vmdl";
        Pole = "models/blockbuilder/pole_ice.vmdl";
    }
}

public class BlockCamouflage : BlockModel
{
    public BlockCamouflage()
    {
        Title = "Camouflage";
        Block = "models/blockbuilder/camouflage.vmdl";
        Pole = "models/blockbuilder/pole_camouflage.vmdl";
    }
}