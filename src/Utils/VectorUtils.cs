using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.InteropServices;

public static class VectorUtils
{
    public static bool IsPointOnLine(Vector start, Vector end, Vector point, double threshold)
    {
        double lineDirectionX = end.X - start.X;
        double lineDirectionY = end.Y - start.Y;
        double lineDirectionZ = end.Z - start.Z;

        double pointVectorX = point.X - start.X;
        double pointVectorY = point.Y - start.Y;
        double pointVectorZ = point.Z - start.Z;

        double scalarProjection = (pointVectorX * lineDirectionX + pointVectorY * lineDirectionY + pointVectorZ * lineDirectionZ) /
                                 (lineDirectionX * lineDirectionX + lineDirectionY * lineDirectionY + lineDirectionZ * lineDirectionZ);

        if (scalarProjection >= 0 && scalarProjection <= 1)
        {
            double closestPointX = start.X + scalarProjection * lineDirectionX;
            double closestPointY = start.Y + scalarProjection * lineDirectionY;
            double closestPointZ = start.Z + scalarProjection * lineDirectionZ;

            double distance = Math.Sqrt(Math.Pow(point.X - closestPointX, 2) + Math.Pow(point.Y - closestPointY, 2) + Math.Pow(point.Z - closestPointZ, 2));

            return distance <= threshold;
        }

        return false;
    }

    public static CBaseProp? GetBlockAimTarget(this CCSPlayerController player)
    {
        var GameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;

        if (GameRules is null)
            return null;

        VirtualFunctionWithReturn<IntPtr, IntPtr, IntPtr> findPickerEntity = new(GameRules.Handle, 27);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) findPickerEntity = new(GameRules.Handle, 28);

        var target = new CBaseProp(findPickerEntity.Invoke(GameRules.Handle, player.Handle));

        if (target != null && target.IsValid && target.Entity != null && target.DesignerName.Contains("prop_physics_override")) return target;

        return null;
    }

    public static CBaseProp? GetClosestBlock(Vector start, Vector end, double threshold, CBaseProp excludeBlock)
    {
        CBaseProp? closestBlock = null;
        double closestDistance = double.MaxValue;

        foreach (var prop in Utilities.GetAllEntities().Where(e => e.DesignerName.Contains("prop_physics_override") && e.Entity!.Name.StartsWith("blockmaker")))
        {
            var currentProp = prop.As<CBaseProp>();

            if (currentProp == excludeBlock)
                continue;

            var pos = currentProp.CBodyComponent!.SceneNode!.AbsOrigin!;

            bool isOnLine = IsPointOnLine(start, end, pos, threshold);

            if (isOnLine)
            {
                double distance = CalculateDistance(start, pos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = currentProp;
                }
            }
        }

        return closestBlock;
    }

    public static float CalculateDistance(Vector a, Vector b)
    {
        return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
    }

    public static Vector AnglesToDirection(Vector angles)
    {
        float pitch = -angles.X * (float)(Math.PI / 180.0);
        float yaw = angles.Y * (float)(Math.PI / 180.0);

        float x = (float)(Math.Cos(pitch) * Math.Cos(yaw));
        float y = (float)(Math.Cos(pitch) * Math.Sin(yaw));
        float z = (float)Math.Sin(pitch);

        return new Vector(x, y, z);
    }

    public static Vector AddInFrontOf(Vector origin, QAngle angles, float units)
    {
        Vector direction = AnglesToDirection(new Vector(angles.X, angles.Y, angles.Z));
        return origin + (direction * units);
    }

    public static (Vector position, QAngle rotation) GetEndXYZ(CCSPlayerController player, CBaseProp block, double distance = 250, bool grid = false, float gridValue = 0f, bool snapping = false)
    {
        var pawn = player.Pawn()!;
        var playerpos = pawn.AbsOrigin!;

        Vector playerPos = new Vector(playerpos.X, playerpos.Y, playerpos.Z + pawn.CameraServices!.OldPlayerViewOffsetZ); 

        double angleA = -pawn.EyeAngles.X;
        double angleB = pawn.EyeAngles.Y;

        double radianA = (Math.PI / 180) * angleA;
        double radianB = (Math.PI / 180) * angleB;

        double x = playerPos.X + distance * Math.Cos(radianA) * Math.Cos(radianB);
        double y = playerPos.Y + distance * Math.Cos(radianA) * Math.Sin(radianB);
        double z = playerPos.Z + distance * Math.Sin(radianA);

        if (grid && gridValue != 0)
        {
            x = (float)Math.Round(x / gridValue) * gridValue;
            y = (float)Math.Round(y / gridValue) * gridValue;
            z = (float)Math.Round(z / gridValue) * gridValue;
        }

        float scale = 1;

        if (Blocks.Props.ContainsKey(block))
            scale = Utils.GetSize(Blocks.Props[block].Size);

        Vector endPos = new((float)x, (float)y, (float)z);
        QAngle endRotation = block.AbsRotation!;

        if (snapping)
        {
            var closestBlock = GetClosestBlock(playerPos, endPos, block.Collision.Maxs.X * scale * 2, block);
            if (closestBlock != null)
            {
                var snap = SnapToClosestBlock(endPos, closestBlock);

                var dist = CalculateDistance(closestBlock.AbsOrigin!, block.AbsOrigin!);

                if (dist < (closestBlock.Collision.Maxs.X * scale * 2) + 10)
                {
                    endPos = snap.Position;
                    endRotation = snap.Rotation;
                }
            }
        }

        return (endPos, endRotation);
    }

    public static (Vector Position, QAngle Rotation) SnapToClosestBlock(Vector endPos, CBaseProp block)
    {
        Vector position = new();
        QAngle rotation = new();

        Vector BlockPosition = block.AbsOrigin!;
        QAngle BlockRotation = block.AbsRotation!;

        float scale = 1;

        if (Blocks.Props.ContainsKey(block))
            scale = Utils.GetSize(Blocks.Props[block].Size);

        Vector Maxs = block.Collision.Maxs * scale * 2;

        System.Numerics.Matrix4x4 rotationMatrix = CalculateRotationMatrix(BlockRotation);

        Vector[] localFaceOffsets =
        {
            new Vector(-(Maxs.X / 2 + Maxs.X / 2), 0, 0), // -X face (left)
            new Vector(Maxs.X / 2 + Maxs.X / 2, 0, 0),   // +X face (right)
            new Vector(0, -(Maxs.Y / 2 + Maxs.Y / 2), 0), // -Y face (back)
            new Vector(0, Maxs.Y / 2 + Maxs.Y / 2, 0),   // +Y face (front)
            new Vector(0, 0, -(Maxs.Z / 2 + Maxs.Z / 2)), // -Z face (bottom)
            new Vector(0, 0, Maxs.Z / 2 + Maxs.Z / 2)    // +Z face (top)
        };

        float closestDistance = float.MaxValue;

        for (int i = 0; i < localFaceOffsets.Length; ++i)
        {
            Vector worldFaceOffset = TransformVector(localFaceOffsets[i], rotationMatrix);
            Vector testPos = BlockPosition + worldFaceOffset;

            float distance = CalculateDistance(endPos, testPos);

            if (distance < closestDistance)
            {
                closestDistance = distance;

                position = testPos;
                rotation = BlockRotation;

                if (BlockRotation.X == 90 || BlockRotation.X == -90)
                {
                    if (i == 0)
                        position.X = testPos.X + Maxs.X - Maxs.Z;
                    else if (i == 1)
                        position.X = testPos.X - Maxs.X + Maxs.Z;
                    else if (i == 4)
                        position.Y = testPos.Y - (BlockRotation.X == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                    else if (i == 5)
                        position.Y = testPos.Y + (BlockRotation.X == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                }
                else if (BlockRotation.Z == 90 || BlockRotation.Z == -90)
                {
                    if (i == 0)
                        position.Y = testPos.Y - (BlockRotation.Z == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                    else if (i == 1)
                        position.Y = testPos.Y + (BlockRotation.Z == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                    else if (i == 4)
                        position.Z = testPos.Z - Maxs.X + Maxs.Z;
                    else if (i == 5)
                        position.Z = testPos.Z + Maxs.X - Maxs.Z;
                }
                else if (BlockRotation.Y == 90 || BlockRotation.Y == -90)
                {
                    if (i == 0)
                        position.Z = testPos.Z + (BlockRotation.Y == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                    else if (i == 1)
                        position.Z = testPos.Z - (BlockRotation.Y == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                    else if (i == 4)
                        position.X = testPos.X + (BlockRotation.Y == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                    else if (i == 5)
                        position.X = testPos.X - (BlockRotation.Y == 90 ? Maxs.X - Maxs.Z : -(Maxs.X - Maxs.Z));
                }
            }
        }

        return (position, rotation);
    }

    public static System.Numerics.Matrix4x4 CalculateRotationMatrix(QAngle rotation)
    {
        double radX = (Math.PI / 180) * rotation.X;
        double radY = (Math.PI / 180) * rotation.Y;
        double radZ = (Math.PI / 180) * rotation.Z;

        System.Numerics.Matrix4x4 matrixX = System.Numerics.Matrix4x4.CreateRotationX((float)radX);
        System.Numerics.Matrix4x4 matrixY = System.Numerics.Matrix4x4.CreateRotationY((float)radY);
        System.Numerics.Matrix4x4 matrixZ = System.Numerics.Matrix4x4.CreateRotationZ((float)radZ);

        return matrixX * matrixY * matrixZ;
    }

    public static Vector TransformVector(Vector vector, System.Numerics.Matrix4x4 matrix)
    {
        return new Vector(
            vector.X * matrix.M11 + vector.Y * matrix.M12 + vector.Z * matrix.M13,
            vector.X * matrix.M21 + vector.Y * matrix.M22 + vector.Z * matrix.M23,
            vector.X * matrix.M31 + vector.Y * matrix.M32 + vector.Z * matrix.M33
        );
    }

    public static bool IsWithinBounds(Vector entityPosition, Vector playerPosition, Vector entitySize, Vector playerSize)
    {
        bool overlapX = Math.Abs(entityPosition.X - playerPosition.X) <= (entitySize.X + playerSize.X) / 2;
        bool overlapY = Math.Abs(entityPosition.Y - playerPosition.Y) <= (entitySize.Y + playerSize.Y) / 2;
        bool overlapZ = Math.Abs(entityPosition.Z - playerPosition.Z) <= (entitySize.Z + playerSize.Z) / 2;

        return overlapX && overlapY && overlapZ;
    }

    public class VectorDTO
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public VectorDTO() { }

        public VectorDTO(Vector vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        public Vector ToVector()
        {
            return new Vector(X, Y, Z);
        }
    }

    public class QAngleDTO
    {
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Roll { get; set; }

        public QAngleDTO() { }

        public QAngleDTO(QAngle qangle)
        {
            Pitch = qangle.X;
            Yaw = qangle.Y;
            Roll = qangle.Z;
        }

        public QAngle ToQAngle()
        {
            return new QAngle(Pitch, Yaw, Roll);
        }
    }

}