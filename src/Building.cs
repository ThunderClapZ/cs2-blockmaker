using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CS2TraceRay.Class;
using CS2TraceRay.Enum;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;
using StarCore.Utils;
using System.Data;
using System.Drawing;
using System.Reflection.Metadata;

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
        public string LightStyle = "None";
        public string LightBrightness = "1";
        public string LightDistance = "500";
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
        public Vector_t Offset = new();
        public int Distance = 0;
        public List<CBeam> Beams = new();
        public bool LockedMessage = false;
        
        // 新增旋转相关字段
        public QAngle_t InitialEyeAngles = new QAngle_t();
        public QAngle_t InitialBlockRotation = new QAngle_t();
        public bool InitialEyeAnglesSet = false;
        public DateTime LastMoveTime = DateTime.Now;
    }

    private static Plugin Instance = Plugin.Instance;
    private static Config Config = Instance.Config;

    public static Dictionary<CCSPlayerController, BuildData> PlayerHolds = new Dictionary<CCSPlayerController, BuildData>();
    public static Dictionary<CBaseProp, CCSPlayerController> ControlledBlocks = new Dictionary<CBaseProp, CCSPlayerController>();
    public static Dictionary<CBaseProp, (CCSPlayerController player, DateTime time)> LastBlockMovers = new Dictionary<CBaseProp, (CCSPlayerController, DateTime)>();

    public static void OnTick()
    {
        if (Lib.IsWarmupPeriod()) return;
        if (!Instance.buildMode) return;
        foreach (var player in Utilities.GetPlayers().Where(p => p.IsLegal() && p.IsAlive() && Instance.BuilderData.ContainsKey(p.Slot)))
        {
            if (!PlayerHolds.ContainsKey(player))
            {
                if (player.Buttons.HasFlag(PlayerButtons.Reload) || player.Buttons.HasFlag(PlayerButtons.Use))
                {
                    GrabBlock(player);
                }
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

                // 处理距离调整
                if (player.Buttons.HasFlag(PlayerButtons.Use))
                {
                    DistanceRepeat(player, playerHolds.Entity);
                        
                    // 如果同时按着Reload键，重置旋转状态但不旋转
                    if (player.Buttons.HasFlag(PlayerButtons.Reload))
                    {
                        playerHolds.InitialEyeAnglesSet = false;
                    }
                }
                // 处理旋转
                else if (player.Buttons.HasFlag(PlayerButtons.Reload))
                {
                    RotateRepeat(player, playerHolds.Entity);
                }
                // 释放方块
                else
                {
                    ReleaseBlock(player, playerHolds);
                }
            }
            var entity = player.GetBlockAim();
            if (entity != null)
            {
                ShowLastMoverInfo(player, entity);
            }
        }
    }

    private static void ReleaseBlock(CCSPlayerController player, BuildData playerHolds)
    {
        if (Blocks.Entities.TryGetValue(playerHolds.Entity, out var block))
        {
            var color = Utils.GetColor(block.Color);
            int alpha = Utils.GetAlpha(block.Transparency);

            block.Entity.Render = Color.FromArgb(alpha, color.R, color.G, color.B);
            Utilities.SetStateChanged(block.Entity, "CBaseModelEntity", "m_clrRender");

            block.Entity.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEFAULT;
            block.Entity.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEFAULT;
            block.Entity.CollisionRulesChanged();
        }

        foreach (var beam in playerHolds.Beams)
        {
            if (beam != null && beam.IsValid)
                beam.Remove();
        }

        // 更新最后移动者记录
        LastBlockMovers[playerHolds.Entity] = (player, DateTime.Now);
        
        // 释放控制权
        ControlledBlocks.Remove(playerHolds.Entity);
        
        // 重置旋转状态
        playerHolds.InitialEyeAnglesSet = false;
        PlayerHolds.Remove(player);

        if (Config.Sounds.Building.Enabled)
            player.EmitSound(Config.Sounds.Building.Place);
    }


    private static void GrabBlock(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();

        if (entity != null)
        {
            // 检查是否已被其他玩家控制
            if (ControlledBlocks.TryGetValue(entity, out var currentController) && currentController != player) return;

            bool block = Blocks.Entities.ContainsKey(entity);
            bool light = Lights.Entities.ContainsKey(entity);
            var teleports = Teleports.Entities.FirstOrDefault(pair => pair.Entry.Entity == entity || pair.Exit.Entity == entity);

            if (!block && !light && teleports == null)
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}查询不到数据");
                return;
            }

            if (Blocks.Entities.TryGetValue(entity, out var blockData))
            {
                CCSPlayerController? owner = blockData.Owner;
                if (owner != null && owner != player)
                {
                    Utils.PrintToChat(player, $"这是别人的专属板块!");
                    return;
                }
            }

            var pawn = player.Pawn()!;

            Vector_t position = new(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + pawn.CameraServices!.OldPlayerViewOffsetZ);

            CGameTrace? trace = TraceRay.TraceShape(player.GetEyePosition()!, pawn.EyeAngles, TraceMask.MaskShot, player);
            if (trace == null || !trace.HasValue || trace.Value.Position.Length() == 0)
                return;

            var endPos = trace.Value.Position;

            string size = "1";

            if (block)
                size = Blocks.Entities[entity].Size;

            if (VectorUtils.CalculateDistance(entity.AbsOrigin!.ToVector_t(), new(endPos.X, endPos.Y, endPos.Z)) > (entity.Collision.Maxs.X * 2 * Utils.GetSize(size)))
            {
                return;
            }

            int distance = (int)VectorUtils.CalculateDistance(entity.AbsOrigin!.ToVector_t(), position);

            if (block)
            {
                entity.Render = Utils.ParseColor(Config.Settings.Building.Grab.RenderColor);
                Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");
            }

            // 记录控制权
            ControlledBlocks[entity] = player;
            
            PlayerHolds.Add(player, new BuildData() { 
                Entity = entity, 
                Distance = distance
            });

            entity.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            entity.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            entity.CollisionRulesChanged();
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
        
        // 更新最后移动者信息
        playerHolds.LastMoveTime = DateTime.Now;

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
                    Utils.PrintToChat(player, $"{ChatColors.Red}板块已被锁定");

                PlayerHolds[player].LockedMessage = true;
                return;
            }
        }

        var playerHolds = PlayerHolds[player];
        QAngle_t currentEyeAngle = player.Pawn()!.EyeAngles.ToQAngle_t();

        if (!playerHolds.InitialEyeAnglesSet)
        {
            playerHolds.InitialEyeAngles = currentEyeAngle;
            playerHolds.InitialBlockRotation = block.AbsRotation!.ToQAngle_t();
            playerHolds.InitialEyeAnglesSet = true;
        }

        QAngle_t eyeAngleDelta = new QAngle_t(
            currentEyeAngle.X - playerHolds.InitialEyeAngles.X,
            currentEyeAngle.Y - playerHolds.InitialEyeAngles.Y,
            currentEyeAngle.Z - playerHolds.InitialEyeAngles.Z
        );

        if (Math.Abs(eyeAngleDelta.X) > 0.1f || Math.Abs(eyeAngleDelta.Y) > 0.1f || Math.Abs(eyeAngleDelta.Z) > 0.1f)
        {
            // 计算新的旋转角度（乘以7.5f保持原有灵敏度）
            float newX = playerHolds.InitialBlockRotation.X + (eyeAngleDelta.X * 7.5f);
            float newY = playerHolds.InitialBlockRotation.Y + (eyeAngleDelta.Y * 7.5f);
            float newZ = playerHolds.InitialBlockRotation.Z + (eyeAngleDelta.Z * 7.5f);

            // 强制对齐到最近的90度倍数
            newX = MathF.Round(newX / 90f) * 90f;
            newY = MathF.Round(newY / 90f) * 90f;
            newZ = MathF.Round(newZ / 90f) * 90f;

            // 确保角度在0-360范围内
            newX = NormalizeAngle(newX);
            newY = NormalizeAngle(newY);
            newZ = NormalizeAngle(newZ);

            QAngle_t newRotation = new(newX, newY, newZ);
            block.Teleport(null, newRotation);
            
            playerHolds.LastMoveTime = DateTime.Now;
        }
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    public static void ShowLastMoverInfo(CCSPlayerController player, CBaseProp block)
    {
        // 首先检查是否正在被控制
        if (ControlledBlocks.TryGetValue(block, out var currentController))
        {
            player.PrintToCenter($"正在被 {currentController.PlayerName} 移动中");
            return;
        }
        
        // 然后检查最后移动记录
        if (LastBlockMovers.TryGetValue(block, out var lastMover))
        {
            var timeAgo = DateTime.Now - lastMover.time;
            player.PrintToCenter($"最后移动者: {lastMover.player.PlayerName} ({timeAgo.TotalSeconds:F0}秒前)");
        }
    }
}