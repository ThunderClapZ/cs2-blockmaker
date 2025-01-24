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

        foreach (var player in Utilities.GetPlayers().Where(p => p.IsLegal() && p.IsAlive() && instance.playerData.ContainsKey(p.Slot) && instance.playerData[p.Slot].Builder))
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
                    var color = Plugin.GetColor(UsedBlocks[playerHolds.block].Color);
                    int alpha = Plugin.GetAlpha(UsedBlocks[playerHolds.block].Transparency);

                    playerHolds.block.Render = Color.FromArgb(alpha, color.R, color.G, color.B);
                    Utilities.SetStateChanged(playerHolds.block, "CBaseModelEntity", "m_clrRender");

                    PlayerHolds.Remove(player);

                    if (instance.Config.Sounds.Building.Enabled)
                        player.PlaySound(instance.Config.Sounds.Building.Place);
                }
            }
        }
    }

    public static void GrabBlock(CCSPlayerController player)
    {
        var block = player.GetBlockAimTarget();

        if (block != null)
        {
            if (!UsedBlocks.ContainsKey(block))
            {
                instance.PrintToChat(player, $"{ChatColors.Red}Block not found in UsedBlocks");
                return;
            }

            GrabBlockAdd(player, block);
        }
    }

    public static void GrabBlockAdd(CCSPlayerController player, CBaseProp block)
    {
        var pawn = player.Pawn();

        var hitPoint = RayTrace.TraceShape(new Vector(pawn!.AbsOrigin!.X, pawn.AbsOrigin!.Y, pawn.AbsOrigin!.Z + pawn.CameraServices!.OldPlayerViewOffsetZ), pawn.EyeAngles!, false, true);

        if (block != null && block.IsValid && hitPoint != null && hitPoint.HasValue)
        {
            if (VectorUtils.CalculateDistance(block.AbsOrigin!, RayTrace.Vector3toVector(hitPoint.Value)) > 150)
            {
                instance.PrintToChat(player, $"{ChatColors.Red}Distance too large between block and aim location");
                return;
            }

            int distance = (int)VectorUtils.CalculateDistance(block.AbsOrigin!, player.PlayerPawn.Value!.AbsOrigin!);

            block.Render = instance.ParseColor(instance.Config.Settings.Building.BlockGrabColor);
            Utilities.SetStateChanged(block, "CBaseModelEntity", "m_clrRender");

            PlayerHolds.Add(player, new BuildingData() { block = block, distance = distance });
        }
    }

    public static void DistanceRepeat(CCSPlayerController player, CBaseProp block)
    {
        var playerHolds = PlayerHolds[player];
        var playerData = instance.playerData[player.Slot];

        var (position, rotation) = VectorUtils.GetEndXYZ(player, block, playerHolds.distance, playerData.Grid, playerData.GridValue, playerData.Snapping, Plugin.GetSize(UsedBlocks[block].Size));
        
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

    public static void RotateRepeat(CCSPlayerController player, CBaseProp block)
    {
        var playerHolds = PlayerHolds[player];
        var playerData = instance.playerData[player.Slot];

        var (position, rotation) = VectorUtils.GetEndXYZ(player, block, playerHolds.distance, playerData.Grid, playerData.GridValue, playerData.Snapping, Plugin.GetSize(UsedBlocks[block].Size));

        block.Teleport(position, rotation);

        if (player.Buttons.HasFlag(PlayerButtons.Attack))
            playerHolds.block.Teleport(null, new QAngle(playerHolds.block.AbsRotation!.X, playerHolds.block.AbsRotation!.Y + 3, playerHolds.block.AbsRotation!.Z));

        else if (player.Buttons.HasFlag(PlayerButtons.Attack2))
            playerHolds.block.Teleport(null, new QAngle(playerHolds.block.AbsRotation!.X, playerHolds.block.AbsRotation!.Y, playerHolds.block.AbsRotation!.Z + 3));
    }
}