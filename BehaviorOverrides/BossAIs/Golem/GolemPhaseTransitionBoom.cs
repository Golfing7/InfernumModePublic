using InfernumMode.BaseEntities;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;

namespace InfernumMode.BehaviorOverrides.BossAIs.Golem
{
    public class GolemPhaseTransitionBoom : BaseWaveExplosionProjectile
    {
        public override int Lifetime => 400;
        public override float MaxRadius => 2000f;
        public override float RadiusExpandRateInterpolant => 0.15f;
        public override float DetermineScreenShakePower(float lifetimeCompletionRatio, float distanceFromPlayer)
        {
            float baseShakePower = MathHelper.Lerp(2f, 7f, (float)Math.Sin(MathHelper.Pi * lifetimeCompletionRatio));
            return baseShakePower * Utils.InverseLerp(2200f, 1050f, distanceFromPlayer, true);
        }

        public override Color DetermineExplosionColor(float lifetimeCompletionRatio)
        {
            return Color.Lerp(Color.Yellow, Color.DarkOrange, MathHelper.Clamp(lifetimeCompletionRatio * 1.75f, 0f, 1f));
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write((int)projectile.localAI[1]);

        public override void ReceiveExtraAI(BinaryReader reader) => projectile.localAI[1] = reader.ReadInt32();
    }
}
