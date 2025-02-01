using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

public partial class Blocks
{
    public class BlocksCooldown
    {
        public Dictionary<CBaseEntity, bool> Block = new Dictionary<CBaseEntity, bool>();
    }

    public static Dictionary<int, BlocksCooldown> PlayerCooldowns = new Dictionary<int, BlocksCooldown>();
    public static Dictionary<int, List<Timer>> CooldownsTimers = new();

    private static void BlockCooldownTimer(CCSPlayerController player, CBaseEntity block, float timer = 0, bool message = false)
    {
        if (timer <= 0 || block == null || block.Entity == null)
            return;

        var blockCooldowns = PlayerCooldowns[player.Slot].Block;

        if (!BlockCooldown(player, block))
            blockCooldowns[block] = true;

        var cdtimer = instance.AddTimer(timer, () =>
        {
            if (!player.IsAlive()) return;

            if (blockCooldowns.ContainsKey(block))
            {
                blockCooldowns[block] = false;

                if (message)
                    Utils.PrintToChat(player, $"{ChatColors.White}{block.Entity.Name} {ChatColors.Grey}block is no longer on cooldown");
            }

            else Utils.PrintToChat(player, $"{ChatColors.Red}Error: could not reset cooldown for {block} block");
        });

        CooldownsTimers[player.Slot].Add(cdtimer);
    }

    private static bool BlockCooldown(CCSPlayerController player, CBaseEntity block)
    {
        var blockCooldowns = PlayerCooldowns[player.Slot].Block;
        return blockCooldowns.TryGetValue(block, out bool isOnCooldown) && isOnCooldown;
    }
}