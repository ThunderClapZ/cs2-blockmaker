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
        if (instance.buildMode && (instance.playerData[player.Slot].Builder || HasPermission(player)))
            return true;
        else if (!instance.buildMode && (instance.playerData[player.Slot].Builder || HasPermission(player)))
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
        player.PrintToChat($"{config.Settings.Prefix} {ChatColors.Grey}{message}");
    }

    public static void PrintToChatAll(string message)
    {
        Server.PrintToChatAll($"{config.Settings.Prefix} {ChatColors.Grey}{message}");
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

    public static string GetModelFromSelectedBlock(CCSPlayerController player, bool pole)
    {
        var blockType = instance.playerData[player.Slot].BlockType;

        int hyphenIndex = blockType.IndexOf('.');
        if (hyphenIndex >= 0)
            blockType = blockType.Substring(0, hyphenIndex);

        foreach (var property in typeof(BlockModels).GetProperties())
        {
            var block = (BlockModel)property.GetValue(Files.BlockModels)!;

            if (block.Title.Equals(blockType, StringComparison.OrdinalIgnoreCase))
                return pole ? block.Pole : block.Block;
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

    public static float GetSize(string input)
    {
        var blockSize = config.Settings.Building.BlockSizes
            .FirstOrDefault(bs => bs.Title.Equals(input, StringComparison.OrdinalIgnoreCase));

        return blockSize?.Size ?? config.Settings.Building.BlockSizes.First(bs => bs.Size == 1.0f).Size;
    }

    public static CBeam DrawBeam(Vector startPos, Vector endPos, Color color, float width = 0.5f)
    {
        var beam = Utilities.CreateEntityByName<CBeam>("beam")!;

        beam.Entity!.Name = "blockmaker_beam";
        beam.Render = color;
        beam.Width = width;
        beam.EndPos.Add(endPos);

        beam.DispatchSpawn();
        beam.Teleport(startPos);

        return beam;
    }

    public static void DrawBeamsAroundBlock(CCSPlayerController player, CBaseEntity block, Color color)
    {
        var pos = block.AbsOrigin!;

        var max = VectorUtils.GetMaxs(block) * GetSize(Blocks.Props[block].Size);

        var corners = new Vector[]
        {
            pos + new Vector(-max.X, -max.Y, -max.Z),
            pos + new Vector(max.X, -max.Y, -max.Z),
            pos + new Vector(max.X, max.Y, -max.Z),
            pos + new Vector(-max.X, max.Y, -max.Z),
            pos + new Vector(-max.X, -max.Y, max.Z),
            pos + new Vector(max.X, -max.Y, max.Z),
            pos + new Vector(max.X, max.Y, max.Z),
            pos + new Vector(-max.X, max.Y, max.Z)
        };

        var beams = new List<Vector[]>
        {
            new[] {corners[0], corners[1]}, new[] {corners[1], corners[2]}, new[] {corners[2], corners[3]}, new[] {corners[3], corners[0]},
            new[] {corners[4], corners[5]}, new[] {corners[5], corners[6]}, new[] {corners[6], corners[7]}, new[] {corners[7], corners[4]},
            new[] {corners[0], corners[4]}, new[] {corners[1], corners[5]}, new[] {corners[2], corners[6]}, new[] {corners[3], corners[7]}
        };

        if (Blocks.PlayerHolds[player].beams.Count > 0)
        {
            int beamcount = 0;
            foreach (var oldbeam in Blocks.PlayerHolds[player].beams)
            {                
                oldbeam.EndPos.X = beams[beamcount][1].X;
                oldbeam.EndPos.Y = beams[beamcount][1].Y;
                oldbeam.EndPos.Z = beams[beamcount][1].Z;

                oldbeam.DispatchSpawn();

                oldbeam.Teleport(beams[beamcount][0], block.AbsRotation);

                beamcount++;
            }
            return;
        }

        foreach (var beam in beams)
        {
            var newbeam = DrawBeam(beam[0], beam[1], color);
            Blocks.PlayerHolds[player].beams.Add(newbeam);
        }
    }
}