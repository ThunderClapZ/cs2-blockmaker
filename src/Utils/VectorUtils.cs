using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.InteropServices;

public static class VectorUtils
{
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

    public static CBaseProp? GetClosestBlock(Vector endPos, CBaseProp excludeBlock, double threshold)
    {
        CBaseProp? closestBlock = null;

        foreach (var prop in Utilities.GetAllEntities().Where(e => e.DesignerName.Contains("prop_physics_override") && e.Entity!.Name.StartsWith("blockmaker")))
        {
            var currentProp = prop.As<CBaseProp>();

            if (currentProp == excludeBlock)
                continue;

            double distance = CalculateDistance(endPos, currentProp.AbsOrigin!);
            if (distance < threshold)
                closestBlock = currentProp;
        }

        return closestBlock;
    }

    public static (Vector position, QAngle rotation) GetEndXYZ(CCSPlayerController player, CBaseProp block, double distance = 250, bool grid = false, float gridValue = 0f, bool snapping = false, float snapValue = 0f)
    {
        if (Blocks.Props.TryGetValue(Blocks.PlayerHolds[player].block, out var locked))
        {
            if (Blocks.Props[locked.Entity].Properties.Locked)
            {
                if (Blocks.PlayerHolds[player].lockedmessage == false)
                    Utils.PrintToChat(player, $"{ChatColors.Red}Block is locked");

                Blocks.PlayerHolds[player].lockedmessage = true;

                return (block.AbsOrigin!, block.AbsRotation!);
            }
        }

        var pawn = player.Pawn()!;
        var playerpos = pawn.AbsOrigin!;

        Vector aim = new Vector(playerpos.X, playerpos.Y, playerpos.Z + pawn.CameraServices!.OldPlayerViewOffsetZ); 

        double angleA = -pawn.EyeAngles.X;
        double angleB = pawn.EyeAngles.Y;
        double radianA = (Math.PI / 180) * angleA;
        double radianB = (Math.PI / 180) * angleB;
        double x = aim.X + distance * Math.Cos(radianA) * Math.Cos(radianB);
        double y = aim.Y + distance * Math.Cos(radianA) * Math.Sin(radianB);
        double z = aim.Z + distance * Math.Sin(radianA);

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
            float scale = Blocks.Props.ContainsKey(block) ? Utils.GetSize(Blocks.Props[block].Size) : 1;

            var closestBlock = GetClosestBlock(endPos, block, scale * block.Collision.Maxs.X * 2);
            if (closestBlock != null)
            {
                var snap = SnapToClosestBlock(block, closestBlock, snapValue, endPos);

                endPos = snap.Position;
                endRotation = snap.Rotation;
            }
        }

        return (endPos, endRotation);
    }

    public static (Vector Position, QAngle Rotation) SnapToClosestBlock(CBaseProp block, CBaseProp closestBlock, float snapValue, Vector playerEyePos)
    {
        Vector position = block.AbsOrigin!;
        QAngle rotation = closestBlock.AbsRotation!;

        float scale = Blocks.Props.ContainsKey(closestBlock) ? Utils.GetSize(Blocks.Props[closestBlock].Size) : 1;

        Vector mins = closestBlock.Collision.Mins * scale * 2;
        Vector maxs = closestBlock.Collision.Maxs * scale * 2;

        // Get the forward, right, and up vectors based on the closest block's rotation
        Vector forward = new Vector(
            (float)Math.Cos(rotation.Y * Math.PI / 180) * (float)Math.Cos(rotation.X * Math.PI / 180),
            (float)Math.Sin(rotation.Y * Math.PI / 180) * (float)Math.Cos(rotation.X * Math.PI / 180),
            (float)-Math.Sin(rotation.X * Math.PI / 180)
        );
        Vector right = new Vector(
            (float)Math.Cos((rotation.Y + 90) * Math.PI / 180),
            (float)Math.Sin((rotation.Y + 90) * Math.PI / 180),
            0
        );
        Vector up = Cross(forward, right);

        Vector[] localFaceCenters =
        {
            new Vector(mins.X, 0, 0),   // -X face
            new Vector(maxs.X, 0, 0),   // +X face
            new Vector(0, mins.Y, 0),   // -Y face
            new Vector(0, maxs.Y, 0),   // +Y face
            new Vector(0, 0, mins.Z),   // -Z face
            new Vector(0, 0, maxs.Z)    // +Z face
        };

        // Transform the face centers to world space using the rotation
        Vector[] faceCenters = new Vector[6];
        for (int i = 0; i < localFaceCenters.Length; i++)
        {
            Vector localCenter = localFaceCenters[i];
            faceCenters[i] =
                closestBlock.AbsOrigin! +
                forward * localCenter.X +
                right * localCenter.Y +
                up * localCenter.Z;
        }

        float closestDistance = float.MaxValue;
        int closestFace = -1;

        for (int i = 0; i < faceCenters.Length; i++)
        {
            float distance = CalculateDistance(playerEyePos, faceCenters[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFace = i;
                position = faceCenters[i];
            }
        }

        if (closestFace != -1)
        {
            Vector snapDirection = Vector.Zero;

            switch (closestFace)
            {
                case 0: snapDirection = -forward; break;// -X face
                case 1: snapDirection = forward; break; // +X face
                case 2: snapDirection = -right; break;  // -Y face
                case 3: snapDirection = right; break;   // +Y face
                case 4: snapDirection = -up; break;     // -Z face
                case 5: snapDirection = up; break;      // +Z face
            }

            position += snapDirection * snapValue;
        }

        return (position, rotation);
    }

    public static Vector Cross(Vector a, Vector b)
    {
        return new Vector(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public static float CalculateDistance(Vector a, Vector b)
    {
        return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
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