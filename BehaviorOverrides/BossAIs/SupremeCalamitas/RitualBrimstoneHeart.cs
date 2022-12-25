using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.SupremeCalamitas
{
    public class RitualBrimstoneHeart : ModProjectile
    {
        public PrimitiveTrail RayDrawer = null;

        public const float LaserLength = 2700f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Heart");
            Main.projFrames[Projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 60;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 96000;
        }

        public override void AI()
        {
            // It is SCal's responsibility to move these things around.
            if (CalamityGlobalNPC.SCal == -1 || !Main.npc[CalamityGlobalNPC.SCal].active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Opacity = Utils.GetLerpValue(96000f, 95960f, Projectile.timeLeft, true) * Projectile.Infernum().ExtraAI[0];
            Projectile.frameCounter++;
            Projectile.frame = (int)((Projectile.frameCounter / 6 + Projectile.ai[0] * 4f) % Main.projFrames[Projectile.type]);
        }
        
        internal float PrimitiveWidthFunction(float completionRatio) => Projectile.scale * 30f;

        internal Color PrimitiveColorFunction(float completionRatio)
        {
            float opacity = Projectile.Opacity * Utils.GetLerpValue(0.97f, 0.9f, completionRatio, true) *
                Utils.GetLerpValue(0f, MathHelper.Clamp(15f / LaserLength, 0f, 0.5f), completionRatio, true) *
                (float)Math.Pow(Utils.GetLerpValue(60f, 270f, LaserLength, true), 3D);
            float flameInterpolant = (float)Math.Sin(completionRatio * 3f + Main.GlobalTimeWrappedHourly * 0.5f + Projectile.identity * 0.3156f) * 0.5f + 0.5f;
            Color c = Color.Lerp(Color.White, Color.Orange, MathHelper.Lerp(0.5f, 0.8f, flameInterpolant)) * opacity;
            c.A = 0;

            return c * Projectile.ai[1] * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (RayDrawer is null)
                RayDrawer = new PrimitiveTrail(PrimitiveWidthFunction, PrimitiveColorFunction, specialShader: GameShaders.Misc["Infernum:PrismaticRay"]);

            Vector2 overallOffset = -Main.screenPosition;
            Vector2[] basePoints = new Vector2[24];
            for (int i = 0; i < basePoints.Length; i++)
                basePoints[i] = Projectile.Center - Vector2.UnitY * i / (basePoints.Length - 1f) * LaserLength;

            Projectile.scale *= 0.8f;
            GameShaders.Misc["Infernum:PrismaticRay"].UseImage1("Images/Misc/Perlin");
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakSolid.Value;
            Projectile.scale /= 0.8f;

            RayDrawer.Draw(basePoints, overallOffset, 42);

            Projectile.scale *= 1.5f;
            GameShaders.Misc["Infernum:PrismaticRay"].SetShaderTexture(InfernumTextureRegistry.CultistRayMap);
            Main.instance.GraphicsDevice.Textures[2] = InfernumTextureRegistry.StreakFaded.Value;
            RayDrawer.Draw(basePoints, overallOffset, 42);
            Projectile.scale /= 1.5f;
            return true;
        }
    }
}
