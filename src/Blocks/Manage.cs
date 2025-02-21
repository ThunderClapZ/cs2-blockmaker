using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

public partial class Blocks
{
    public static void Clear()
    {
        foreach (var rest in Utilities.GetAllEntities().Where(r => r.DesignerName == "prop_physics_override" || r.DesignerName == "trigger_multiple"))
        {
            if (!String.IsNullOrEmpty(rest.Entity!.Name) && rest.Entity.Name.StartsWith("blockmaker"))
                rest.Remove();
        }

        foreach (var timer in Plugin.Instance.Timers)
            timer.Kill();

        Props.Clear();
        Triggers.Clear();

        Teleports.Clear();
        isNextTeleport.Clear();

        PlayerCooldowns.Clear();
        CooldownsTimers.Clear();
        TempTimers.Clear();

        PlayerHolds.Clear();

        nuked = false;
    }

    public static void Delete(CCSPlayerController player, bool all = false)
    {
        if (all)
        {
            foreach (var rest in Utilities.GetAllEntities().Where(r => r.DesignerName == "prop_physics_override" || r.DesignerName == "trigger_multiple"))
            {
                if (rest == null || !rest.IsValid || rest.Entity == null)
                    continue;

                if (!String.IsNullOrEmpty(rest.Entity.Name) && rest.Entity.Name.StartsWith("blockmaker"))
                    rest.Remove();
            }

            Props.Clear();
            Triggers.Clear();

            Teleports.Clear();
            isNextTeleport.Clear();
        }
        else
        {
            var entity = player.GetBlockAimTarget();

            if (entity == null)
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to delete");
                return;
            }

            if (Props.TryGetValue(entity, out var block))
            {
                block.Entity.Remove();
                Props.Remove(block.Entity);

                var trigger = Triggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
                if (trigger != null)
                {
                    trigger.Remove();
                    Triggers.Remove(trigger);
                }

                if (config.Sounds.Building.Enabled)
                {
                    var sound = config.Sounds.Building.Delete;
                    player.PlaySound(sound.Event, sound.Volume);
                }

                Utils.PrintToChat(player, $"Deleted -" +
                    $" type: {ChatColors.White}{block.Type}{ChatColors.Grey}," +
                    $" size: {ChatColors.White}{block.Size}{ChatColors.Grey}," +
                    $" color: {ChatColors.White}{block.Color}{ChatColors.Grey}," +
                    $" team: {ChatColors.White}{block.Team}{ChatColors.Grey}," +
                    $" transparency: {ChatColors.White}{block.Transparency}"
                );

                return;
            }

            var teleports = Teleports.FirstOrDefault(pair => pair.Entry.Entity == entity || pair.Exit.Entity == entity);

            if (teleports != null)
            {
                var entryEntity = teleports.Entry.Entity;
                if (entryEntity != null && entryEntity.IsValid)
                {
                    entryEntity.Remove();

                    var entryTrigger = Triggers.FirstOrDefault(kvp => kvp.Value == entryEntity).Key;
                    if (entryTrigger != null)
                    {
                        entryTrigger.Remove();
                        Triggers.Remove(entryTrigger);
                    }
                }

                var exitEntity = teleports.Exit.Entity;
                if (exitEntity != null && exitEntity.IsValid)
                {
                    exitEntity.Remove();

                    var exitTrigger = Triggers.FirstOrDefault(kvp => kvp.Value == exitEntity).Key;
                    if (exitTrigger != null)
                    {
                        exitTrigger.Remove();
                        Triggers.Remove(exitTrigger);
                    }
                }

                Teleports.Remove(teleports);

                if (config.Sounds.Building.Enabled)
                {
                    var sound = config.Sounds.Building.Delete;
                    player.PlaySound(sound.Event, sound.Volume);
                }

                Utils.PrintToChat(player, $"Deleted teleport pair");
            }
        }
    }

    public static void Position(CCSPlayerController player, string input, bool rotate)
    {
        var entity = player.GetBlockAimTarget();

        float value = rotate ? instance.playerData[player.Slot].RotationValue : instance.playerData[player.Slot].PositionValue;

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to modify position");
            return;
        }

        if (Props.TryGetValue(entity, out var block))
        {
            if (string.IsNullOrEmpty(input))
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Input option cannot be empty");
                return;
            }

            var AbsRotation = block.Entity.AbsRotation!;
            var AbsOrigin = block.Entity.AbsOrigin!;

            QAngle rot = new QAngle(AbsRotation.X, AbsRotation.Y, AbsRotation.Z);
            Vector pos = new Vector(AbsOrigin.X, AbsOrigin.Y, AbsOrigin.Z);

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
                rot = new QAngle();
            }

            else
            {
                Utils.PrintToChat(player, $"{ChatColors.White}{input} {ChatColors.Red}is not a valid option");
                return;
            }

            if (rotate) block.Entity.Teleport(null, rot);
            else block.Entity.Teleport(pos);

            if (config.Sounds.Building.Enabled)
            {
                var sound = config.Sounds.Building.Rotate;
                player.PlaySound(sound.Event, sound.Volume);
            }

            string text = $"{ChatColors.White}{input} {(string.Equals(input, "reset", StringComparison.OrdinalIgnoreCase) ? $"" : $"by {value} Units")}";

            if (rotate) Utils.PrintToChat(player, $"Rotated Block: {text}");
            else Utils.PrintToChat(player, $"Moved Block: {text}");
        }
    }

    public static void Convert(CCSPlayerController player)
    {
        var entity = player.GetBlockAimTarget();

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to convert");
            return;
        }

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Props.TryGetValue(entity, out var block))
        {
            block.Entity.Remove();
            Props.Remove(block.Entity);

            var trigger = Triggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
            if (trigger != null)
            {
                trigger.Remove();
                Triggers.Remove(trigger);
            }

            var playerData = instance.playerData[player.Slot];

            CreateBlock(playerData.BlockType, playerData.BlockPole, playerData.BlockSize, entity.AbsOrigin!, entity.AbsRotation!, playerData.BlockColor, playerData.BlockTransparency, playerData.BlockTeam);

            Utils.PrintToChat(player, $"Converted -" +
                $" type: {ChatColors.White}{playerData.BlockType}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{playerData.BlockSize}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{playerData.BlockColor}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{playerData.BlockTeam}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{playerData.BlockTransparency}"
            );
        }
    }

    public static void Copy(CCSPlayerController player)
    {
        var entity = player.GetBlockAimTarget();

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to copy");
            return;
        }

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Props.TryGetValue(entity, out var block))
        {
            var playerData = instance.playerData[player.Slot];

            CreateBlock(block.Type, block.Pole, block.Size, entity.AbsOrigin!, entity.AbsRotation!, block.Color, block.Transparency, block.Team, block.Properties);

            if (config.Sounds.Building.Enabled)
            {
                var sound = config.Sounds.Building.Create;
                player.PlaySound(sound.Event, sound.Volume);
            }

            Utils.PrintToChat(player, $"Copied -" +
                $" type: {ChatColors.White}{playerData.BlockType}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{playerData.BlockSize}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{playerData.BlockColor}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{playerData.BlockTeam}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{playerData.BlockTransparency}"
            );
        }
    }

    public static void Lock(CCSPlayerController player)
    {
        var entity = player.GetBlockAimTarget();

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to lock");
            return;
        }

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Props.TryGetValue(entity, out var block))
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

    public static void RenderColor(CCSPlayerController player)
    {
        var entity = player.GetBlockAimTarget();

        if (entity == null)
            return;

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        if (Props.TryGetValue(entity, out var block))
        {
            var color = instance.playerData[player.Slot].BlockColor;

            var clr = Utils.GetColor(color);
            int alpha = Utils.GetAlpha(block.Transparency);
            entity.Render = Color.FromArgb(alpha, clr.R, clr.G, clr.B);
            Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");

            Props[entity].Color = color;

            Utils.PrintToChat(player, $"Changed block color to {ChatColors.White}{color}");
        }
    }

    public static void ChangeProperties(CCSPlayerController player, string type, string input)
    {
        var playerData = instance.playerData[player.Slot];

        if (!playerData.PropertyEntity.TryGetValue(type, out var entity) || entity == null)
        {
            playerData.ChatInput = "";
            playerData.PropertyEntity.Clear();
            Utils.PrintToChat(player, $"{ChatColors.Red}No entity found for {type}");
            return;
        }

        if ((!float.TryParse(input, out float number) || number <= 0) && input != "Reset" && input != "OnTop")
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Invalid input value: {ChatColors.White}{input}");
            return;
        }

        if (Props.TryGetValue(entity, out var block))
        {
            var properties = block.Properties;
            var blocktype = block.Type;

            switch (type)
            {
                case "Reset":
                    var defaultProperties = Files.PropsData.Properties.BlockProperties[blocktype.Split('.')[0]];
                    block.Properties = new BlockData_Properties
                    {
                        Cooldown = defaultProperties.Cooldown,
                        Value = defaultProperties.Value,
                        Duration = defaultProperties.Duration,
                        OnTop = defaultProperties.OnTop,
                    };
                    Utils.PrintToChat(player, $"{ChatColors.White}{blocktype} {ChatColors.Grey}properties has been reset");
                    break;
                case "OnTop":
                    properties.OnTop = !properties.OnTop;
                    Utils.PrintToChat(player, $"Changed {ChatColors.White}{blocktype} {ChatColors.Grey}{type} to {ChatColors.White}{(properties.OnTop ? "Enabled" : "Disabled")}{ChatColors.Grey}");
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
                    return;
            }
        }

        playerData.ChatInput = "";
        playerData.PropertyEntity.Remove(type);
    }
}
