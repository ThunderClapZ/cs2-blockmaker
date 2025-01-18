using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

public partial class Plugin : BasePlugin, IPluginConfig<Config>
{
    // this is a temporary way to touch blocks
    // huge thanks to aquevadis for developing this
    // https://github.com/aquevadis/bgkoka-cs2-entity-touching-system

    internal static readonly List<CEntityInstance> _entities_have_touch = new();

    public HookResult PawnPostThinkFunc(DynamicHook hook)
    {
        if (Server.TickCount % 8 != 0) return HookResult.Continue;

        var entity = hook.GetParam<CCSPlayerPawnBase>(0);

        if (entity is null || entity.IsValid is not true)
            return HookResult.Continue;

        OnPlayerEntityThink(entity);

        return HookResult.Continue;

    }

    public void OnPlayerEntityThink(CCSPlayerPawnBase player)
    {
        if (player.LifeState is not (byte)LifeState_t.LIFE_ALIVE || player.DesignerName.Contains("player") != true)
            return;

        foreach (var entity_has_touch in _entities_have_touch)
        {
            var entity = entity_has_touch.As<CBaseEntity>();

            if (entity == null || entity.ValidateEntity() != true)
                continue;

            if (entity.AbsOrigin == null || player.AbsOrigin == null)
                continue;

            if (entity.DesignerName != "prop_physics_override")
                continue;

            if (entity.Entity == null || entity.Entity.Name == null)
                continue;

            if (player.OriginalController.Value == null || player.OriginalController.Value.UserId == null)
                continue;

            CCSPlayerController? playerController = Utilities.GetPlayerFromUserid((int)player.OriginalController.Value.UserId);

            if (playerController == null || !playerController.IsValid)
                continue;

            if (Entities.Collides(entity.AbsOrigin, player.AbsOrigin!))
                Blocks.Actions(playerController, entity);
        }
    }

    public static void StartTouch(CEntityInstance entity)
    {
        if (entity is null || entity.ValidateEntity() is not true)
            return;

        if (_entities_have_touch.Contains(entity) is true)
            return;

        _entities_have_touch.Add(entity);
    }

    public static void RemoveTouch(CEntityInstance entity)
    {
        if (entity is null || entity.ValidateEntity() is not true)
            return;

        if (_entities_have_touch.Contains(entity) is not true)
            return;

        _entities_have_touch.Remove(entity);
    }

    public static void ClearAllManagedEntities()
    {
        if (_entities_have_touch.Count > 0)
            _entities_have_touch.Clear();
    }

    public static void OnEntityDeleted(CEntityInstance entity)
    {
        if (entity.ValidateEntity() is not true) return;
        if (_entities_have_touch.Contains(entity) is not true) return;
        _entities_have_touch.Remove(entity);
    }
}

public static class Entities
{

    /// <summary>
    /// Fully checks if entity is valid
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>true if IsValid; NOT 0 and below max entities(32768)</returns>
    public static bool ValidateEntity(this CEntityInstance entity)
    {

        if (entity.IsValid is not true
        || entity.Index <= 0
        || entity.Index >= 32768) return false;

        return true;
    }

    /// <summary>
    /// Fully checks if entity is valid and if it belongs to a player
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>true if IsValid; NOT 0;below 32768 and is player entity</returns>
    public static bool ValidatePotentialPlayerEntity(this CEntityInstance entity)
    {

        if (entity.IsValid is not true
        || entity.Index <= 0
        || entity.Index >= 32768
        || entity.DesignerName.Contains("player") is not true) return false;

        return true;
    }

    /// <summary>
    /// Compare two Vectors and check if they collide
    /// </summary>
    /// <param name="entityPosition"></param>
    /// <param name="pointOfColision"></param>
    /// <returns></returns>
    public static bool Collides(Vector entityPosition, Vector pointOfColision)
    {
        var distSrt = Vector3DistanceSquared(/*position of the entity's that has OnTouch*/entityPosition, pointOfColision);
        var radiusSquared = Math.Pow(40, 2);

        return distSrt < radiusSquared;

    }

    /// <summary>
    /// Calculate distance between two Vectors
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Vector3DistanceSquared(Vector a, Vector b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        float dz = a.Z - b.Z;

        return dx * dx + dy * dy + dz * dz;
    }

}