using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using FixVectorLeak.src;
using FixVectorLeak.src.Structs;
using Microsoft.Extensions.Logging;
using StarCore.Utils;
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
        var blockModels = Blocks.Models.Data;

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

    public static CBeam DrawBeam(Vector_t startPos, Vector_t endPos, Color color, float width = 0.25f)
    {
        var beam = Utilities.CreateEntityByName<CBeam>("beam")!;

        beam.Entity!.Name = "blockmaker_beam";
        beam.Render = color;
        beam.Width = width;
        beam.EndPos.Add(new(endPos.X, endPos.Y, endPos.Z));

        beam.Teleport(startPos);
        beam.DispatchSpawn();

        return beam;
    }

    public static void DrawBeamsAroundBlock(CCSPlayerController player, CBaseProp block, Color color)
    {
        Vector_t pos = block.AbsOrigin!.ToVector_t();
        QAngle_t rotation = block.AbsRotation!.ToQAngle_t();

        float scale = Blocks.Entities.ContainsKey(block) ? Utils.GetSize(Blocks.Entities[block].Size) : 1;

        var max = block.Collision!.Maxs * scale;
        var min = block.Collision!.Mins * scale;

        Vector_t forward = new(
            (float)Math.Cos(rotation.Y * Math.PI / 180) * (float)Math.Cos(rotation.X * Math.PI / 180),
            (float)Math.Sin(rotation.Y * Math.PI / 180) * (float)Math.Cos(rotation.X * Math.PI / 180),
            (float)-Math.Sin(rotation.X * Math.PI / 180)
        );
        Vector_t right = new(
            (float)Math.Cos((rotation.Y + 90) * Math.PI / 180),
            (float)Math.Sin((rotation.Y + 90) * Math.PI / 180),
            0
        );
        Vector_t up = VectorUtils.Cross(forward, right);

        Vector_t[] localCorners =
        {
            new(min.X, min.Y, min.Z), // Bottom-back-left
            new(max.X, min.Y, min.Z), // Bottom-back-right
            new(max.X, max.Y, min.Z), // Bottom-front-right
            new(min.X, max.Y, min.Z), // Bottom-front-left
            new(min.X, min.Y, max.Z), // Top-back-left
            new(max.X, min.Y, max.Z), // Top-back-right
            new(max.X, max.Y, max.Z), // Top-front-right
            new(min.X, max.Y, max.Z)  // Top-front-left
        };

        Vector_t[] corners = new Vector_t[8];
        for (int i = 0; i < localCorners.Length; i++)
        {
            Vector_t localCorner = localCorners[i];
            corners[i] =
                pos +
                forward * localCorner.X +
                right * localCorner.Y +
                up * localCorner.Z;
        }

        var beams = new List<Vector_t[]>
        {
            new[] {corners[0], corners[1]}, new[] {corners[1], corners[2]}, new[] {corners[2], corners[3]}, new[] {corners[3], corners[0]},
            new[] {corners[4], corners[5]}, new[] {corners[5], corners[6]}, new[] {corners[6], corners[7]}, new[] {corners[7], corners[4]},
            new[] {corners[0], corners[4]}, new[] {corners[1], corners[5]}, new[] {corners[2], corners[6]}, new[] {corners[3], corners[7]}
        };

        // Update existing
        if (Building.PlayerHolds[player].Beams.Count > 0)
        {
            int beamCount = 0;
            foreach (var oldBeam in Building.PlayerHolds[player].Beams)
            {
                oldBeam.EndPos.X = beams[beamCount][1].X;
                oldBeam.EndPos.Y = beams[beamCount][1].Y;
                oldBeam.EndPos.Z = beams[beamCount][1].Z;

                oldBeam.DispatchSpawn();

                oldBeam.Teleport(beams[beamCount][0], block.AbsRotation!.ToQAngle_t());

                beamCount++;
            }
            return;
        }

        // Create new
        foreach (var beam in beams)
        {
            var newBeam = DrawBeam(beam[0], beam[1], color);
            Building.PlayerHolds[player].Beams.Add(newBeam);
        }
    }

    public static bool BlockLocked(CCSPlayerController player, Blocks.Data block)
    {
        if (block.Properties.Locked)
        {
            PrintToChat(player, "Block is locked");
            return true;
        }
        return false;
    }

    public static void RemoveEntities()
    {
        var entityNames = new HashSet<string>
        {
            "prop_physics_override",
            "trigger_multiple",
            "light_omni2",
            "env_particle_glow"
        };
        foreach (var entity in Utilities.GetAllEntities().Where(x => entityNames.Contains(x.DesignerName)))
        {
            if (entity.Entity == null || string.IsNullOrEmpty(entity.Entity.Name))
                continue;

            if (entity.Entity.Name.StartsWith("blockmaker"))
                entity.Remove();
        }
    }

    public static void Clear()
    {
        RemoveEntities();

        foreach (var timer in Plugin.Instance.Timers)
        {
            if (timer == Events.AutoSaveTimer)
                continue;

            timer?.Kill();
        }

        Blocks.Entities.Clear();
        Blocks.Triggers.Clear();

        Teleports.Entities.Clear();
        Teleports.isNext.Clear();

        Lights.Entities.Clear();

        Blocks.PlayerCooldowns.Clear();
        Blocks.CooldownsTimers.Clear();
        Blocks.TempTimers.Clear();

        Blocks.HiddenPlayers.Clear();
        Blocks.nuked = false;

        Building.PlayerHolds.Clear();
    }

    /// <summary>
    /// 获取玩家的money
    /// </summary>
    /// <param name="player"></param>
    public static int GetMoney(CCSPlayerController player)
    {
        if (!Lib.IsPlayerValid(player) || player.InGameMoneyServices is null) return -1;
        return player.InGameMoneyServices.Account;
    }

    /// <summary>
    /// 设置玩家的money
    /// </summary>
    /// <param name="player"></param>
    /// <param name="money"></param>
    public static void SetMoney(CCSPlayerController player, int money, bool ignoreLimit = false)
    {
        if (!Lib.IsPlayerValid(player) || player.InGameMoneyServices is null) return;
        player.InGameMoneyServices.Account = money;
        if (!ignoreLimit)
        {
            if (player.InGameMoneyServices.Account > 32000)
            {
                player.InGameMoneyServices.Account = 32000;
            }
        }
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
    }
    
    /// <summary>
    /// 增加玩家的money
    /// </summary>
    /// <param name="player"></param>
    /// <param name="money">只支持正数</param>
    public static void AddMoney(CCSPlayerController player, int money, bool ignoreLimit = false)
    {
        if (!Lib.IsPlayerValid(player) || money <=0 || player.InGameMoneyServices is null) return;
        player.InGameMoneyServices.Account += money;
        if (!ignoreLimit)
        {
            if (player.InGameMoneyServices.Account > 32000)
            {
                player.InGameMoneyServices.Account = 32000;
            }
        }
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
    }

    /// <summary>
    /// 减少玩家的money
    /// </summary>
    /// <param name="player"></param>
    /// <param name="money">只支持正数</param>
    public static void ReduceMoney(CCSPlayerController player, int money)
    {
        if (!Lib.IsPlayerValid(player) || money <= 0 || player.InGameMoneyServices is null) return;
        player.InGameMoneyServices.Account -= money;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
    }
}