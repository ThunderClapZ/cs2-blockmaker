using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text.Json;

public partial class Plugin
{
    public bool BuildMode(CCSPlayerController player)
    {
        if (buildMode && playerData[player.Slot].Builder)
            return true;
        else if (!buildMode && playerData[player.Slot].Builder)
        {
            PrintToChat(player, $"{ChatColors.Red}Build Mode is disabled");
            return false;
        }
        else
        {
            PrintToChat(player, $"{ChatColors.Red}You don't have access to Build Mode");
            return false;
        }
    }

    public bool HasPermission(CCSPlayerController player)
    {
        return string.IsNullOrEmpty(Config.Commands.Admin.Permission) || AdminManager.PlayerHasPermissions(player, Config.Commands.Admin.Permission);
    }

    public void PrintToChat(CCSPlayerController player, string message)
    {
        player.PrintToChat($"{Config.Settings.Main.Prefix} {ChatColors.Grey}{message}");
    }

    public void PrintToChatAll(string message)
    {
        Server.PrintToChatAll($"{Config.Settings.Main.Prefix} {ChatColors.Grey}{message}");
    }

    public void PlaySoundAll(string sound)
    {
        foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot))
            player.PlaySound(sound);
    }

    public bool IsValidJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Logger.LogInformation($"JSON Check: file does not exist ({filePath})");
            return false;
        }

        string fileContent = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(fileContent))
        {
            Logger.LogError($"JSON Check: file is empty ({filePath})");
            return false;
        }

        try
        {
            JsonDocument.Parse(fileContent);
            return true;
        }
        catch (JsonException)
        {
            Logger.LogError($"JSON Check: invalid content ({filePath})");
            return false;
        }
    }

    public string GetModelFromSelectedBlock(CCSPlayerController player, string size)
    {
        var blockType = playerData[player.Slot].BlockType;

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockSizes)property.GetValue(BlockModels)!;

            if (block.Title.Equals(blockType, StringComparison.OrdinalIgnoreCase))
            {
                return size.ToLower() switch
                {
                    "small" => block.Small,
                    "medium" => block.Medium,
                    "large" => block.Large,
                    "pole" => block.Pole,
                    _ => block.Medium,
                };
            }
        }

        return string.Empty;
    }

    public int GetPlacedBlocksCount()
    {
        int blockCount = 0;

        foreach (var block in Utilities.GetAllEntities().Where(b => b.DesignerName == "prop_physics_override"))
        {
            if (block == null || !block.IsValid || block.Entity == null)
                continue;

            if (!String.IsNullOrEmpty(block.Entity.Name) && block.Entity.Name.StartsWith("blockmaker"))
                blockCount++;
        }

        return blockCount;
    }

    public string GetMapName()
    {
        return Server.MapName.ToString();
    }

    public Color ParseColor(string colorValue)
    {
        var colorParts = colorValue.Split(',');
        if (colorParts.Length == 4 &&
            int.TryParse(colorParts[0], out var r) &&
            int.TryParse(colorParts[1], out var g) &&
            int.TryParse(colorParts[2], out var b) &&
            int.TryParse(colorParts[3], out var a))
        {
            return Color.FromArgb(a, r, g, b);
        }
        return Color.FromArgb(255, 255, 255, 255);
    }

    public static readonly Dictionary<string, Color> ColorMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "default", Color.White },
        { "red", Color.Red },
        { "green", Color.Green },
        { "blue", Color.Blue },
        { "yellow", Color.Yellow },
        { "orange", Color.Orange },
        { "lime", Color.Lime },
        { "aqua", Color.Aqua },
        { "lightblue", Color.LightBlue },
        { "darkblue", Color.DarkBlue },
        { "purple", Color.Purple },
        { "pink", Color.LightPink},
        { "hotpink", Color.HotPink},
        { "gray", Color.Gray },
        { "silver", Color.Silver },
        { "white", Color.White },
    };
    public static Color GetColor(string colorName)
    {
        return ColorMapping.TryGetValue(colorName.ToLower(), out var color) ? color : ColorMapping["default"];
    }
}