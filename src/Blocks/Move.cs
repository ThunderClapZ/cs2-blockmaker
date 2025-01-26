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

                if (player.Buttons.HasFlag(PlayerButtons.Reload))
                    RotateRepeat(player, playerHolds.block);

                else if (player.Buttons.HasFlag(PlayerButtons.Use))
                    DistanceRepeat(player, playerHolds.block);

                else
                {
                    var color = Utils.GetColor(BlocksEntities[playerHolds.block].Color);
                    int alpha = Utils.GetAlpha(BlocksEntities[playerHolds.block].Transparency);

                    playerHolds.block.Render = Color.FromArgb(alpha, color.R, color.G, color.B);
                    Utilities.SetStateChanged(playerHolds.block, "CBaseModelEntity", "m_clrRender");

                    PlayerHolds.Remove(player);

                    if (config.Sounds.Building.Enabled)
                        player.PlaySound(config.Sounds.Building.Place);
                }
            }
        }
    }

    private static void GrabBlock(CCSPlayerController player)
    {
        var block = player.GetBlockAimTarget();

        if (block != null)
        {
            if (!BlocksEntities.ContainsKey(block))
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Block not found in UsedBlocks");
                return;
            }

            GrabBlockAdd(player, block);
        }
    }

    private static void GrabBlockAdd(CCSPlayerController player, CBaseProp block)
    {
        var pawn = player.Pawn()!;

        Vector position = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.CameraServices!.OldPlayerViewOffsetZ);

        var hitPoint = RayTrace.TraceShape(position, pawn.EyeAngles!, false, true);

        if (block != null && block.IsValid && hitPoint != null && hitPoint.HasValue)
        {
            if (VectorUtils.CalculateDistance(block.AbsOrigin!, RayTrace.Vector3toVector(hitPoint.Value)) > 150)
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Distance too large between block and aim location");
                return;
            }

            int distance = (int)VectorUtils.CalculateDistance(block.AbsOrigin!, pawn.AbsOrigin);

            block.Render = Utils.ParseColor(config.Settings.Building.BlockGrabColor);
            Utilities.SetStateChanged(block, "CBaseModelEntity", "m_clrRender");

            PlayerHolds.Add(player, new BuildingData() { block = block, distance = distance });
        }
    }

    private static void DistanceRepeat(CCSPlayerController player, CBaseProp block)
    {
        var playerHolds = PlayerHolds[player];
        var playerData = instance.playerData[player.Slot];

        var (position, rotation) = VectorUtils.GetEndXYZ(player, block, playerHolds.distance, playerData.Grid, playerData.GridValue, playerData.Snapping, Utils.GetSize(BlocksEntities[block].Size));
        
        block.Teleport(position, rotation);

        if (player.Buttons.HasFlag(PlayerButtons.Attack))
        {
            if (playerHolds.distance > 350) playerHolds.distance += 7;
                playerHolds.distance += 3;
        }

        else if (player.Buttons.HasFlag(PlayerButtons.Attack2) && playerHolds.distance > 3)
        {
            if (playerHolds.distance > 350) playerHolds.distance -= 7;
                playerHolds.distance -= 3;
        }
    }

    private static void RotateRepeat(CCSPlayerController player, CBaseProp block)
    {
        var playerHolds = PlayerHolds[player];
        var playerData = instance.playerData[player.Slot];

        var (position, rotation) = VectorUtils.GetEndXYZ(player, block, playerHolds.distance, playerData.Grid, playerData.GridValue, playerData.Snapping, Utils.GetSize(BlocksEntities[block].Size));

        block.Teleport(position, rotation);

        if (player.Buttons.HasFlag(PlayerButtons.Attack))
        {
            var AbsRotation = playerHolds.block.AbsRotation!;
            QAngle angle = new QAngle(AbsRotation.X, AbsRotation.Y + 3, AbsRotation.Z);

            playerHolds.block.Teleport(null, angle);
        }

        else if (player.Buttons.HasFlag(PlayerButtons.Attack2))
        {
            var AbsRotation = playerHolds.block.AbsRotation!;
            QAngle angle = new QAngle(AbsRotation.X, AbsRotation.Y, AbsRotation.Z + 3);

            playerHolds.block.Teleport(null, angle);
        }
    }
}