using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.Ravager
{
    public class DarkFlamePillar : ModProjectile
    {
        public int OwnerIndex;

        public PrimitiveTrailCopy FireDrawer;

        public ref float Time => ref Projectile.ai[0];

        public ref float InitialRotationalOffset => ref Projectile.localAI[0];

        public const int Lifetime = 136;

        public float Height => MathHelper.Lerp(4f, Projectile.height, Projectile.scale * Projectile.Opacity);

        public float Width => MathHelper.Lerp(3f, Projectile.width, Projectile.scale * Projectile.Opacity);

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Dark Flame Pillar");

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 960;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 255;
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.MaxUpdates = 2;
            CooldownSlot = 1;
        }

        public override void AI()
        {
            // Fade in.
            Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + 0.04f, 0f, 1f);

            Projectile.scale = (float)Math.Sin(MathHelper.Pi * Time / Lifetime) * 2f;
            if (Projectile.scale > 1f)
                Projectile.scale = 1f;

            // Create bright light.
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 1.4f);

            Time++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Top;
            Vector2 end = start - Vector2.UnitY.RotatedBy(Projectile.rotation) * Height * 0.72f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Width * 0.82f, ref _);
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity >= 0.9f;

        public float WidthFunction(float completionRatio)
        {
            float tipFadeoffInterpolant = MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(1f, 0.75f, completionRatio, true));
            float baseFadeoffInterpolant = MathHelper.SmoothStep(2.4f, 1f, 1f - CalamityUtils.Convert01To010(Utils.GetLerpValue(0f, 0.19f, completionRatio, true)));
            float widthAdditionFactor = (float)Math.Sin(Main.GlobalTimeWrappedHourly * -13f + Projectile.identity + completionRatio * MathHelper.Pi * 4f) * 0.2f;
            return Width * tipFadeoffInterpolant * baseFadeoffInterpolant * (1f + widthAdditionFactor);
        }

        public Color ColorFunction(float completionRatio)
        {
            Color darkFlameColor = new(58, 107, 252);
            Color lightFlameColor = new(45, 207, 239);
            float colorShiftInterpolant = (float)Math.Sin(-Main.GlobalTimeWrappedHourly * 6.7f + completionRatio * MathHelper.TwoPi) * 0.5f + 0.5f;
            Color color = Color.Lerp(darkFlameColor, lightFlameColor, (float)Math.Pow(colorShiftInterpolant, 1.64f));
            return color * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            FireDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, GameShaders.Misc["Infernum:DarkFlamePillar"]);

            // Create a telegraph line upward that fades away away the pillar fades in.
            Vector2 start = Projectile.Top;
            Vector2 end = start - Vector2.UnitY.RotatedBy(Projectile.rotation) * Height;
            if (Projectile.Opacity < 1f)
                Main.spriteBatch.DrawLineBetter(start + Projectile.Size * 0.5f, end + Projectile.Size * 0.5f, Color.Cyan * (1f - Projectile.Opacity), Projectile.Opacity * 6f);

            var oldBlendState = Main.instance.GraphicsDevice.BlendState;
            Main.instance.GraphicsDevice.BlendState = BlendState.Additive;
            GameShaders.Misc["Infernum:DarkFlamePillar"].UseSaturation(1.4f);
            GameShaders.Misc["Infernum:DarkFlamePillar"].SetShaderTexture(InfernumTextureRegistry.StreakFaded);
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakFaded.Value;

            List<Vector2> points = new();
            for (int i = 0; i <= 8; i++)
                points.Add(Vector2.Lerp(start, end, i / 8f));

            if (Time >= 2f)
                FireDrawer.Draw(points, Projectile.Size * 0.5f - Main.screenPosition, 166);
            Main.instance.GraphicsDevice.BlendState = oldBlendState;
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
