using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;
using System.Drawing;

public partial class Blocks
{
    public static void Position(CCSPlayerController player, string input, bool rotate)
    {
        var entity = player.GetBlockAim();

        float value = rotate ? instance.BuilderData[player.Slot].RotationValue : instance.BuilderData[player.Slot].PositionValue;

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to modify position");
            return;
        }

        if (Entities.TryGetValue(entity, out var block))
        {
            if (Utils.BlockLocked(player, block))
                return;

            if (string.IsNullOrEmpty(input))
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Input option cannot be empty");
                return;
            }

            var Rotation = block.Entity.AbsRotation!;
            var Position = block.Entity.AbsOrigin!;

            QAngle_t rot = new(Rotation.X, Rotation.Y, Rotation.Z);
            Vector_t pos = new(Position.X, Position.Y, Position.Z);

            if (string.Equals(input, "x-", StringComparison.OrdinalIgnoreCase))
            {
                if (rotate) rot.X -= value;
                else pos.X -= value;
            }
            else if (string.Equals(input, "x+", StringComparison.OrdinalIgnoreCase))
            {
                if (rotate) rot.X += value;
                else pos.X += value;
            }
            else if (string.Equals(input, "y-", StringComparison.OrdinalIgnoreCase))
            {
                if (rotate) rot.Y -= value;
                else pos.Y -= value;
            }
            else if (string.Equals(input, "y+", StringComparison.OrdinalIgnoreCase))
            {
                if (rotate) rot.Y += value;
                else pos.Y += value;
            }
            else if (string.Equals(input, "z-", StringComparison.OrdinalIgnoreCase))
            {
                if (rotate) rot.Z -= value;
                else pos.Z -= value;
            }
            else if (string.Equals(input, "z+", StringComparison.OrdinalIgnoreCase))
            {
                if (rotate) rot.Z += value;
                else pos.Z += value;
            }
            else if (string.Equals(input, "reset", StringComparison.OrdinalIgnoreCase))
            {
                rot = new QAngle_t();
            }

            else
            {
                Utils.PrintToChat(player, $"{ChatColors.White}{input} {ChatColors.Red}is not a valid option");
                return;
            }

            if (rotate) block.Entity.Teleport(null, rot);
            else block.Entity.Teleport(pos);

            if (config.Sounds.Building.Enabled)
                player.EmitSound(config.Sounds.Building.Rotate);

            string text = $"{ChatColors.White}{input} {(string.Equals(input, "reset", StringComparison.OrdinalIgnoreCase) ? $"" : $"by {value} Units")}";

            if (rotate) Utils.PrintToChat(player, $"Rotated Block: {text}");
            else Utils.PrintToChat(player, $"Moved Block: {text}");
        }
    }

    public static void Convert(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to convert");
            return;
        }

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Entities.TryGetValue(entity, out var block))
        {
            if (Utils.BlockLocked(player, block))
                return;

            block.Entity.Remove();
            Entities.Remove(block.Entity);

            var trigger = Triggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
            if (trigger != null)
            {
                trigger.Remove();
                Triggers.Remove(trigger);
            }

            var BuilderData = instance.BuilderData[player.Slot];

            CreateBlock(player, BuilderData.BlockType, BuilderData.BlockPole, BuilderData.BlockSize, entity.AbsOrigin!.ToVector_t(), entity.AbsRotation!.ToQAngle_t(), BuilderData.BlockColor, BuilderData.BlockTransparency, BuilderData.BlockTeam, BuilderData.BlockEffect?.Particle ?? "");

            Utils.PrintToChat(player, $"Converted -" +
                $" type: {ChatColors.White}{BuilderData.BlockType}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{BuilderData.BlockSize}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{BuilderData.BlockColor}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{BuilderData.BlockTeam}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{BuilderData.BlockTransparency},"
            );
        }
    }

    public static void Copy(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}无板块可以复制");
            return;
        }

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Entities.TryGetValue(entity, out var block))
        {
            if (Utils.BlockLocked(player, block))
                return;
            
            // 复制板块
            if (Utils.GetMoney(player) < 500) return;
            if (block.Type.Contains("平台"))
            {
                Utils.PrintToChat(player, "复制成功");
                Utils.ReduceMoney(player, 500);
            }
            else
            {
                Utils.PrintToChat(player, "特殊板块无法复制");
                return;
            }

            var BuilderData = instance.BuilderData[player.Slot];

            CreateBlock(null, block.Type, block.Pole, block.Size, entity.AbsOrigin!.ToVector_t(), entity.AbsRotation!.ToQAngle_t(), block.Color, block.Transparency, block.Team, block.Effect, block.Properties);

            if (config.Sounds.Building.Enabled)
                player.EmitSound(config.Sounds.Building.Create);
        }
    }

    public static void Lock(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to lock");
            return;
        }

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Entities.TryGetValue(entity, out var block))
        {
            block.Properties.Locked = !block.Properties.Locked;

            Utils.PrintToChat(player, $"{(block.Properties.Locked ? "Locked" : "Unlocked")} -" +
                $" type: {ChatColors.White}{block.Type}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{block.Size}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{block.Color}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{block.Team}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{block.Transparency}"
            );
        }
    }

    private static bool lockedAll;
    public static void LockAll(CCSPlayerController player)
    {
        if (lockedAll)
        {
            foreach (var block in Entities.Values)
                block.Properties.Locked = false;

            Utils.PrintToChat(player, "Unlocked all blocks");

            lockedAll = false;
        }
        else
        {
            foreach (var block in Entities.Values)
                block.Properties.Locked = true;

            Utils.PrintToChat(player, "Locked all blocks");

            lockedAll = true;
        }
    }

    public static void RenderColor(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();

        if (entity == null)
            return;

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Entities.TryGetValue(entity, out var block))
        {
            if (Utils.BlockLocked(player, block))
                return;

            var color = instance.BuilderData[player.Slot].BlockColor;

            var clr = Utils.GetColor(color);
            int alpha = Utils.GetAlpha(block.Transparency);
            entity.Render = Color.FromArgb(alpha, clr.R, clr.G, clr.B);
            Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");

            Entities[entity].Color = color;

            Utils.PrintToChat(player, $"Changed block color to {ChatColors.White}{color}");
        }
    }

    public static void ChangeProperties(CCSPlayerController player, string type, string input)
    {
        var BuilderData = instance.BuilderData[player.Slot];

        if (!BuilderData.PropertyEntity.TryGetValue(type, out var entity) || entity == null)
        {
            BuilderData.ChatInput = "";
            BuilderData.PropertyEntity.Clear();
            Utils.PrintToChat(player, $"{ChatColors.Red}No entity found for {type}");
            return;
        }

        if ((!float.TryParse(input, out float number) || number <= 0) && input != "Reset" && input != "OnTop" && input != "Locked")
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Invalid input value: {ChatColors.White}{input}");
            return;
        }

        if (Entities.TryGetValue(entity, out var block))
        {
            var properties = block.Properties;
            var blocktype = block.Type;

            switch (type)
            {
                case "Reset":
                    var defaultProperties = Properties.BlockProperties[blocktype.Split('.')[0]];
                    block.Properties = new Property
                    {
                        Cooldown = defaultProperties.Cooldown,
                        Value = defaultProperties.Value,
                        Duration = defaultProperties.Duration,
                        OnTop = defaultProperties.OnTop,
                        Locked = defaultProperties.Locked,
                        Builder = properties.Builder,
                    };
                    Utils.PrintToChat(player, $"{ChatColors.White}{blocktype} {ChatColors.Grey}properties has been reset");
                    break;
                case "OnTop":
                    properties.OnTop = !properties.OnTop;
                    Utils.PrintToChat(player, $"Changed {ChatColors.White}{blocktype} {ChatColors.Grey}{type} to {ChatColors.White}{(properties.OnTop ? "Enabled" : "Disabled")}{ChatColors.Grey}");
                    break;
                case "Locked":
                    properties.Locked = !properties.Locked;
                    Utils.PrintToChat(player, $"Changed {ChatColors.White}{blocktype} {ChatColors.Grey}{type} to {ChatColors.White}{(properties.Locked ? "Enabled" : "Disabled")}{ChatColors.Grey}");
                    break;
                case "Duration":
                    properties.Duration = number;
                    Utils.PrintToChat(player, $"Changed {ChatColors.White}{blocktype} {ChatColors.Grey}{type} to {ChatColors.White}{input}{ChatColors.Grey}");
                    break;
                case "Value":
                    properties.Value = number;
                    Utils.PrintToChat(player, $"Changed {ChatColors.White}{blocktype} {ChatColors.Grey}{type} to {ChatColors.White}{input}{ChatColors.Grey}");
                    break;
                case "Cooldown":
                    properties.Cooldown = number;
                    Utils.PrintToChat(player, $"Changed {ChatColors.White}{blocktype} {ChatColors.Grey}{type} to {ChatColors.White}{input}{ChatColors.Grey}");
                    break;
                default:
                    Utils.PrintToChat(player, $"{ChatColors.Red}Unknown property type: {type}");
                    break;
            }
        }

        BuilderData.ChatInput = "";
        BuilderData.PropertyEntity.Remove(type);
    }

    public static void Transparency(CCSPlayerController player, string value)
    {
        var entity = player.GetBlockAim();

        if (entity == null || entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Entities.TryGetValue(entity, out var block))
        {
            if (Utils.BlockLocked(player, block))
                return;

            Entities[entity].Transparency = value;

            var color = Utils.GetColor(block.Color);
            int alpha = Utils.GetAlpha(value);
            entity.Render = Color.FromArgb(alpha, color.R, color.G, color.B);
            Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");

            Utils.PrintToChat(player, $"Changed block transparency to {ChatColors.White}{value}");
        }
    }

    public static void ChangeEffect(CCSPlayerController player)
    {
        var entity = player.GetBlockAim();

        if (entity == null || entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Entities.TryGetValue(entity, out var block))
        {
            if (Utils.BlockLocked(player, block))
                return;

            var BuilderData = instance.BuilderData[player.Slot];
            Effect effect = BuilderData.BlockEffect;
            Entities[entity].Effect = effect.Particle;

            block.Entity.Remove();
            Entities.Remove(block.Entity);

            var trigger = Triggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
            if (trigger != null)
            {
                trigger.Remove();
                Triggers.Remove(trigger);
            }

            CreateBlock(player, block.Type, block.Pole, block.Size, entity.AbsOrigin!.ToVector_t(), entity.AbsRotation!.ToQAngle_t(), block.Color, block.Transparency, block.Team, BuilderData.BlockEffect?.Particle ?? "");

            Utils.PrintToChat(player, $"Changed block effect to {ChatColors.White}{effect.Title}");
        }
    }
}
