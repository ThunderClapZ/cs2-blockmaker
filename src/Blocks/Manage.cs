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
            if (rest == null || !rest.IsValid || rest.Entity == null)
                continue;

            if (!String.IsNullOrEmpty(rest.Entity.Name) && rest.Entity.Name.StartsWith("blockmaker"))
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
                    player.PlaySound(config.Sounds.Building.Delete);

                Utils.PrintToChat(player, $"Deleted -" +
                    $" type: {ChatColors.White}{block.Name}{ChatColors.Grey}," +
                    $" size: {ChatColors.White}{block.Size}{ChatColors.Grey}," +
                    $" color: {ChatColors.White}{block.Color}{ChatColors.Grey}," +
                    $" team: {ChatColors.White}{block.Team}{ChatColors.Grey}," +
                    $" transparency: {ChatColors.White}{block.Transparency}");

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
                    player.PlaySound(config.Sounds.Building.Delete);

                Utils.PrintToChat(player, $"Deleted teleport pair");
            }
        }
    }

    public static void Rotate(CCSPlayerController player, string input)
    {
        var entity = player.GetBlockAimTarget();

        float selectedRotation = instance.playerData[player.Slot].RotationValue;

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to rotate");
            return;
        }

        if (Props.TryGetValue(entity, out var block))
        {
            if (string.IsNullOrEmpty(input))
            {
                Utils.PrintToChat(player, $"{ChatColors.Red}Rotate option cannot be empty");
                return;
            }

            var AbsOrigin = block.Entity.AbsOrigin!;
            Vector pos = new(AbsOrigin.X, AbsOrigin.Y, AbsOrigin.Z);

            var AbsRotation = block.Entity.AbsRotation!;
            QAngle rotation = new(AbsRotation.X, AbsRotation.Y, AbsRotation.Z);

            if (string.Equals(input, "reset", StringComparison.OrdinalIgnoreCase))
                rotation = new QAngle(0, 0, 0);

            else if (string.Equals(input, "x-", StringComparison.OrdinalIgnoreCase))
                rotation.X -= selectedRotation;
            else if (string.Equals(input, "x+", StringComparison.OrdinalIgnoreCase))
                rotation.X += selectedRotation;

            else if (string.Equals(input, "y-", StringComparison.OrdinalIgnoreCase))
               rotation.Y -= selectedRotation;
            else if (string.Equals(input, "y+", StringComparison.OrdinalIgnoreCase))
               rotation.Y += selectedRotation;

            else if (string.Equals(input, "z-", StringComparison.OrdinalIgnoreCase))
                rotation.Z -= selectedRotation;
            else if (string.Equals(input, "z+", StringComparison.OrdinalIgnoreCase))
                rotation.Z += selectedRotation;

            else
            {
                Utils.PrintToChat(player, $"{ChatColors.White}{input} {ChatColors.Red}is not a valid rotate option");
                return;
            }

            block.Entity.Teleport(null, rotation);

            if (config.Sounds.Building.Enabled)
                player.PlaySound(config.Sounds.Building.Rotate);

            Utils.PrintToChat(player, $"Rotated Block: {ChatColors.White}{input} {(string.Equals(input, "reset", StringComparison.OrdinalIgnoreCase) ? $"" : $"by {selectedRotation} Units")}");
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

        var playerData = instance.playerData[player.Slot];

        string blockmodel = Utils.GetModelFromSelectedBlock(player, playerData.Pole);

        if (Props.TryGetValue(entity, out var block))
        {
            var AbsOrigin = block.Entity.AbsOrigin!;
            Vector pos = new(AbsOrigin.X, AbsOrigin.Y, AbsOrigin.Z);

            var AbsRotation = block.Entity.AbsRotation!;
            QAngle rotation = new(AbsRotation.X, AbsRotation.Y, AbsRotation.Z);

            block.Entity.Remove();
            Props.Remove(block.Entity);

            var trigger = Triggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
            if (trigger != null)
            {
                trigger.Remove();
                Triggers.Remove(trigger);
            }

            CreateBlock(playerData.BlockType, blockmodel, playerData.BlockSize, pos, rotation, playerData.BlockColor, playerData.BlockTransparency, playerData.BlockTeam);

            Utils.PrintToChat(player, $"Converted -" +
                $" type: {ChatColors.White}{playerData.BlockType}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{playerData.BlockSize}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{playerData.BlockColor}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{playerData.BlockTeam}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{playerData.BlockTransparency}");
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

            playerData.BlockColor = block.Color;
            playerData.BlockSize = block.Size;
            playerData.BlockType = block.Name;
            playerData.BlockTeam = block.Team;
            playerData.BlockTransparency = block.Transparency;

            Utils.PrintToChat(player, $"Copied -" +
                $" type: {ChatColors.White}{playerData.BlockType}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{playerData.BlockSize}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{playerData.BlockColor}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{playerData.BlockTeam}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{playerData.BlockTransparency}");
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
            playerData.PropertyType = "";
            playerData.PropertyEntity.Remove(type);
            Utils.PrintToChat(player, $"{ChatColors.Red}No entity found for {type}");
            return;
        }

        if ((!float.TryParse(input, out float number) || number <= 0) && input != "Reset" && input != "OnTop")
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Invalid {type} input value");
            return;
        }

        var blockname = Props[entity].Name;
        var defaultProperties = Files.PropsData.Properties.BlockProperties;

        switch (type)
        {
            case "Reset":
                Props[entity].Properties.Cooldown = defaultProperties[blockname].Cooldown;
                Props[entity].Properties.Value = defaultProperties[blockname].Value;
                Props[entity].Properties.Duration = defaultProperties[blockname].Duration;
                Utils.PrintToChat(player, $"{ChatColors.White}{blockname} {ChatColors.Grey}properties has been reset");
                break;
            case "OnTop":
                Props[entity].Properties.OnTop = Props[entity].Properties.OnTop ? false : true;
                Utils.PrintToChat(player, $"Changed {ChatColors.White}{blockname} {ChatColors.Grey}{type} to {ChatColors.White}{Props[entity].Properties.OnTop}{ChatColors.Grey}");
                break;
            case "Duration":
                Props[entity].Properties.Duration = number;
                break;
            case "Value":
                Props[entity].Properties.Value = number;
                break;
            case "Cooldown":
                Props[entity].Properties.Cooldown = number;
                break;
            default:
                Utils.PrintToChat(player, $"{ChatColors.Red}Unknown property type: {type}");
                return;
        }

        playerData.PropertyType = "";
        playerData.PropertyEntity.Remove(type);

        if (input != "Reset" && input != "OnTop")
            Utils.PrintToChat(player, $"Changed {ChatColors.White}{blockname} {ChatColors.Grey}{type} to {ChatColors.White}{input}{ChatColors.Grey}");
    }
}
