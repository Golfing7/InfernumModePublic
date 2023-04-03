using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Providence
{
    public class MoltenFire : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Molten Bolt");
            Main.projFrames[Projectile.type] = 3;
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
            Projectile.timeLeft = 300;
            Projectile.Infernum().FadesAwayWhenManuallyKilled = true;
        }

        public override void AI()
        {
            Projectile.Opacity = Utils.GetLerpValue(300f, 285f, Projectile.timeLeft, true);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.velocity.Length() < 14f)
                Projectile.velocity *= 1.02f;

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Projectile.type];
            Projectile.rotation = Projectile.velocity.ToRotation();

            Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3() * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
            lightColor.A = 128;
            if (ProvidenceBehaviorOverride.IsEnraged)
                lightColor = Color.Lerp(Color.White, Color.Cyan, 0.7f) with { A = 0 };

            Utilities.DrawAfterimagesCentered(Projectile, lightColor, ProjectileID.Sets.TrailingMode[Projectile.type]);
            return false;
        }

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of false */ => Projectile.alpha < 20;
    }
}
