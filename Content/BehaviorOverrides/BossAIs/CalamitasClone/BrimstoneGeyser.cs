using InfernumMode.Assets.Effects;
using InfernumMode.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.CalamitasClone
{
    public class BrimstoneGeyser : ModProjectile, IPixelPrimitiveDrawer
    {
        internal PrimitiveTrailCopy LavaDrawer;
        internal ref float Time => ref Projectile.ai[0];
        internal ref float GeyserHeight => ref Projectile.ai[1];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Lava");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Projectile.Opacity = (float)Math.Sin(Projectile.timeLeft / 120f * MathHelper.Pi) * 4f;
            if (Projectile.Opacity > 1f)
                Projectile.Opacity = 1f;

            if (GeyserHeight < 2f)
                GeyserHeight = 2f;

            if (Time > 20f)
                GeyserHeight = MathHelper.Lerp(GeyserHeight, GeyserHeight * 1.45f, 0.2f) + 2f;
            else
                Projectile.timeLeft++;

            if (Time == 0f)
            {
                Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                SoundEngine.PlaySound(SoundID.Item73, closestPlayer.Center);
            }

            Time++;
        }

        internal Color ColorFunction(float completionRatio) => Color.Red * Projectile.Opacity;

        internal float WidthFunction(float completionRatio) => MathHelper.Lerp(56f, 63f, (float)Math.Abs(Math.Cos(Main.GlobalTimeWrappedHourly * 2f))) * Utils.GetLerpValue(20f, 270f, GeyserHeight, true) * Projectile.Opacity;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float width = WidthFunction(0.6f);
            Vector2 start = Projectile.Center - Vector2.UnitY * GeyserHeight * 0.15f;
            Vector2 end = Projectile.Center - Vector2.UnitY * GeyserHeight * 0.7f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Time < 20f)
            {
                float telegraphFade = Utils.GetLerpValue(0f, 8f, Time, true) * Utils.GetLerpValue(20f, 10f, Time, true);
                float telegraphWidth = telegraphFade * 4f;
                Color telegraphColor = Color.Lerp(Color.Purple, Color.DarkRed, 0.375f) * telegraphFade;
                Vector2 start = Projectile.Center - Vector2.UnitY * 2000f;
                Vector2 end = Projectile.Center + Vector2.UnitY * 2000f;
                Main.spriteBatch.DrawLineBetter(start, end, telegraphColor, telegraphWidth);
            }

            return false;
        }

        public void DrawPixelPrimitives(SpriteBatch spriteBatch)
        {
            LavaDrawer ??= new PrimitiveTrailCopy(WidthFunction, ColorFunction, null, true, InfernumEffectsRegistry.WoFGeyserVertexShader);
            InfernumEffectsRegistry.WoFGeyserVertexShader.UseSaturation(-1f);
            InfernumEffectsRegistry.WoFGeyserVertexShader.UseColor(Color.Orange);
            InfernumEffectsRegistry.WoFGeyserVertexShader.SetShaderTexture(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"));

            List<Vector2> points = new();
            for (int i = 0; i < 25; i++)
                points.Add(Vector2.Lerp(Projectile.Center, Projectile.Center - Vector2.UnitY * GeyserHeight, i / 24f));
            LavaDrawer.DrawPixelated(points, Vector2.UnitX * 10f - Main.screenPosition, 35);

            InfernumEffectsRegistry.WoFGeyserVertexShader.UseSaturation(1f);
            LavaDrawer.DrawPixelated(points, Vector2.UnitX * -10f - Main.screenPosition, 35);
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {

        }
    }
}
