﻿using CalamityMod;
using CalamityMod.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.DoG
{
    public class DoGDeathInfernum : ModProjectile
    {
        public Vector2 OldVelocity;

        public ref float TelegraphDelay => ref Projectile.ai[0];

        public const float TelegraphTotalTime = 150f;
        public const float TelegraphFadeTime = 30f;
        public const float TelegraphWidth = 4200f;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Death Beam");
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 780;
            Projectile.Calamity().DealsDefenseDamage = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.MaxUpdates);
            writer.WriteVector2(OldVelocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.MaxUpdates = reader.ReadInt32();
            OldVelocity = reader.ReadVector2();
        }

        public override void AI()
        {
            // Determine the relative opacities for each player based on their distance.
            // This has a lower bound of 0.35 to prevent the laser from going completely invisible and players getting hit by cheap shots.
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.netUpdate = true;
            }
            // Fade in after telegraphs have faded.
            if (TelegraphDelay > TelegraphTotalTime)
            {
                if (Projectile.alpha > 0)
                {
                    Projectile.alpha -= 12;
                }
                if (Projectile.alpha < 0)
                {
                    Projectile.alpha = 0;
                }
                // If an old velocity is in reserve, set the true velocity to it and make it as "taken" by setting it to <0,0>
                if (OldVelocity != Vector2.Zero)
                {
                    Projectile.velocity = OldVelocity * (BossRushEvent.BossRushActive ? 1.25f : 1f);
                    OldVelocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
                Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
            }
            // Otherwise, be sure to save the velocity the projectile started with. It will be set again when the telegraph is over.
            else if (OldVelocity == Vector2.Zero)
            {
                OldVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
                Projectile.rotation = OldVelocity.ToRotation() + PiOver2;
            }
            TelegraphDelay++;
        }

        public override bool CanHitPlayer(Player target) => TelegraphDelay > TelegraphTotalTime;

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, Projectile.alpha);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (TelegraphDelay >= TelegraphTotalTime)
                return true;

            Texture2D laserTelegraph = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/LaserWallTelegraphBeam").Value;

            float yScale = 2f;
            if (TelegraphDelay < TelegraphFadeTime)
                yScale = Lerp(0f, 2f, TelegraphDelay / TelegraphFadeTime);
            if (TelegraphDelay > TelegraphTotalTime - TelegraphFadeTime)
                yScale = Lerp(2f, 0f, (TelegraphDelay - (TelegraphTotalTime - TelegraphFadeTime)) / TelegraphFadeTime);

            Vector2 scaleInner = new(TelegraphWidth / laserTelegraph.Width, yScale);
            Vector2 origin = laserTelegraph.Size() * new Vector2(0f, 0.5f);
            Vector2 scaleOuter = scaleInner * new Vector2(1f, 1.6f);

            Color colorOuter = Color.Lerp(Color.Cyan, Color.Purple, TelegraphDelay / TelegraphTotalTime * 2f % 1f); // Iterate through purple and cyan once and then flash.
            Color colorInner = Color.Lerp(colorOuter, Color.White, 0.75f);

            colorOuter *= 0.7f;
            colorInner *= 0.7f;

            Main.EntitySpriteDraw(laserTelegraph, Projectile.Center - Main.screenPosition, null, colorInner, OldVelocity.ToRotation(), origin, scaleInner, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(laserTelegraph, Projectile.Center - Main.screenPosition, null, colorOuter, OldVelocity.ToRotation(), origin, scaleOuter, SpriteEffects.None, 0);
            return false;
        }
    }
}
