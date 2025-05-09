using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2TraceRay.Class;
using CS2TraceRay.Enum;
using System.Data;
using System.Drawing;

public static class Building
{
    public class BuilderData
    {
        public string BlockType = "Platform";
        public bool BlockPole = false;
        public string BlockSize = "Normal";
        public string BlockTeam = "Both";
        public string BlockColor = "None";
        public string BlockTransparency = "100%";
        public Blocks.Effect BlockEffect = new("None", "");
        public string LightColor = "White";
        public string LightBrightness = "5";
        public string LightDistance = "1000";
        public bool Grid = false;
        public float GridValue = 32f;
        public float SnapValue = 0f;
        public float RotationValue = 90f;
        public float PositionValue = 8f;
        public string MoveAngle = "X+";
        public bool Snapping = false;
        public bool Noclip = false;
        public bool Godmode = false;
        public string ChatInput = "";
        public Dictionary<string, CBaseEntity> PropertyEntity = new();
    }

    public class BuildData
    {
        public CBaseProp Entity = null!;
        public Vector Offset = new();
        public int Distance = 0;
        public List<CBeam> Beams = new();
        public bool LockedMessage = false;
    }

    private static Plugin Instance = Plugin.Instance;
    private static Config Config = Instance.Config;

    public static Dictionary<CCSPlayerController, BuildData> PlayerHolds = new Dictionary<CCSPlayerController, BuildData>();

    public static void OnTick()
    {
        if (!Instance.buildMode)
            return;

        foreach (var player in Utilities.GetPlayers().Where(p =>
            p.IsLegal() &&
            p.IsAlive() &&
            Instance.BuilderData.ContainsKey(p.Slot))
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

                if (playerHolds.Entity == null || !playerHolds.Entity.IsValid)
                {
                    PlayerHolds.Remove(player);
                    continue;
                }

                if (Config.Settings.Building.Grab.Beams)
                    Utils.DrawBeamsAroundBlock(player, playerHolds.Entity, Utils.ParseColor(Config.Settings.Building.Grab.BeamsColor));

                if (player.Buttons.HasFlag(PlayerButtons.Use))
                    DistanceRepeat(player, playerHolds.Entity);

                else if (player.Buttons.HasFlag(PlayerButtons.Reload))
                    RotateRepeat(player, playerHolds.Entity);

                else
                {
                    if (Blocks.Entities.TryGetValue(playerHolds.Entity, out var block))
                    {
                        var color = Utils.GetColor(block.Color);
                        int alpha = Utils.GetAlpha(block.Transparency);

                        block.Entity.Render = Color.FromArgb(alpha, color.R, color.G, color.B);
                        Utilities.SetStateChanged(block.Entity, "CBaseModelEntity", "m_clrRender");
                    }

                    foreach (var beam in playerHolds.Beams)
                    {
                        if (beam != null && beam.IsValid)
                            beam.Remove();
                    }

                    PlayerHolds.Remove(player);

                    if (Config.Sounds.Building.Enabled)
                        player.EmitSound(Config.Sounds.Building.Place);
                }
            }
        }
    }

    private static void GrabBlock(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();

        if (entity != null)
        {
            bool block = Blocks.Entities.ContainsKey(entity);
            bool light = Lights.Entities.ContainsKey(entity);
            var teleports = Teleports.Entities.FirstOrDefault(pair => pair.Entry.Entity == entity || pair.Exit.Entity == entity);

            if (!block && !light && teleports == null)
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Entity not found in data");
                return;
            }

            var pawn = player.Pawn()!;

            Vector position = new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.CameraServices!.OldPlayerViewOffsetZ);

            CGameTrace? trace = TraceRay.TraceShape(player.GetEyePosition()!, pawn.EyeAngles, TraceMask.MaskShot, player);
            if (trace == null || !trace.HasValue || trace.Value.Position.Length() == 0)
                return;

            var endPos = trace.Value.Position;

            string size = "1";

            if (block)
                size = Blocks.Entities[entity].Size;

            if (VectorUtils.CalculateDistance(entity.AbsOrigin!, new(endPos.X, endPos.Y, endPos.Z)) > (entity.Collision.Maxs.X * 2 * Utils.GetSize(size)))
            {
                //Utils.PrintToChat(player, $"{ChatColors.Red}Distance too large between block and aim location");
                return;
            }

            int distance = (int)VectorUtils.CalculateDistance(entity.AbsOrigin!, position);

            if (block)
            {
                entity.Render = Utils.ParseColor(Config.Settings.Building.Grab.RenderColor);
                Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");
            }

            PlayerHolds.Add(player, new BuildData() { Entity = entity, Distance = distance });
            return;
        }
    }

    private static void DistanceRepeat(CCSPlayerController player, CBaseProp block)
    {
        var playerHolds = PlayerHolds[player];
        var BuilderData = Instance.BuilderData[player.Slot];

        var (position, rotation) =
            VectorUtils.GetEndXYZ(
                player,
                block,
                playerHolds.Distance,
                BuilderData.Grid,
                BuilderData.GridValue,
                BuilderData.Snapping,
                BuilderData.SnapValue
            );

        block.Teleport(position, rotation);

        if (player.Buttons.HasFlag(PlayerButtons.Attack))
            playerHolds.Distance += 3;

        else if (player.Buttons.HasFlag(PlayerButtons.Attack2))
            playerHolds.Distance -= 3;
    }

    private static void RotateRepeat(CCSPlayerController player, CBaseProp block)
    {
        if (Blocks.Entities.TryGetValue(block, out var locked))
        {
            if (Blocks.Entities[locked.Entity].Properties.Locked)
            {
                if (PlayerHolds[player].LockedMessage == false)
                    Utils.PrintToChat(player, $"{ChatColors.Red}Block is locked");

                PlayerHolds[player].LockedMessage = true;

                return;
            }
        }

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