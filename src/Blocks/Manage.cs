using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

public partial class Blocks
{
    public static void Delete(CCSPlayerController player)
    {
        var entity = player.GetBlockAimTarget();

        if (entity != null)
        {
            if (UsedBlocks.TryGetValue(entity, out var block))
            {
                block.Entity.Remove();
                UsedBlocks.Remove(block.Entity);

                var trigger = BlockTriggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
                if (trigger != null)
                {
                    trigger.Remove();
                    BlockTriggers.Remove(trigger);
                }

                if (instance.Config.Sounds.Building.Enabled)
                    player.PlaySound(instance.Config.Sounds.Building.Delete);

                instance.PrintToChat(player, $"{ChatColors.Green}Deleted block");
            }
        }
        else instance.PrintToChat(player, "Delete Block: Could not find a block");
    }

    public static void Clear()
    {
        foreach (var rest in Utilities.GetAllEntities().Where(r => r.DesignerName == "prop_physics_override" || r.DesignerName == "trigger_multiple"))
        {
            if (rest == null || !rest.IsValid || rest.Entity == null)
                continue;

            if (!String.IsNullOrEmpty(rest.Entity.Name) && rest.Entity.Name.StartsWith("blockmaker"))
                rest.Remove();
        }

        UsedBlocks.Clear();
        BlockTriggers.Clear();
    }

    public static void Rotate(CCSPlayerController player, string rotation)
    {
        var block = player.GetBlockAimTarget();

        float selectedRotation = instance.playerData[player.Slot].RotationValue;

        if (block != null)
        {
            if (UsedBlocks.ContainsKey(block))
            {
                if (string.IsNullOrEmpty(rotation))
                {
                    instance.PrintToChat(player, $"Rotate Block: Option cannot be empty");
                    return;
                }

                if (string.Equals(rotation, "reset", StringComparison.OrdinalIgnoreCase))
                    block.Teleport(block.AbsOrigin, new QAngle(0, 0, 0));

                else if (string.Equals(rotation, "x-", StringComparison.OrdinalIgnoreCase))
                    block.Teleport(block.AbsOrigin, new QAngle(block.AbsRotation!.X - selectedRotation, block.AbsRotation.Y, block.AbsRotation.Z));
                else if (string.Equals(rotation, "x+", StringComparison.OrdinalIgnoreCase))
                    block.Teleport(block.AbsOrigin, new QAngle(block.AbsRotation!.X + selectedRotation, block.AbsRotation.Y, block.AbsRotation.Z));

                else if (string.Equals(rotation, "y-", StringComparison.OrdinalIgnoreCase))
                    block.Teleport(block.AbsOrigin, new QAngle(block.AbsRotation!.X, block.AbsRotation.Y - selectedRotation, block.AbsRotation.Z));
                else if (string.Equals(rotation, "y+", StringComparison.OrdinalIgnoreCase))
                    block.Teleport(block.AbsOrigin, new QAngle(block.AbsRotation!.X, block.AbsRotation.Y + selectedRotation, block.AbsRotation.Z));

                else if (string.Equals(rotation, "z-", StringComparison.OrdinalIgnoreCase))
                    block.Teleport(block.AbsOrigin, new QAngle(block.AbsRotation!.X, block.AbsRotation.Y, block.AbsRotation.Z - selectedRotation));
                else if (string.Equals(rotation, "z+", StringComparison.OrdinalIgnoreCase))
                    block.Teleport(block.AbsOrigin, new QAngle(block.AbsRotation!.X, block.AbsRotation.Y, block.AbsRotation.Z + selectedRotation));

                else
                {
                    instance.PrintToChat(player, $"Rotate Block: {ChatColors.White}'{rotation}' {ChatColors.Grey}is not a valid option");
                    return;
                }

                if (instance.Config.Sounds.Building.Enabled)
                    player.PlaySound(instance.Config.Sounds.Building.Rotate);

                instance.PrintToChat(player, $"Rotate Block: {ChatColors.White}{rotation} {(string.Equals(rotation, "reset", StringComparison.OrdinalIgnoreCase) ? $"" : $"by {selectedRotation} Units")}");
            }
        }
        else instance.PrintToChat(player, $"Rotate Block: Could not find a block");
    }

    public static void Convert(CCSPlayerController player)
    {
        var entity = player.GetBlockAimTarget();

        if (entity == null)
        {
            instance.PrintToChat(player, $"{ChatColors.Red}could not find a block to convert");
            return;
        }

        if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
            return;

        var playerData = instance.playerData[player.Slot];

        string blockmodel = instance.GetModelFromSelectedBlock(player, playerData.BlockSize);

        if (UsedBlocks.TryGetValue(entity, out var block))
        {
            Vector pos = new(block.Entity.AbsOrigin!.X, block.Entity.AbsOrigin.Y, block.Entity.AbsOrigin.Z);
            QAngle rotation = new(block.Entity.AbsRotation!.X, block.Entity.AbsRotation.Y, block.Entity.AbsRotation.Z);

            block.Entity.Remove();
            UsedBlocks.Remove(block.Entity);

            var trigger = BlockTriggers.FirstOrDefault(kvp => kvp.Value == block.Entity).Key;
            if (trigger != null)
            {
                trigger.Remove();
                BlockTriggers.Remove(trigger);
            }

            CreateBlock(playerData.BlockType, blockmodel, playerData.BlockSize, pos, rotation, playerData.BlockColor, playerData.BlockTransparency, playerData.BlockTeam);

            instance.PrintToChat(player, $"{ChatColors.Green}Converted block");
        }
        else instance.PrintToChat(player, $"Convert Block: {ChatColors.Red}Could not find the block");
    }
}
