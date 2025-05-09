using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

public partial class Blocks
{
    public class Effect
    {
        public string Title { get; set; } = "None";
        public string Particle { get; set; } = "";

        public Effect(string title, string particle)
        {
            Title = title;
            Particle = particle;
        }
    }

    private static void CreateParticle(CBaseProp block, string effect, string size)
    {
        var particle = Utilities.CreateEntityByName<CEnvParticleGlow>("env_particle_glow");

        if (particle != null && particle.IsValid && particle.Entity != null)
        {
            particle.Entity.Name = "blockmaker_effect";
            particle.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
            particle.StartActive = true;

            particle.EffectName = effect;
            particle.SetModel(block.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
            particle.AcceptInput("FollowEntity", block, particle, "!activator");

            particle.DispatchSpawn();
        }
    }
}
