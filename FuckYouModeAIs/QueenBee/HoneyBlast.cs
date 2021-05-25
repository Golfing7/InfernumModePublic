﻿using CalamityMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.FuckYouModeAIs.QueenBee
{
    public class HoneyBlast : ModProjectile
    {
        public bool Poisonous => projectile.ai[0] == 1f;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Honey Blast");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 16;
            projectile.ignoreWater = true;
            projectile.timeLeft = 300;
            projectile.scale = 1f;
            projectile.tileCollide = true;
            projectile.friendly = false;
            projectile.hostile = true;
        }

        public override void AI() => projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
            int buffToGive = Poisonous ? BuffID.Poisoned : BuffID.Honey;
            target.AddBuff(buffToGive, 240);
		}

		public override void Kill(int timeLeft)
		{
            for (int i = 0; i < 10; i++)
			{
                Dust ichor = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 170);
                ichor.velocity = Main.rand.NextVector2Circular(3f, 3f);
                ichor.scale = 0.7f;
                ichor.fadeIn = 0.7f;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color drawColor = Poisonous ? Color.Green : Color.White;
            drawColor.A = 0;

            CalamityGlobalProjectile.DrawCenteredAndAfterimage(projectile, drawColor, ProjectileID.Sets.TrailingMode[projectile.type], 3);
            return true;
        }
    }
}
