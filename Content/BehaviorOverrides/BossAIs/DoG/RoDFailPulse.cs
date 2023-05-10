using InfernumMode.Common.BaseEntities;
using Microsoft.Xna.Framework;
using System;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.DoG
{
    public class RoDFailPulse : BaseWaveExplosionProjectile
    {
        public override int Lifetime => 45;

        public override float MaxRadius => 72f;

        public override float RadiusExpandRateInterpolant => 0.15f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override float DetermineScreenShakePower(float lifetimeCompletionRatio, float distanceFromPlayer) => MathF.Sin(MathHelper.Pi * lifetimeCompletionRatio) * 3f;

        public override Color DetermineExplosionColor(float lifetimeCompletionRatio) => Color.Lerp(Color.Cyan, Color.Fuchsia, lifetimeCompletionRatio * 2f % 1f);
    }
}
