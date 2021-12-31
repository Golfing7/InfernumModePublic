﻿using InfernumMode.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.BehaviorOverrides.BossAIs.WallOfFlesh
{
    public class WallOfFleshEyeBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.WallofFleshEye;

        public override NPCOverrideContext ContentToOverride => NPCOverrideContext.NPCAI | NPCOverrideContext.NPCPreDraw;

        #region AI

        public override bool PreAI(NPC npc)
        {
            ref float attackTimer = ref npc.ai[1];

            // Disappear if the WoF body is not present.
            if (!Main.npc.IndexInRange(Main.wof))
            {
                npc.active = false;
                npc.netUpdate = true;
                return false;
            }

            Player target = Main.player[Main.npc[Main.wof].target];

            // Attack the target independently after being "killed".
            if (npc.Infernum().ExtraAI[2] == 1f)
            {
                Vector2 hoverOffset = (MathHelper.TwoPi * (npc.Infernum().ExtraAI[1] + Main.npc[Main.wof].ai[3] / 90f) / 4f).ToRotationVector2() * 360f;
                Vector2 hoverDestination = target.Center + hoverOffset;
                if (!Main.npc[Main.wof].WithinRange(target.Center, 4000f))
                    hoverDestination = Main.npc[Main.wof].Center;

                npc.SimpleFlyMovement(npc.SafeDirectionTo(hoverDestination) * 18f, 0.9f);
                npc.damage = 0;
                npc.rotation = npc.AngleTo(target.Center) + MathHelper.Pi;
                npc.dontTakeDamage = true;

                int circleHoverOffsetIndex = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (!Main.npc[i].active || Main.npc[i].type != npc.type || Main.npc[i].Infernum().ExtraAI[2] == 0f)
                        continue;

                    Main.npc[i].Infernum().ExtraAI[1] = circleHoverOffsetIndex;
                    circleHoverOffsetIndex++;
                }

                Vector2 laserShootVelocity = npc.SafeDirectionTo(target.Center) * 10f;
                Vector2 laserShootPosition = npc.Center + laserShootVelocity * 7.5f;

                // Create a dust telegraph prior to releasing lasers.
                if (Main.npc[Main.wof].ai[3] % 90f > 60f)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Dust laser = Dust.NewDustPerfect(laserShootPosition + Main.rand.NextVector2Circular(25f, 25f), 182);
                        laser.velocity = (laserShootPosition - laser.position).SafeNormalize(Vector2.UnitY) * -Main.rand.NextFloat(2f, 8f);
                        laser.noGravity = true;
                    }
                }

                if (Main.npc[Main.wof].ai[3] % 90f == 89f)
                {
                    Main.PlaySound(SoundID.Item12, npc.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int laser = Utilities.NewProjectileBetter(laserShootPosition, laserShootVelocity, ProjectileID.ScutlixLaser, 100, 0f);
                        if (Main.projectile.IndexInRange(laser))
                            Main.projectile[laser].hostile = true;
                    }
                }

                return false;
            }

            float destinationOffset = MathHelper.Clamp(npc.Distance(target.Center), 60f, 210f);
            destinationOffset += MathHelper.Lerp(0f, 215f, (float)Math.Sin(npc.whoAmI % 4f / 4f * MathHelper.Pi + attackTimer / 16f) * 0.5f + 0.5f);
            destinationOffset += npc.Distance(target.Center) * 0.1f;

            float destinationAngularOffset = MathHelper.Lerp(-1.5f, 1.5f, npc.ai[0]);
            destinationAngularOffset += (float)Math.Sin(attackTimer / 32f + npc.whoAmI % 4f / 4f * MathHelper.Pi) * 0.16f;

            // Move in sharp, sudden movements while releasing things at the target.
            Vector2 destination = Main.npc[Main.wof].Center;
            destination += Main.npc[Main.wof].velocity.SafeNormalize(Vector2.UnitX).RotatedBy(destinationAngularOffset) * destinationOffset;

            float maxSpeed = Utilities.AnyProjectiles(ModContent.ProjectileType<FireBeamWoF>()) ? 4f : 15f;

            npc.velocity = (destination - npc.Center).SafeNormalize(Vector2.Zero) * MathHelper.Min(npc.Distance(destination) * 0.5f, maxSpeed);
            if (!npc.WithinRange(Main.npc[Main.wof].Center, 750f))
                npc.Center = Main.npc[Main.wof].Center + Main.npc[Main.wof].SafeDirectionTo(npc.Center) * 750f;

            npc.spriteDirection = 1;
            npc.rotation = npc.rotation.AngleTowards(npc.AngleTo(target.Center), MathHelper.Pi * 0.1f);

            attackTimer++;

            int beamShootRate = 1600;
            if (attackTimer % beamShootRate == (beamShootRate + npc.whoAmI * 300) % beamShootRate)
                WallOfFleshMouthBehaviorOverride.PrepareFireBeam(npc, target);

            return false;
        }

        #endregion

        #region Drawing

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            ref float verticalOffsetFactor = ref npc.ai[0];

            // Don't draw any chains once free.
            if (npc.Infernum().ExtraAI[2] == 1f)
            {
                Texture2D texture = Main.npcTexture[npc.type];
                Vector2 drawPosition = npc.Center - Main.screenPosition + Vector2.UnitY * npc.gfxOffY;
                Vector2 origin = npc.frame.Size() * 0.5f;
                spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(lightColor), npc.rotation, origin, npc.scale, 0, 0f);
                return false;
            }

            if (Main.wof == -1)
                return false;

            float yStart = MathHelper.Lerp(Main.wofB, Main.wofT, verticalOffsetFactor);
            Vector2 start = new Vector2(Main.npc[Main.wof].Center.X, yStart);

            Texture2D fleshRopeTexture = Main.chain12Texture;
            void drawChainFrom(Vector2 startingPosition)
            {
                Vector2 drawPosition = startingPosition;
                float rotation = npc.AngleFrom(drawPosition) - MathHelper.PiOver2;
                while (Vector2.Distance(drawPosition, npc.Center) > 40f)
                {
                    drawPosition += npc.DirectionFrom(drawPosition) * fleshRopeTexture.Height;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 drawOffset = Vector2.UnitX.RotatedBy(rotation) * (float)Math.Cos(MathHelper.TwoPi * i / 4f) * 4f;
                        Color color = Lighting.GetColor((int)(drawPosition + drawOffset).X / 16, (int)(drawPosition + drawOffset).Y / 16);
                        spriteBatch.Draw(fleshRopeTexture, drawPosition + drawOffset - Main.screenPosition, null, color, rotation, fleshRopeTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
                    }
                }
            }

            drawChainFrom(start);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].type != NPCID.WallofFleshEye || !Main.npc[i].active || Main.npc[i].whoAmI == npc.whoAmI)
                    continue;

                // Draw order depends on index. Therefore, if the other index is greater than this one, that means it will draw
                // a chain of its own. This is done to prevent duplicates.
                if (Main.npc[i].whoAmI < npc.whoAmI)
                    drawChainFrom(Main.npc[i].Center);
            }
            return true;
        }
        #endregion
    }
}
