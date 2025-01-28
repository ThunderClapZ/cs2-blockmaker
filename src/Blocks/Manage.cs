using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

public partial class Blocks
{
    public static void Unload()
    {
        foreach (var rest in Utilities.GetAllEntities().Where(r => r.DesignerName == "prop_physics_override" || r.DesignerName == "trigger_multiple"))
        {
            if (rest == null || !rest.IsValid || rest.Entity == null)
                continue;

            if (!String.IsNullOrEmpty(rest.Entity.Name) && rest.Entity.Name.StartsWith("blockmaker"))
                rest.Remove();
        }

        BlocksEntities.Clear();
        BlockTriggers.Clear();
    }

    public static void Delete(CCSPlayerController player)
    {
        var entity = player.GetBlockAimTarget();

        if (entity == null)
        {
            Utils.PrintToChat(player, $"{ChatColors.Red}Could not find a block to delete");
            return;
        }
   
        if (BlocksEntities.TryGetValue(entity, out var block))
        {
            block.Entity.Remove();
            BlocksEntities.Remove(block.Entity);

            var trigger = BlockTriggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
            if (trigger != null)
            {
                trigger.Remove();
                BlockTriggers.Remove(trigger);
            }

            if (config.Sounds.Building.Enabled)
                player.PlaySound(config.Sounds.Building.Delete);

            Utils.PrintToChat(player, $"Deleted -" +
                $" type: {ChatColors.White}{block.Name}{ChatColors.Grey}," +
                $" size: {ChatColors.White}{block.Size}{ChatColors.Grey}," +
                $" color: {ChatColors.White}{block.Color}{ChatColors.Grey}," +
                $" team: {ChatColors.White}{block.Team}{ChatColors.Grey}," +
                $" transparency: {ChatColors.White}{block.Transparency}");
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

        if (BlocksEntities.TryGetValue(entity, out var block))
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
                Utils.PrintToChat(player, $"{ChatColors.White}'{input}' {ChatColors.Red}is not a valid rotate option");
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

        if (BlocksEntities.TryGetValue(entity, out var block))
        {
            var AbsOrigin = block.Entity.AbsOrigin!;
            Vector pos = new(AbsOrigin.X, AbsOrigin.Y, AbsOrigin.Z);

            var AbsRotation = block.Entity.AbsRotation!;
            QAngle rotation = new(AbsRotation.X, AbsRotation.Y, AbsRotation.Z);

            block.Entity.Remove();
            BlocksEntities.Remove(block.Entity);

            var trigger = BlockTriggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
            if (trigger != null)
            {
                trigger.Remove();
                BlockTriggers.Remove(trigger);
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

        if (BlocksEntities.TryGetValue(entity, out var block))
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

        if (BlocksEntities.TryGetValue(entity, out var block))
        {
            var color = instance.playerData[player.Slot].BlockColor;

            var clr = Utils.GetColor(color);
            int alpha = Utils.GetAlpha(block.Transparency);
            entity.Render = Color.FromArgb(alpha, clr.R, clr.G, clr.B);
            Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");

            BlocksEntities[entity].Color = color;

            Utils.PrintToChat(player, $"Changed block color to {ChatColors.White}{color}");
        }
    }
}
