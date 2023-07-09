using CalamityMod;
using CalamityMod.Events;
using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernumMode.Content.BehaviorOverrides.BossAIs.Skeletron.SkeletronHeadBehaviorOverride;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Skeletron
{
    public class SkeletronHandBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.SkeletronHand;

        public override bool PreAI(NPC npc)
        {
            float armDirection = -npc.ai[0];
            NPC owner = Main.npc[(int)npc.ai[1]];
            float animationTime = owner.ai[2];
            if (!owner.active)
            {
                npc.active = false;
                return false;
            }
            float phaseChangeCountdown = owner.Infernum().ExtraAI[0];

            npc.life = npc.lifeMax = 200;
            npc.damage = 0;
            npc.Calamity().DR = 0.4f;
            npc.dontTakeDamage = true;
            npc.timeLeft = 3600;

            // Disable natural despawning.
            npc.Infernum().DisableNaturalDespawning = true;

            if (animationTime < 200f || phaseChangeCountdown > 0f)
            {
                Vector2 destination = owner.Center + new Vector2(armDirection * 125f, -285f);
                npc.SimpleFlyMovement(npc.SafeDirectionTo(destination) * 17f, 0.3f);

                npc.rotation = npc.AngleTo(destination - Vector2.UnitY * 25f) - PiOver2;
            }
            else
            {
                SkeletronAttackType ownerAttackState = (SkeletronAttackType)(int)owner.ai[0];
                float attackTimer = owner.ai[1];
                Player target = Main.player[owner.target];

                switch (ownerAttackState)
                {
                    case SkeletronAttackType.HoverSkulls:
                    case SkeletronAttackType.DownwardAcceleratingSkulls:
                        Vector2 destination = owner.Center + new Vector2(armDirection * 600f, 950f);
                        if (npc.Center.Y > destination.Y)
                        {
                            if (npc.velocity.Y > 0f)
                                npc.velocity.Y *= 0.92f;
                            npc.velocity.Y -= 0.45f;
                            if (npc.velocity.Y > 1.5f)
                                npc.velocity.Y = 1.5f;
                        }
                        else if (npc.Center.Y < destination.Y)
                        {
                            if (npc.velocity.Y < 0f)
                                npc.velocity.Y *= 0.92f;
                            npc.velocity.Y += 0.45f;
                            if (npc.velocity.Y < -1.5f)
                                npc.velocity.Y = -1.5f;
                        }

                        if (npc.Center.X > destination.X)
                        {
                            if (npc.velocity.X > 0f)
                                npc.velocity.X *= 0.92f;
                            npc.velocity.X -= 0.6f;
                            if (npc.velocity.X > 3.5f)
                                npc.velocity.X = 3.5f;
                        }

                        if (npc.Center.X < destination.X)
                        {
                            if (npc.velocity.X < 0f)
                                npc.velocity.X *= 0.92f;
                            npc.velocity.X += 0.6f;
                            if (npc.velocity.X < -3.5f)
                                npc.velocity.X = -3.5f;
                        }

                        if (npc.WithinRange(destination, 100f) && attackTimer % 5f == 4f && attackTimer > 50f && owner.life < owner.lifeMax * 0.825f)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, npc.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Vector2 flameShootVelocity = (TwoPi * i / 4f).ToRotationVector2().RotatedByRandom(0.1f) * Main.rand.NextFloat(18f, 24f);

                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        Utilities.NewProjectileBetter(npc.Center, flameShootVelocity, ModContent.ProjectileType<ShadowflameFireball>(), ShadowflameFireballArenaDamage, 0f);
                                }
                            }
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient && ownerAttackState == SkeletronAttackType.HoverSkulls &&
                            npc.WithinRange(destination, 100f) && attackTimer % 50f == 49f && attackTimer > 90f)
                        {
                            Vector2 flameShootVelocity = npc.SafeDirectionTo(target.Center) * 13f;
                            Utilities.NewProjectileBetter(npc.Center, flameShootVelocity, ModContent.ProjectileType<ShadowflameFireball>(), ShadowflameFireballDamage, 0f);
                        }
                        break;
                    case SkeletronAttackType.HandWaves:
                        Vector2 idealPosition = owner.Center + new Vector2(armDirection * 200f, -230f);

                        bool facingPlayer = armDirection == (target.Center.X > npc.Center.X).ToDirectionInt();
                        float adjustedTimer = attackTimer % 160f;
                        int waveCounter = (int)(attackTimer / 160) % 2;
                        if (facingPlayer && adjustedTimer > 65f && adjustedTimer < 140f)
                        {
                            float swipeAngularOffset = Lerp(-0.6f, 1.22f, Utils.GetLerpValue(90f, 140f, adjustedTimer, true));
                            idealPosition = owner.Center + owner.SafeDirectionTo(target.Center).RotatedBy(swipeAngularOffset) * 250f;
                        }

                        npc.Center = Vector2.Lerp(npc.Center, idealPosition, 0.08f);
                        npc.Center = npc.Center.MoveTowards(idealPosition, 8f);
                        npc.velocity = Vector2.Zero;

                        int shootDelay = waveCounter == 1 ? 2 : 3;
                        if (Main.netMode != NetmodeID.MultiplayerClient && facingPlayer && adjustedTimer > 90f && adjustedTimer < 140f && adjustedTimer % 4f == shootDelay)
                        {
                            Vector2 skullSpawnPosition = npc.Center;
                            Vector2 skullShootVelocity = (skullSpawnPosition - owner.Center).SafeNormalize(Vector2.UnitY) * 5.6f;
                            if (BossRushEvent.BossRushActive)
                                skullShootVelocity *= 2f;

                            skullSpawnPosition += skullShootVelocity * 4f;
                            Utilities.NewProjectileBetter(skullSpawnPosition, skullShootVelocity, ModContent.ProjectileType<NonHomingSkull>(), SkullDamage, 0f);
                        }

                        break;
                    case SkeletronAttackType.SpinCharge:
                    case SkeletronAttackType.Phase1Fakeout:
                        destination = owner.Center + new Vector2(armDirection * 200f, -230f);
                        npc.Center = Vector2.Lerp(npc.Center, destination, 0.035f);
                        npc.Center = npc.Center.MoveTowards(destination, 5f);
                        break;
                    case SkeletronAttackType.HandShadowflameBurst:
                        destination = owner.Center + new Vector2(armDirection * 540f, 360f);
                        npc.Center = Vector2.Lerp(npc.Center, destination, 0.1f);
                        npc.Center = npc.Center.MoveTowards(destination, 12f);

                        adjustedTimer = attackTimer % 210f;
                        if (adjustedTimer > 50f && adjustedTimer < 180f && adjustedTimer % 45f == 44f && attackTimer < 360f)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, npc.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float offsetAngle = Main.rand.NextFloat(TwoPi);
                                for (int i = 0; i < 8; i++)
                                {
                                    Vector2 flameShootVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(TwoPi * i / 8f) * 8.5f;
                                    if (BossRushEvent.BossRushActive)
                                        flameShootVelocity *= 3f;
                                    Utilities.NewProjectileBetter(npc.Center, flameShootVelocity, ModContent.ProjectileType<ShadowflameFireball>(), 100, 0f);
                                    Utilities.NewProjectileBetter(npc.Center, flameShootVelocity.RotatedBy(offsetAngle) * 0.6f, ModContent.ProjectileType<ShadowflameFireball>(), ShadowflameFireballDamage, 0f);
                                }
                            }
                        }
                        break;
                    case SkeletronAttackType.HandShadowflameWaves:
                        int attackDelay = 75;
                        adjustedTimer = (attackTimer - attackDelay) % 150f;
                        bool shouldAttack = (int)((attackTimer - attackDelay) / 150) % 2 == (armDirection == 1f).ToInt() && attackTimer > attackDelay;
                        destination = owner.Center + new Vector2(armDirection * 620f, 420f);

                        if (attackTimer > attackDelay)
                            destination.Y += Sin((attackTimer - attackDelay) * Pi / 50f) * shouldAttack.ToInt() * 250f;

                        npc.Center = Vector2.Lerp(npc.Center, destination, 0.065f);
                        npc.Center = npc.Center.MoveTowards(destination, 5f);

                        if (attackTimer < 590f && adjustedTimer > 45f && adjustedTimer % 7f == 6f && shouldAttack)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, npc.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    Vector2 flameShootVelocity = Vector2.UnitX * Math.Sign(npc.SafeDirectionTo(target.Center).X) * 13f;
                                    if (BossRushEvent.BossRushActive)
                                        flameShootVelocity *= 2f;
                                    Utilities.NewProjectileBetter(npc.Center, flameShootVelocity, ModContent.ProjectileType<ShadowflameFireball>(), ShadowflameFireballDamage, 0f);
                                }
                            }
                        }
                        break;
                }

                if (npc.velocity.Length() < owner.velocity.Length() * 0.4f)
                    npc.velocity = owner.velocity * 0.4f;

                npc.rotation = npc.AngleFrom(owner.Center) - PiOver2;
            }

            return false;
        }
    }
}
