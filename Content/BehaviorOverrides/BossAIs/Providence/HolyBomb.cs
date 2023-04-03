using CalamityMod;
using InfernumMode.Assets.ExtraTextures;
using InfernumMode.Core.GlobalInstances.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Providence
{
    public class HolyBomb : ModProjectile
    {
        public float ExplosionRadius => Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Holy Bomb");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;

        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.Opacity = 0f;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.timeLeft);

        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.timeLeft = reader.ReadInt32();

        public override void AI()
        {
            Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + 0.08f, 0f, 0.55f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.velocity *= 0.99f;
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Projectile.type];
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3() * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            if (ProvidenceBehaviorOverride.IsEnraged)
                texture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/Providence/HolyBombNight").Value;

            float explosionInterpolant = Utils.GetLerpValue(200f, 35f, Projectile.timeLeft, true);
            float circleFadeinInterpolant = Utils.GetLerpValue(0f, 0.15f, explosionInterpolant, true);
            float pulseInterpolant = Utils.GetLerpValue(0.75f, 0.85f, explosionInterpolant, true);
            float colorPulse = ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 6.3f + Projectile.identity) * 0.5f + 0.5f) * pulseInterpolant;
            lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
            lightColor.A = 128;
            Utilities.DrawAfterimagesCentered(Projectile, lightColor, ProjectileID.Sets.TrailingMode[Projectile.type], 1, texture);

            if (explosionInterpolant > 0f)
            {
                Texture2D explosionTelegraphTexture = InfernumTextureRegistry.HollowCircleSoftEdge.Value;
                Vector2 scale = Vector2.One * ExplosionRadius / explosionTelegraphTexture.Size();
                Color explosionTelegraphColor = Color.Lerp(Color.Yellow, Color.Red, colorPulse) * circleFadeinInterpolant;
                if (ProvidenceBehaviorOverride.IsEnraged)
                    explosionTelegraphColor = Color.Lerp(Color.Cyan, Color.Lime, colorPulse * 0.67f) * circleFadeinInterpolant;

                Main.spriteBatch.SetBlendState(BlendState.Additive);
                Main.spriteBatch.Draw(explosionTelegraphTexture, Projectile.Center - Main.screenPosition, null, explosionTelegraphColor, 0f, explosionTelegraphTexture.Size() * 0.5f, scale, 0, 0f);
                Main.spriteBatch.ResetBlendState();
            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, Projectile.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int explosionDamage = !ProvidenceBehaviorOverride.IsEnraged ? 350 : 600;

                ProjectileSpawnManagementSystem.PrepareProjectileForSpawning(explosion =>
                {
                    explosion.ModProjectile<HolySunExplosion>().MaxRadius = ExplosionRadius * 0.7f;
                });
                Utilities.NewProjectileBetter(Projectile.Center, Vector2.Zero, ModContent.ProjectileType<HolySunExplosion>(), explosionDamage, 0f);
            }

            // Do some some mild screen-shake effects to accomodate the explosion.
            // This effect is set instead of added to to ensure separate explosions do not together create an excessive amount of shaking.
            float screenShakeFactor = Utilities.Remap(Projectile.Distance(Main.LocalPlayer.Center), 2000f, 1300f, 0f, 8f);
            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakeFactor)
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakeFactor;
        }

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of false */ => false;
    }
}
