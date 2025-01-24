using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.InteropServices;

public static class VectorUtils
{
    private static bool IsPointOnLine(Vector start, Vector end, Vector point, double threshold)
    {
        // Calculate the direction vector of the line
        double lineDirectionX = end.X - start.X;
        double lineDirectionY = end.Y - start.Y;
        double lineDirectionZ = end.Z - start.Z;

        // Calculate the vector from start to the point
        double pointVectorX = point.X - start.X;
        double pointVectorY = point.Y - start.Y;
        double pointVectorZ = point.Z - start.Z;

        // Calculate the scalar projection of pointVector onto the lineDirection
        double scalarProjection = (pointVectorX * lineDirectionX + pointVectorY * lineDirectionY + pointVectorZ * lineDirectionZ) /
                                 (lineDirectionX * lineDirectionX + lineDirectionY * lineDirectionY + lineDirectionZ * lineDirectionZ);

        // Check if the scalar projection is within [0, 1], meaning the point is between start and end
        if (scalarProjection >= 0 && scalarProjection <= 1)
        {
            // Calculate the closest point on the line to the given point
            double closestPointX = start.X + scalarProjection * lineDirectionX;
            double closestPointY = start.Y + scalarProjection * lineDirectionY;
            double closestPointZ = start.Z + scalarProjection * lineDirectionZ;

            // Calculate the distance between the given point and the closest point on the line
            double distance = Math.Sqrt(Math.Pow(point.X - closestPointX, 2) + Math.Pow(point.Y - closestPointY, 2) + Math.Pow(point.Z - closestPointZ, 2));

            // Check if the distance is within the specified threshold
            return distance <= threshold;
        }

        // Point is not between start and end
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

    public static (Vector position, QAngle rotation) GetEndXYZ(CCSPlayerController player, CBaseProp block, double distance = 250, bool grid = false, float gridValue = 0f, bool snapping = false, float scale = 1)
    {
        Vector playerPos = new Vector(player.PlayerPawn.Value!.AbsOrigin!.X, player.PlayerPawn.Value.AbsOrigin.Y, player.PlayerPawn.Value.AbsOrigin.Z + player.PlayerPawn.Value!.CameraServices!.OldPlayerViewOffsetZ); 

        double angleA = -player.PlayerPawn.Value.EyeAngles.X;
        double angleB = player.PlayerPawn.Value.EyeAngles.Y;

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

        Vector endPos = new((float)x, (float)y, (float)z);
        QAngle endRotation = block.AbsRotation!;

        if (snapping)
        {
            var closestBlock = GetClosestBlock(playerPos, endPos, GetBlockSizeMax(block).X * scale, block);
            if (closestBlock != null)
            {
                var snap = SnapToClosestBlock(endPos, endRotation, closestBlock, scale);

                endPos = snap.Position;
                endRotation = snap.Rotation;
            }
        }

        return (endPos, endRotation);
    }

    public static (Vector Position, QAngle Rotation) SnapToClosestBlock(Vector position, QAngle rotation, CBaseProp block, float scale = 1)
    {
        Vector newBlockPosition = position;
        QAngle newBlockRotation = rotation;

        Vector BlockSizeMax = GetBlockSizeMax(block) * scale;

        Vector BlockPosition = block.AbsOrigin!;
        QAngle BlockRotation = block.AbsRotation!;

        Vector[] faceOffsets = new Vector[]
        {
            new Vector(-BlockSizeMax.X, 0, 0), // -X face
            new Vector(BlockSizeMax.X, 0, 0),  // +X face
            new Vector(0, -BlockSizeMax.Y, 0), // -Y face
            new Vector(0, BlockSizeMax.Y, 0),  // +Y face
            new Vector(0, 0, -BlockSizeMax.Z), // -Z face
            new Vector(0, 0, BlockSizeMax.Z)   // +Z face
        };

        float closestDistance = float.MaxValue;

        for (int i = 0; i < faceOffsets.Length; ++i)
        {
            Vector testPos = BlockPosition + faceOffsets[i];

            float distance = CalculateDistance(position, testPos);

            if (distance < closestDistance)
            {
                closestDistance = distance;

                newBlockPosition = testPos;

                newBlockRotation = BlockRotation;

                if (BlockRotation.Z != 0)
                {
                    if (i == 4)
                        newBlockPosition.Z = testPos.Z + BlockSizeMax.X;

                    if (i == 5)
                        newBlockPosition.Z = testPos.Z - BlockSizeMax.X;
                }
            }
        }

        return (newBlockPosition, newBlockRotation);
    }

    public static bool IsWithinBounds(Vector entityPos, Vector blockPos, Vector mins, Vector maxs)
    {
        return entityPos.X >= blockPos.X + mins.X && entityPos.X <= blockPos.X + maxs.X &&
              entityPos.Y >= blockPos.Y + mins.Y && entityPos.Y <= blockPos.Y + maxs.Y &&
              entityPos.Z >= blockPos.Z + mins.Z && entityPos.Z <= blockPos.Z + maxs.Z;
    }

    public static Vector GetBlockSizeMax(CBaseProp block)
    {
        return new Vector(block.Collision.Maxs.X, block.Collision.Maxs.Y, block.Collision.Maxs.Z) * 2;
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