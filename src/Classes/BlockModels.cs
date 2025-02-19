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
    public BlockHoney Honey { get; set; } = new BlockHoney();
}


public class BlockPlatform : BlockModel
{
    public BlockPlatform()
    {
        Title = "Platform";
        Block = "models/blockmaker/platform/block.vmdl";
        Pole = "models/blockmaker/platform/pole.vmdl";
    }
}

public class BlockBhop : BlockModel
{
    public BlockBhop()
    {
        Title = "Bhop";
        Block = "models/blockmaker/bhop/block.vmdl";
        Pole = "models/blockmaker/bhop/pole.vmdl";
    }
}

public class BlockNoFallDmg : BlockModel
{
    public BlockNoFallDmg()
    {
        Title = "NoFallDmg";
        Block = "models/blockmaker/nofall/block.vmdl";
        Pole = "models/blockmaker/nofall/pole.vmdl";
    }
}

public class BlockHealth : BlockModel
{
    public BlockHealth()
    {
        Title = "Health";
        Block = "models/blockmaker/health/block.vmdl";
        Pole = "models/blockmaker/health/pole.vmdl";
    }
}

public class BlockGrenade : BlockModel
{
    public BlockGrenade()
    {
        Title = "Grenade";
        Block = "models/blockmaker/grenade/block.vmdl";
        Pole = "models/blockmaker/grenade/pole.vmdl";
    }
}

public class BlockGravity : BlockModel
{
    public BlockGravity()
    {
        Title = "Gravity";
        Block = "models/blockmaker/gravity/block.vmdl";
        Pole = "models/blockmaker/gravity/pole.vmdl";
    }
}

public class BlockGlass : BlockModel
{
    public BlockGlass()
    {
        Title = "Glass";
        Block = "models/blockmaker/glass/block.vmdl";
        Pole = "models/blockmaker/glass/pole.vmdl";
    }
}

public class BlockFrost : BlockModel
{
    public BlockFrost()
    {
        Title = "Frost";
        Block = "models/blockmaker/frost/block.vmdl";
        Pole = "models/blockmaker/frost/pole.vmdl";
    }
}

public class BlockFlash : BlockModel
{
    public BlockFlash()
    {
        Title = "Flash";
        Block = "models/blockmaker/flash/block.vmdl";
        Pole = "models/blockmaker/flash/pole.vmdl";
    }
}

public class BlockFire : BlockModel
{
    public BlockFire()
    {
        Title = "Fire";
        Block = "models/blockmaker/fire/block.vmdl";
        Pole = "models/blockmaker/fire/pole.vmdl";
    }
}

public class BlockDelay : BlockModel
{
    public BlockDelay()
    {
        Title = "Delay";
        Block = "models/blockmaker/delay/block.vmdl";
        Pole = "models/blockmaker/delay/pole.vmdl";
    }
}

public class BlockDeath : BlockModel
{
    public BlockDeath()
    {
        Title = "Death";
        Block = "models/blockmaker/death/block.vmdl";
        Pole = "models/blockmaker/death/pole.vmdl";
    }
}

public class BlockDamage : BlockModel
{
    public BlockDamage()
    {
        Title = "Damage";
        Block = "models/blockmaker/damage/block.vmdl";
        Pole = "models/blockmaker/damage/pole.vmdl";
    }
}

public class BlockPistol : BlockModel
{
    public BlockPistol()
    {
        Title = "Pistol";
        Block = "models/blockmaker/pistol/block.vmdl";
        Pole = "models/blockmaker/pistol/pole.vmdl";
    }
}

public class BlockRifle : BlockModel
{
    public BlockRifle()
    {
        Title = "Rifle";
        Block = "models/blockmaker/rifle/block.vmdl";
        Pole = "models/blockmaker/rifle/pole.vmdl";
    }
}

public class BlockSniper : BlockModel
{
    public BlockSniper()
    {
        Title = "Sniper";
        Block = "models/blockmaker/sniper/block.vmdl";
        Pole = "models/blockmaker/sniper/pole.vmdl";
    }
}

public class BlockSMG : BlockModel
{
    public BlockSMG()
    {
        Title = "SMG";
        Block = "models/blockmaker/smg/block.vmdl";
        Pole = "models/blockmaker/smg/pole.vmdl";
    }
}

public class BlockShotgunHeavy : BlockModel
{
    public BlockShotgunHeavy()
    {
        Title = "Shotgun/Heavy";
        Block = "models/blockmaker/heavy/block.vmdl";
        Pole = "models/blockmaker/heavy/pole.vmdl";
    }
}

public class BlockTrampoline : BlockModel
{
    public BlockTrampoline()
    {
        Title = "Trampoline";
        Block = "models/blockmaker/trampoline/block.vmdl";
        Pole = "models/blockmaker/trampoline/pole.vmdl";
    }
}

public class BlockStealth : BlockModel
{
    public BlockStealth()
    {
        Title = "Stealth";
        Block = "models/blockmaker/stealth/block.vmdl";
        Pole = "models/blockmaker/stealth/pole.vmdl";
    }
}

public class BlockSpeedBoost : BlockModel
{
    public BlockSpeedBoost()
    {
        Title = "SpeedBoost";
        Block = "models/blockmaker/speedboost/block.vmdl";
        Pole = "models/blockmaker/speedboost/pole.vmdl";
    }
}

public class BlockSpeed : BlockModel
{
    public BlockSpeed()
    {
        Title = "Speed";
        Block = "models/blockmaker/speed/block.vmdl";
        Pole = "models/blockmaker/speed/pole.vmdl";
    }
}

public class BlockSlap : BlockModel
{
    public BlockSlap()
    {
        Title = "Slap";
        Block = "models/blockmaker/slap/block.vmdl";
        Pole = "models/blockmaker/slap/pole.vmdl";
    }
}

public class BlockRandom : BlockModel
{
    public BlockRandom()
    {
        Title = "Random";
        Block = "models/blockmaker/random/block.vmdl";
        Pole = "models/blockmaker/random/pole.vmdl";
    }
}

public class BlockNuke : BlockModel
{
    public BlockNuke()
    {
        Title = "Nuke";
        Block = "models/blockmaker/nuke/block.vmdl";
        Pole = "models/blockmaker/nuke/pole.vmdl";
    }
}

public class BlockInvincibility : BlockModel
{
    public BlockInvincibility()
    {
        Title = "Invincibility";
        Block = "models/blockmaker/invincibility/block.vmdl";
        Pole = "models/blockmaker/invincibility/pole.vmdl";
    }
}

public class BlockIce : BlockModel
{
    public BlockIce()
    {
        Title = "Ice";
        Block = "models/blockmaker/ice/block.vmdl";
        Pole = "models/blockmaker/ice/pole.vmdl";
    }
}

public class BlockCamouflage : BlockModel
{
    public BlockCamouflage()
    {
        Title = "Camouflage";
        Block = "models/blockmaker/camouflage/block.vmdl";
        Pole = "models/blockmaker/camouflage/pole.vmdl";
    }
}

public class BlockHoney : BlockModel
{
    public BlockHoney()
    {
        Title = "Honey";
        Block = "models/blockmaker/honey/block.vmdl";
        Pole = "models/blockmaker/honey/pole.vmdl";
    }
}
