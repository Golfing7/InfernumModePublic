using InfernumMode.Assets.Effects;
using InfernumMode.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.OldDuke
{
    public class AcidFountainBubble : ModProjectile, IPixelPrimitiveDrawer
    {
        public PrimitiveTrailCopy WaterDrawer;

        public ref float Time => ref Projectile.ai[0];

        public const float Radius = 24f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Acid Bubble");

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = (int)Radius;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 150;
            CooldownSlot = 1;
        }

        public override void AI()
        {
            Projectile.Opacity = (float)Math.Sin(MathHelper.Pi * Projectile.timeLeft / 150f) * 4f;
            if (Projectile.Opacity > 1f)
                Projectile.Opacity = 1f;
            Projectile.scale = Projectile.Opacity;

            Time++;
        }

        public float WidthFunction(float completionRatio) => Radius * Projectile.scale * (float)Math.Sin(MathHelper.Pi * completionRatio);

        public Color ColorFunction(float completionRatio)
        {
            float colorInterpolant = (float)Math.Pow(Math.Abs(Math.Sin(completionRatio * MathHelper.Pi + Main.GlobalTimeWrappedHourly)), 3D) * 0.5f;
            return Color.Lerp(new Color(140, 234, 87), new Color(144, 114, 166), colorInterpolant) * Projectile.Opacity;
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 shootVelocity = (MathHelper.TwoPi * i / 3f).ToRotationVector2() * 9f;
                Utilities.NewProjectileBetter(Projectile.Center, shootVelocity, ModContent.ProjectileType<HomingAcid>(), 275, 0f);
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            WaterDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.DukeTornadoVertexShader);

            InfernumEffectsRegistry.DukeTornadoVertexShader.UseImage1("Images/Misc/Perlin");
            List<Vector2> drawPoints = new();

            for (float offsetAngle = -MathHelper.PiOver2; offsetAngle <= MathHelper.PiOver2; offsetAngle += MathHelper.Pi / 6f)
            {
                drawPoints.Clear();

                float adjustedAngle = offsetAngle + Main.GlobalTimeWrappedHourly * 2.2f;
                Vector2 offsetDirection = adjustedAngle.ToRotationVector2();
                Vector2 radius = Vector2.One * Radius;
                radius.Y *= MathHelper.Lerp(1f, 2f, (float)Math.Abs(Math.Cos(Main.GlobalTimeWrappedHourly * 1.9f)));

                for (int i = 0; i <= 8; i++)
                {
                    drawPoints.Add(Vector2.Lerp(Projectile.Center - offsetDirection * radius * 0.8f, Projectile.Center + offsetDirection * radius * 0.8f, i / 8f));
                }

                WaterDrawer.DrawPixelated(drawPoints, -Main.screenPosition, 42, adjustedAngle);
            }
        }
    }
}
