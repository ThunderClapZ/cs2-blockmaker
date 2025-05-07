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
        bool isBuilder = instance.BuilderData.TryGetValue(player.Slot, out var BuilderData);

        if (instance.buildMode && (isBuilder || HasPermission(player)))
            return true;

        else if (!instance.buildMode && (isBuilder || HasPermission(player)))
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
        if (config.Commands.Admin.Permission.Count == 0)
            return true;

        foreach (string perm in config.Commands.Admin.Permission)
        {
            if (perm.StartsWith("@") && AdminManager.PlayerHasPermissions(player, perm))
                return true;
            if (perm.StartsWith("#") && AdminManager.PlayerInGroup(player, perm))
                return true;
        }
        return false;
    }

    public static void Log(string message)
    {
        instance.Logger.LogInformation(message);
    }

    public static void PrintToChat(CCSPlayerController player, string message)
    {
        player.PrintToChat($" {config.Settings.Prefix} {message}");
    }

    public static void PrintToChatAll(string message)
    {
        Server.PrintToChatAll($" {config.Settings.Prefix} {message}");
    }

    public static void PlaySoundAll(string sound)
    {
        foreach (var player in Utilities.GetPlayers())
        {
            RecipientFilter filter = [player];
            player.EmitSound(sound, filter);
        }
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

    public static string GetModelFromSelectedBlock(string blockType, bool pole)
    {
        int hyphenIndex = blockType.IndexOf('.');
        if (hyphenIndex >= 0)
            blockType = blockType.Substring(0, hyphenIndex);
        var blockModels = Files.Models.Props;

        foreach (var model in blockModels.GetAllBlocks())
        {
            if (model.Title.Equals(blockType, StringComparison.OrdinalIgnoreCase))
                return pole ? model.Pole : model.Block;
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
        { "White", Color.White },
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
        var blockSize = config.Settings.Blocks.Sizes
            .FirstOrDefault(bs => bs.Title.Equals(input, StringComparison.OrdinalIgnoreCase));

        return blockSize?.Size ?? config.Settings.Blocks.Sizes.First(bs => bs.Size == 1.0f).Size;
    }

    public static CBeam DrawBeam(Vector startPos, Vector endPos, Color color, float width = 0.25f)
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

    public static void DrawBeamsAroundBlock(CCSPlayerController player, CBaseProp block, Color color)
    {
        var pos = block.AbsOrigin!;
        var rotation = block.AbsRotation!;

        float scale = Blocks.Props.ContainsKey(block) ? Utils.GetSize(Blocks.Props[block].Size) : 1;

        var max = block.Collision!.Maxs * scale;
        var min = block.Collision!.Mins * scale;

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
        Vector up = VectorUtils.Cross(forward, right);

        Vector[] localCorners =
        {
            new Vector(min.X, min.Y, min.Z), // Bottom-back-left
            new Vector(max.X, min.Y, min.Z), // Bottom-back-right
            new Vector(max.X, max.Y, min.Z), // Bottom-front-right
            new Vector(min.X, max.Y, min.Z), // Bottom-front-left
            new Vector(min.X, min.Y, max.Z), // Top-back-left
            new Vector(max.X, min.Y, max.Z), // Top-back-right
            new Vector(max.X, max.Y, max.Z), // Top-front-right
            new Vector(min.X, max.Y, max.Z)  // Top-front-left
        };

        Vector[] corners = new Vector[8];
        for (int i = 0; i < localCorners.Length; i++)
        {
            Vector localCorner = localCorners[i];
            corners[i] =
                pos +
                forward * localCorner.X +
                right * localCorner.Y +
                up * localCorner.Z;
        }

        var beams = new List<Vector[]>
        {
            new[] {corners[0], corners[1]}, new[] {corners[1], corners[2]}, new[] {corners[2], corners[3]}, new[] {corners[3], corners[0]},
            new[] {corners[4], corners[5]}, new[] {corners[5], corners[6]}, new[] {corners[6], corners[7]}, new[] {corners[7], corners[4]},
            new[] {corners[0], corners[4]}, new[] {corners[1], corners[5]}, new[] {corners[2], corners[6]}, new[] {corners[3], corners[7]}
        };

        // Update existing
        if (Blocks.PlayerHolds[player].beams.Count > 0)
        {
            int beamCount = 0;
            foreach (var oldBeam in Blocks.PlayerHolds[player].beams)
            {
                oldBeam.EndPos.X = beams[beamCount][1].X;
                oldBeam.EndPos.Y = beams[beamCount][1].Y;
                oldBeam.EndPos.Z = beams[beamCount][1].Z;

                oldBeam.DispatchSpawn();

                oldBeam.Teleport(beams[beamCount][0], block.AbsRotation);

                beamCount++;
            }
            return;
        }

        // Create new
        foreach (var beam in beams)
        {
            var newBeam = DrawBeam(beam[0], beam[1], color);
            Blocks.PlayerHolds[player].beams.Add(newBeam);
        }
    }
}