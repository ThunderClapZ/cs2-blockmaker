using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Text.Json;

public static class Utils
{
    private static Plugin instance = Plugin.Instance;
    private static Config config = instance.Config;

    public static bool BuildMode(CCSPlayerController player)
    {
        if (instance.buildMode && instance.playerData[player.Slot].Builder)
            return true;
        else if (!instance.buildMode && instance.playerData[player.Slot].Builder || HasPermission(player))
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

    public static bool HasPermission(CCSPlayerController player)
    {
        return string.IsNullOrEmpty(config.Commands.Admin.Permission) || AdminManager.PlayerHasPermissions(player, config.Commands.Admin.Permission);
    }

    public static void Log(string message)
    {
        instance.Logger.LogInformation($"[BlockMaker] {message}");
    }

    public static void PrintToChat(CCSPlayerController player, string message)
    {
        player.PrintToChat($"{config.Settings.Main.Prefix} {ChatColors.Grey}{message}");
    }

    public static void PrintToChatAll(string message)
    {
        Server.PrintToChatAll($"{config.Settings.Main.Prefix} {ChatColors.Grey}{message}");
    }

    public static void PlaySoundAll(string sound)
    {
        foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot))
            player.PlaySound(sound);
    }

    public static bool IsValidJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Log($"JSON Check: file does not exist ({filePath})");
            return false;
        }

        string fileContent = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(fileContent))
        {
            Log($"JSON Check: file is empty ({filePath})");
            return false;
        }

        try
        {
            JsonDocument.Parse(fileContent);
            return true;
        }
        catch (JsonException)
        {
            Log($"JSON Check: invalid content ({filePath})");
            return false;
        }
    }

    public static string GetModelFromSelectedBlock(CCSPlayerController player, string size)
    {
        var blockType = instance.playerData[player.Slot].BlockType;

        int hyphenIndex = blockType.IndexOf('.');
        if (hyphenIndex >= 0)
            blockType = blockType.Substring(0, hyphenIndex);

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockSizes)property.GetValue(Plugin.BlockModels)!;

            if (block.Title.Equals(blockType, StringComparison.OrdinalIgnoreCase))
            {
                return size.ToLower() switch
                {
                    "pole" => block.Pole,
                    "small" => block.Block,
                    "normal" => block.Block,
                    "large" => block.Block,
                    "x-large" => block.Block,
                    _ => block.Block,
                };
            }
        }

        return string.Empty;
    }

    public static int GetPlacedBlocksCount()
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

    public static string GetMapName()
    {
        return Server.MapName.ToString();
    }

    public static Color ParseColor(string input)
    {
        var colorParts = input.Split(',');
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
        { "None", Color.White },
        { "Red", Color.Red },
        { "Green", Color.Green },
        { "Blue", Color.Blue },
        { "Yellow", Color.Yellow },
        { "Orange", Color.Orange },
        { "Lime", Color.Lime },
        { "Aqua", Color.Aqua },
        { "Lightblue", Color.LightBlue },
        { "Darkblue", Color.DarkBlue },
        { "Purple", Color.Purple },
        { "Pink", Color.LightPink},
        { "Hotpink", Color.HotPink},
        { "Gray", Color.Gray },
        { "Silver", Color.Silver },
    };
    public static Color GetColor(string input)
    {
        return ColorMapping.TryGetValue(input.ToLower(), out var color) ? color : ColorMapping["None"];
    }

    public static readonly Dictionary<string, int> AlphaMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "100%", 255 },
        { "75%", 191 },
        { "50%", 128 },
        { "25%", 50 },
        { "0%", 0 },
    };
    public static int GetAlpha(string input)
    {
        return AlphaMapping.TryGetValue(input.ToLower(), out var alpha) ? alpha : AlphaMapping["0%"];
    }

    public static readonly Dictionary<string, float> SizeMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "small", 0.5f },
        { "normal", 1f },
        { "large", 2f },
        { "x-large", 3f},
    };
    public static float GetSize(string input)
    {
        return SizeMapping.TryGetValue(input.ToLower(), out var size) ? size : SizeMapping["normal"];
    }

    public static CBeam DrawBeam(Vector startPos, Vector endPos, Color color, float width = 1)
    {
        var beam = Utilities.CreateEntityByName<CBeam>("beam")!;

        beam.Render = color;
        beam.Width = width;
        beam.Teleport(startPos);
        beam.EndPos.Add(endPos);
        beam.DispatchSpawn();

        return beam;
    }
}