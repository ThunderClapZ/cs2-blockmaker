using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

public partial class Blocks
{
    public static Dictionary<int, List<CBaseProp>> PlayerCooldowns = new();
    public static Dictionary<int, List<Timer>> CooldownsTimers = new();

    private static void BlockCooldownTimer(CCSPlayerController player, CBaseProp block, float timer = 0, bool message = false)
    {
        if (timer <= 0 || block == null || block.Entity == null)
            return;

        var cooldown = PlayerCooldowns[player.Slot];

        if (!BlockCooldown(player, block))
            cooldown.Add(block);

        var cdtimer = instance.AddTimer(timer, () =>
        {
            if (cooldown.Contains(block))
            {
                cooldown.Remove(block);

                if (message)
                    Utils.PrintToChat(player, $"{ChatColors.White}{block.Entity.Name} {ChatColors.Grey}block is no longer on cooldown");
            }

            //else Utils.PrintToChat(player, $"{ChatColors.Red}Error: could not reset cooldown for {block} block");
        });

        CooldownsTimers[player.Slot].Add(cdtimer);
    }

    private static bool BlockCooldown(CCSPlayerController player, CBaseProp block)
    {
        return PlayerCooldowns.TryGetValue(player.Slot, out var blockList) && blockList.Contains(block);
    }
}