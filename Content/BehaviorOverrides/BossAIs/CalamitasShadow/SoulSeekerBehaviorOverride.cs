using CalamityMod.NPCs;
using CalamityMod.NPCs.CalClone;
using CalamityMod.Particles;
using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.CalamitasShadow
{
    public class SoulSeekerBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<SoulSeeker>();

        public override bool PreAI(NPC npc)
        {
            // Disappear if the shadow is not present.
            if (CalamityGlobalNPC.calamitas == -1)
            {
                npc.active = false;
                return false;
            }

            // Why were these things ever doing damage?
            npc.damage = 0;

            // Don't get attacked by homing things.
            npc.chaseable = false;

            int shootRate = 30;
            ref float hasLockedIntoPosition = ref npc.ai[2];
            ref float attackTimer = ref npc.ai[3];
            ref float flyingAway = ref npc.Infernum().ExtraAI[0];
            ref float spinRadius = ref npc.Infernum().ExtraAI[1];

            NPC calShadow = Main.npc[CalamityGlobalNPC.calamitas];
            bool phase3 = calShadow.life / (float)calShadow.lifeMax < CalamitasShadowBehaviorOverride.Phase3LifeRatio;

            // Use more thematically appropriate hit sounds.
            npc.HitSound = SoundID.DD2_KoboldHurt;
            npc.DeathSound = null;

            // Inherit the target from the shadow.
            npc.target = calShadow.target;
            Player target = Main.player[npc.target];

            // Hover near the shadow if applicable. Otherwise fly away from her.
            if (flyingAway == 0f && calShadow.ai[0] != (int)CalamitasShadowBehaviorOverride.CalShadowAttackType.BrothersPhase && !phase3)
            {
                // Spin outward.
                spinRadius = Lerp(spinRadius, 840f, 0.05f);

                Vector2 hoverDestination = calShadow.Center + (npc.ai[0] + npc.ai[1]).ToRotationVector2() * spinRadius;
                npc.Center = Vector2.Lerp(npc.Center, hoverDestination, 0.034f).MoveTowards(hoverDestination, 6f);
                if (hasLockedIntoPosition == 0f && npc.WithinRange(hoverDestination, 20f))
                {
                    hasLockedIntoPosition = 1f;
                    npc.netUpdate = true;
                }
                if (hasLockedIntoPosition == 1f)
                    npc.Center = hoverDestination;
            }
            else
            {
                // Once a flyer, always a flyer...
                flyingAway = 1f;

                npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();
                npc.velocity = Vector2.Lerp(npc.velocity, npc.SafeDirectionTo(calShadow.Center) * -30f, 0.06f);
                if (!npc.WithinRange(calShadow.Center, 3000f))
                    npc.active = false;
            }
            npc.ai[1] += ToRadians(0.6f);

            // Periodically release dark magic bolts.
            if (attackTimer % shootRate == shootRate - 1f && !npc.WithinRange(target.Center, 400f) && flyingAway == 0f)
            {
                // Release some fire mist.
                Vector2 magicVelocity = npc.SafeDirectionTo(target.Center) * Main.rand.NextFloat(9f, 10f);
                for (int i = 0; i < 8; i++)
                {
                    Color fireMistColor = Color.Lerp(Color.Red, Color.Yellow, Main.rand.NextFloat(0.66f));
                    var mist = new MediumMistParticle(npc.Center + magicVelocity * 2f + Main.rand.NextVector2Circular(10f, 10f), Vector2.Zero, fireMistColor, Color.Gray, Main.rand.NextFloat(0.6f, 1.3f), 195 - Main.rand.Next(50), 0.02f)
                    {
                        Velocity = magicVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.9f, 2.4f)
                    };
                    GeneralParticleHandler.SpawnParticle(mist);
                }

                SoundEngine.PlaySound(SoundID.Item72, npc.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(npc.Center + magicVelocity * 2f, magicVelocity, ModContent.ProjectileType<DarkMagicFlame>(), CalamitasShadowBehaviorOverride.DarkMagicFlameDamage, 0f);
            }
            attackTimer++;

            return false;
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            npc.frameCounter++;
            npc.frame.Width = 88;
            npc.frame.Height = 105;
            npc.frame.Y = (int)(npc.frameCounter / 5D + npc.whoAmI * 14) % 5 * npc.frame.Height;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/NPCs/Other/CalamitasEnchantDemon").Value;
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Vector2 origin = npc.frame.Size() * 0.5f;
            SpriteEffects direction = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(Color.White), npc.rotation, origin, npc.scale, direction, 0f);
            return false;
        }
    }
}
