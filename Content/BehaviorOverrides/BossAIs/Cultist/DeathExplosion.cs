using InfernumMode.Common.BaseEntities;
using Microsoft.Xna.Framework;
using System.IO;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Cultist
{
    public class DeathExplosion : BaseWaveExplosionProjectile
    {
        public override int Lifetime => 150;
        public override float MaxRadius => 2100f;
        public override float RadiusExpandRateInterpolant => 0.15f;
        public override float DetermineScreenShakePower(float lifetimeCompletionRatio, float distanceFromPlayer) => 0f;

        public override Color DetermineExplosionColor(float lifetimeCompletionRatio)
        {
            switch ((int)Projectile.localAI[1])
            {
                // Vortex.
                case 0:
                    return Color.Teal;
                // Stardust.
                case 1:
                    return Color.DeepSkyBlue;
                // Nebula.
                case 2:
                    return Color.Violet;
                // Solar.
                case 3:
                    return Color.Orange;
            }

            return Color.White;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write((int)Projectile.localAI[1]);

        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.localAI[1] = reader.ReadInt32();
    }
}
