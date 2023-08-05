using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Content.Projectiles.Magic
{
    public class LingeringSandSkull : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Sand Skull");
            Main.projFrames[Projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 45;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 12;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Time++;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }

            if (Time < 40f)
            {
                if (Projectile.frame >= 4)
                    Projectile.frame = 0;
            }
            else if (Projectile.owner == Main.myPlayer && Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.Kill();

            // Produce some light.
            Lighting.AddLight(Projectile.Center, 0.36f, 0.09f, 0.09f);

            Projectile.velocity *= 0.972f;
            if (Projectile.alpha > 110)
            {
                Projectile.alpha -= 30;
                if (Projectile.alpha < 70)
                    Projectile.alpha = 70;
            }

            if (Math.Abs(Projectile.velocity.X) > 0.1f)
                Projectile.spriteDirection = -Projectile.direction;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(160, 107, 61, 127);
    }
}
