using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Data;
using System.Drawing;

public partial class Blocks
{
    public static Dictionary<CCSPlayerController, BuildingData> PlayerHolds = new Dictionary<CCSPlayerController, BuildingData>();

    public static void OnTick()
    {
        if (!instance.buildMode)
            return;

        foreach (var player in Utilities.GetPlayers().Where(p =>
            p.IsLegal() &&
            p.IsAlive() &&
            instance.playerData.ContainsKey(p.Slot) &&
            instance.playerData[p.Slot].Builder)
        )
        {
            if (!PlayerHolds.ContainsKey(player))
            {
                if (player.Buttons.HasFlag(PlayerButtons.Reload) || player.Buttons.HasFlag(PlayerButtons.Use))
                    GrabBlock(player);
            }
            else
            {
                var playerHolds = PlayerHolds[player];

                if (playerHolds.block == null || !playerHolds.block.IsValid)
                {
                    PlayerHolds.Remove(player);
                    continue;
                }

                if (config.Settings.Building.Grab.Beams)
                    Utils.DrawBeamsAroundBlock(player, playerHolds.block, Utils.ParseColor(config.Settings.Building.Grab.BeamsColor));

                if (player.Buttons.HasFlag(PlayerButtons.Use))
                    DistanceRepeat(player, playerHolds.block);

                else if (player.Buttons.HasFlag(PlayerButtons.Reload))
                    RotateRepeat(player, playerHolds.block);

                else
                {
                    if (Props.TryGetValue(playerHolds.block, out var block))
                    {
                        var color = Utils.GetColor(block.Color);
                        int alpha = Utils.GetAlpha(block.Transparency);

                        block.Entity.Render = Color.FromArgb(alpha, color.R, color.G, color.B);
                        Utilities.SetStateChanged(block.Entity, "CBaseModelEntity", "m_clrRender");
                    }

                    foreach (var beam in playerHolds.beams)
                    {
                        if (beam != null && beam.IsValid)
                            beam.Remove();
                    }

                    PlayerHolds.Remove(player);

                    if (config.Sounds.Building.Enabled)
                        player.EmitSound(config.Sounds.Building.Place);
                }
            }
        }
    }

    private static void GrabBlock(CCSPlayerController player)
    {
        var block = player.GetBlockAimTarget();

        if (block != null)
        {
            var teleports = Teleports.FirstOrDefault(pair => pair.Entry.Entity == block || pair.Exit.Entity == block);

            if (!Props.ContainsKey(block) && teleports == null)
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Block not found in UsedBlocks");
                return;
            }

            var pawn = player.Pawn()!;

            Vector position = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.CameraServices!.OldPlayerViewOffsetZ);

            var hitPoint = RayTrace.TraceShape(position, pawn.EyeAngles!);

            string size = "1";

            if (Props.ContainsKey(block))
                size = Props[block].Size;

            if (block != null && hitPoint != null)
            {
                if (VectorUtils.CalculateDistance(block.AbsOrigin!, hitPoint) > (block.Collision.Maxs.X * 2 * Utils.GetSize(size)))
                {
                    //Utils.PrintToChat(player, $"{ChatColors.Red}Distance too large between block and aim location");
                    return;
                }

                int distance = (int)VectorUtils.CalculateDistance(block.AbsOrigin!, position);

                if (Props.ContainsKey(block))
                {
                    block.Render = Utils.ParseColor(config.Settings.Building.Grab.RenderColor);
                    Utilities.SetStateChanged(block, "CBaseModelEntity", "m_clrRender");
                }

                PlayerHolds.Add(player, new BuildingData() { block = block, distance = distance });
            }
        }
    }

    private static void DistanceRepeat(CCSPlayerController player, CBaseProp block)
    {
        var playerHolds = PlayerHolds[player];
        var playerData = instance.playerData[player.Slot];

        var (position, rotation) =
            VectorUtils.GetEndXYZ(
                player,
                block,
                playerHolds.distance,
                playerData.Grid,
                playerData.GridValue,
                playerData.Snapping,
                playerData.SnapValue
            );

        block.Teleport(position, rotation);

        if (player.Buttons.HasFlag(PlayerButtons.Attack))
            playerHolds.distance += 3;

        else if (player.Buttons.HasFlag(PlayerButtons.Attack2))
            playerHolds.distance -= 3;
    }

    private static void RotateRepeat(CCSPlayerController player, CBaseProp block)
    {
        var playerHolds = PlayerHolds[player];

        QAngle currentEyeAngle = player.Pawn()!.EyeAngles;

        QAngle blockRotation = new(
            0 + (currentEyeAngle.X * 7.5f),
            0 + (currentEyeAngle.Y * 7.5f),
            0 + (currentEyeAngle.Z * 7.5f)
        );

        block.Teleport(null, blockRotation);
    }
}