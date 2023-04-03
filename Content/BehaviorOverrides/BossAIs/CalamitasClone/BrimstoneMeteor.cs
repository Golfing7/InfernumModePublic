using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.CalamitasClone
{
    public class BrimstoneMeteor : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Brimstone Meteor");
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 36;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            CooldownSlot = 1;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Projectile.type];

            Projectile.Opacity = Utils.GetLerpValue(30f, 42f, Time, true) * Utils.GetLerpValue(0f, 12f, Projectile.timeLeft, true);
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (Time == 30f)
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, Projectile.Center);

            Time++;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of false */ => Projectile.Opacity >= 1f;

        public override bool PreDraw(ref Color lightColor)
        {
            if (Time < 30f)
            {
                Vector2 lineDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                float lineWidth = CalamityUtils.Convert01To010(Time / 30f) * 4f + 1f;
                Main.spriteBatch.DrawLineBetter(Projectile.Center - lineDirection * 3400f, Projectile.Center + lineDirection * 3400f, Color.Red, lineWidth);
                return false;
            }

            lightColor.R = (byte)(255 * Projectile.Opacity);
            Utilities.DrawAfterimagesCentered(Projectile, lightColor, ProjectileID.Sets.TrailingMode[Projectile.type], 1);
            return false;
        }

        public override bool ShouldUpdatePosition() => Time >= 30f;

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {

        }
    }
}
