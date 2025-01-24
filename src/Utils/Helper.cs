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

    public Color ParseColor(string input)
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
        { "100%", 0 },
        { "90%", 26 },
        { "80%", 51 },
        { "70%", 77 },
        { "60%", 102 },
        { "50%", 128 },
        { "40%", 153 },
        { "30%", 179 },
        { "20%", 204 },
        { "10%", 230 },
        { "0%", 255 },
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