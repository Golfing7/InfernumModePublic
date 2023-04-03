using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace InfernumMode.Common.Graphics.Particles
{
    public class ExplosionRing : Particle
    {
        private readonly float Spin;

        private readonly Color originalColor;

        public override string Texture => "InfernumMode/Assets/ExtraTextures/GreyscaleObjects/HollowCircleSoftEdge";

        public override bool SetLifetime => true;

        public ExplosionRing(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime, float rotationSpeed = 0.2f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            originalColor = color;
            Scale = scale;
            Lifetime = lifeTime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = rotationSpeed;
            Variant = Main.rand.Next(3);
        }

        public override void Update()
        {
            Color = originalColor;
            Velocity = Velocity * new Vector2(0.95f, 1f) + Vector2.UnitY * 0.28f;
            Rotation += Spin * (Velocity.X > 0 ? 1f : -1f);

            if (Collision.SolidCollision(Position, 1, 1) && Time < Lifetime - 1 && Time > 8)
            {
                SoundEngine.PlaySound(SoundID.Item51, Position);
                Time = Lifetime - 1;
            }
        }
    }
}
