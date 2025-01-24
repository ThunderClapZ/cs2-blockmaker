public class BlockSizes
{
    public string Title { get; set; } = "";
    public string Block { get; set; } = "";
    public string Pole { get; set; } = "";
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
    public BlockDeagle Deagle { get; set; } = new BlockDeagle();
    public BlockAWP AWP { get; set; } = new BlockAWP();
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
    //public BlockNoSlowDown NoSlowDown { get; set; } = new BlockNoSlowDown();
    //public BlockTBarrier TBarrier { get; set; } = new BlockTBarrier();
    //public BlockCTBarrier CTBarrier { get; set; } = new BlockCTBarrier();
    //public BlockHoney Honey { get; set; } = new BlockHoney();
}


public class BlockPlatform : BlockSizes
{
    public BlockPlatform()
    {
        Title = "Platform";
        Block = "models/blockbuilder/platform.vmdl";
        Pole = "models/blockbuilder/pole_platform.vmdl";
    }
}

public class BlockBhop : BlockSizes
{
    public BlockBhop()
    {
        Title = "Bhop";
        Block = "models/blockbuilder/bhop.vmdl";
        Pole = "models/blockbuilder/pole_bhop.vmdl";
    }
}

public class BlockNoFallDmg : BlockSizes
{
    public BlockNoFallDmg()
    {
        Title = "NoFallDmg";
        Block = "models/blockbuilder/nofalldmg.vmdl";
        Pole = "models/blockbuilder/pole_nofalldmg.vmdl";
    }
}

public class BlockHoney : BlockSizes
{
    public BlockHoney()
    {
        Title = "Honey";
        Block = "models/blockbuilder/honey.vmdl";
        Pole = "models/blockbuilder/pole_honey.vmdl";
    }
}

public class BlockHealth : BlockSizes
{
    public BlockHealth()
    {
        Title = "Health";
        Block = "models/blockbuilder/health.vmdl";
        Pole = "models/blockbuilder/pole_health.vmdl";
    }
}

public class BlockGrenade : BlockSizes
{
    public BlockGrenade()
    {
        Title = "Grenade";
        Block = "models/blockbuilder/he.vmdl";
        Pole = "models/blockbuilder/pole_he.vmdl";
    }
}

public class BlockGravity : BlockSizes
{
    public BlockGravity()
    {
        Title = "Gravity";
        Block = "models/blockbuilder/gravity.vmdl";
        Pole = "models/blockbuilder/pole_gravity.vmdl";
    }
}

public class BlockGlass : BlockSizes
{
    public BlockGlass()
    {
        Title = "Glass";
        Block = "models/blockbuilder/glass.vmdl";
        Pole = "models/blockbuilder/pole_glass.vmdl";
    }
}

public class BlockFrost : BlockSizes
{
    public BlockFrost()
    {
        Title = "Frost";
        Block = "models/blockbuilder/frost.vmdl";
        Pole = "models/blockbuilder/pole_frost.vmdl";
    }
}

public class BlockFlash : BlockSizes
{
    public BlockFlash()
    {
        Title = "Flash";
        Block = "models/blockbuilder/flash.vmdl";
        Pole = "models/blockbuilder/pole_flash.vmdl";
    }
}

public class BlockFire : BlockSizes
{
    public BlockFire()
    {
        Title = "Fire";
        Block = "models/blockbuilder/fire.vmdl";
        Pole = "models/blockbuilder/pole_fire.vmdl";
    }
}

public class BlockDelay : BlockSizes
{
    public BlockDelay()
    {
        Title = "Delay";
        Block = "models/blockbuilder/delay.vmdl";
        Pole = "models/blockbuilder/pole_delay.vmdl";
    }
}

public class BlockDeath : BlockSizes
{
    public BlockDeath()
    {
        Title = "Death";
        Block = "models/blockbuilder/death.vmdl";
        Pole = "models/blockbuilder/pole_death.vmdl";
    }
}

public class BlockDamage : BlockSizes
{
    public BlockDamage()
    {
        Title = "Damage";
        Block = "models/blockbuilder/damage.vmdl";
        Pole = "models/blockbuilder/pole_damage.vmdl";
    }
}

public class BlockDeagle : BlockSizes
{
    public BlockDeagle()
    {
        Title = "Deagle";
        Block = "models/blockbuilder/deagle.vmdl";
        Pole = "models/blockbuilder/pole_deagle.vmdl";
    }
}

public class BlockAWP : BlockSizes
{
    public BlockAWP()
    {
        Title = "AWP";
        Block = "models/blockbuilder/awp.vmdl";
        Pole = "models/blockbuilder/pole_awp.vmdl";
    }
}

public class BlockTrampoline : BlockSizes
{
    public BlockTrampoline()
    {
        Title = "Trampoline";
        Block = "models/blockbuilder/tramp.vmdl";
        Pole = "models/blockbuilder/pole_tramp.vmdl";
    }
}

public class BlockStealth : BlockSizes
{
    public BlockStealth()
    {
        Title = "Stealth";
        Block = "models/blockbuilder/stealth.vmdl";
        Pole = "models/blockbuilder/pole_stealth.vmdl";
    }
}

public class BlockSpeedBoost : BlockSizes
{
    public BlockSpeedBoost()
    {
        Title = "SpeedBoost";
        Block = "models/blockbuilder/speedboost.vmdl";
        Pole = "models/blockbuilder/pole_speedboost.vmdl";
    }
}

public class BlockSpeed : BlockSizes
{
    public BlockSpeed()
    {
        Title = "Speed";
        Block = "models/blockbuilder/speed.vmdl";
        Pole = "models/blockbuilder/pole_speed.vmdl";
    }
}

public class BlockTBarrier : BlockSizes
{
    public BlockTBarrier()
    {
        Title = "T-Barrier";
        Block = "models/blockbuilder/tbarrier.vmdl";
        Pole = "models/blockbuilder/pole_tbarrier.vmdl";
    }
}

public class BlockCTBarrier : BlockSizes
{
    public BlockCTBarrier()
    {
        Title = "CT-Barrier";
        Block = "models/blockbuilder/ctbarrier.vmdl";
        Pole = "models/blockbuilder/pole_ctbarrier.vmdl";
    }
}

public class BlockSlap : BlockSizes
{
    public BlockSlap()
    {
        Title = "Slap";
        Block = "models/blockbuilder/slap.vmdl";
        Pole = "models/blockbuilder/pole_slap.vmdl";
    }
}

public class BlockRandom : BlockSizes
{
    public BlockRandom()
    {
        Title = "Random";
        Block = "models/blockbuilder/random.vmdl";
        Pole = "models/blockbuilder/pole_random.vmdl";
    }
}

public class BlockNuke : BlockSizes
{
    public BlockNuke()
    {
        Title = "Nuke";
        Block = "models/blockbuilder/nuke.vmdl";
        Pole = "models/blockbuilder/pole_nuke.vmdl";
    }
}

public class BlockNoSlowDown : BlockSizes
{
    public BlockNoSlowDown()
    {
        Title = "NoSlowDown";
        Block = "models/blockbuilder/noslowdown.vmdl";
        Pole = "models/blockbuilder/pole_noslowdown.vmdl";
    }
}

public class BlockInvincibility : BlockSizes
{
    public BlockInvincibility()
    {
        Title = "Invincibility";
        Block = "models/blockbuilder/invincibility.vmdl";
        Pole = "models/blockbuilder/pole_invincibility.vmdl";
    }
}

public class BlockIce : BlockSizes
{
    public BlockIce()
    {
        Title = "Ice";
        Block = "models/blockbuilder/ice.vmdl";
        Pole = "models/blockbuilder/pole_ice.vmdl";
    }
}

public class BlockCamouflage : BlockSizes
{
    public BlockCamouflage()
    {
        Title = "Camouflage";
        Block = "models/blockbuilder/camouflage.vmdl";
        Pole = "models/blockbuilder/pole_camouflage.vmdl";
    }
}