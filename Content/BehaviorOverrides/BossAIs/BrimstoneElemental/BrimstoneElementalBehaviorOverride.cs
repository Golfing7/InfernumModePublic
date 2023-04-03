using CalamityMod;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Sounds;
using InfernumMode.Assets.Sounds;
using InfernumMode.Content.Dusts;
using InfernumMode.Content.Projectiles;
using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using BrimmyNPC = CalamityMod.NPCs.BrimstoneElemental.BrimstoneElemental;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.BrimstoneElemental
{
    public class BrimstoneElementalBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<BrimmyNPC>();

        #region Enumerations
        public enum BrimmyAttackType
        {
            FlameTeleportBombardment,
            BrimstoneRoseBurst,
            FlameChargeSkullBlasts,
            GrimmBulletHellCopyLmao,
            EyeLaserbeams,
            DeathAnimation
        }

        public enum BrimmyFrameType
        {
            TypicalFly,
            OpenEye,
            ClosedShell
        }
        #endregion

        #region AI

        public const float BaseDR = 0.12f;

        public const float InvincibleDR = 0.99999f;

        public const float RoseCircleRadius = 1279f;

        public const float Phase2LifeRatio = 0.5f;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            Phase2LifeRatio
        };

        public override bool PreAI(NPC npc)
        {
            // Select a new target if an old one was lost.
            npc.TargetClosestIfTargetIsInvalid();

            Player target = Main.player[npc.target];

            CalamityGlobalNPC.brimstoneElemental = npc.whoAmI;

            // Reset DR and its breakability every frame.
            npc.Calamity().DR = BaseDR;
            npc.Calamity().unbreakableDR = false;

            if (!target.active || target.dead || !npc.WithinRange(target.Center, 7200f))
            {
                npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitY * -8f, 0.25f);
                if (!npc.WithinRange(target.Center, 880f))
                {
                    npc.life = 0;
                    npc.active = false;
                    npc.netUpdate = true;
                }
                return false;
            }

            float lifeRatio = npc.life / (float)npc.lifeMax;
            bool pissedOff = target.Bottom.Y < (Main.maxTilesY - 200f) * 16f && !BossRushEvent.BossRushActive;
            ref float attackType = ref npc.ai[0];
            ref float attackTimer = ref npc.ai[1];
            ref float spawnAnimationTimer = ref npc.ai[2];
            ref float frameType = ref npc.localAI[0];

            npc.dontTakeDamage = pissedOff;
            npc.Calamity().CurrentlyEnraged = npc.dontTakeDamage;

            if (spawnAnimationTimer < 240f)
            {
                DoBehavior_SpawnAnimation(npc, target, spawnAnimationTimer, ref frameType);
                spawnAnimationTimer++;
                return false;
            }

            // Spawn cinders.
            target.CreateCinderParticles(lifeRatio, new BrimstoneCinder());

            switch ((BrimmyAttackType)(int)attackType)
            {
                case BrimmyAttackType.FlameTeleportBombardment:
                    npc.damage = 0;
                    DoBehavior_FlameTeleportBombardment(npc, target, lifeRatio, pissedOff, ref attackTimer, ref frameType);
                    break;
                case BrimmyAttackType.BrimstoneRoseBurst:
                    npc.damage = npc.defDamage;
                    DoBehavior_BrimstoneRoseBurst(npc, target, pissedOff, ref attackTimer, ref frameType);
                    break;
                case BrimmyAttackType.FlameChargeSkullBlasts:
                    npc.damage = 0;
                    DoBehavior_FlameChargeSkullBlasts(npc, target, lifeRatio, pissedOff, ref attackTimer, ref frameType);
                    break;
                case BrimmyAttackType.GrimmBulletHellCopyLmao:
                    npc.damage = 0;
                    DoBehavior_CocoonBulletHell(npc, target, lifeRatio, pissedOff, ref attackTimer, ref frameType);
                    break;
                case BrimmyAttackType.EyeLaserbeams:
                    npc.damage = 0;
                    DoBehavior_EyeLaserbeams(npc, target, lifeRatio, pissedOff, ref attackTimer, ref frameType);
                    break;
                case BrimmyAttackType.DeathAnimation:
                    DoBehavior_DeathAnimation(npc, target, ref attackTimer, ref frameType);
                    break;
            }

            attackTimer++;
            return false;
        }

        public static void DoBehavior_SpawnAnimation(NPC npc, Player target, float spawnAnimationTimer, ref float frameType)
        {
            if (spawnAnimationTimer == 1f)
                npc.Center = target.Center - Vector2.UnitY * 250f;

            frameType = (int)BrimmyFrameType.ClosedShell;
            npc.velocity = Vector2.UnitY * Utils.GetLerpValue(135f, 45f, spawnAnimationTimer, true) * -4f;
            npc.Opacity = Utils.GetLerpValue(0f, 40f, spawnAnimationTimer, true);

            // Adjust sprite direction to look at the player.
            if (MathHelper.Distance(target.Center.X, npc.Center.X) > 45f)
                npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();

            int brimstoneDustCount = (int)MathHelper.Lerp(2f, 8f, npc.Opacity);
            for (int i = 0; i < brimstoneDustCount; i++)
            {
                Dust brimstoneFire = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height) * 0.5f, 267);
                brimstoneFire.color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.4f, 0.9f));
                brimstoneFire.velocity = -Vector2.UnitY * Main.rand.NextFloat(2f, 5.4f);
                brimstoneFire.scale = MathHelper.SmoothStep(0.9f, 1.56f, Utils.GetLerpValue(2f, 5.4f, brimstoneFire.velocity.Y, true));
                brimstoneFire.noGravity = true;
            }

            npc.Calamity().DR = InvincibleDR;
            npc.Calamity().unbreakableDR = true;
        }

        public static void DoBehavior_FlameTeleportBombardment(NPC npc, Player target, float lifeRatio, bool pissedOff, ref float attackTimer, ref float frameType)
        {
            int bombardCount = lifeRatio < Phase2LifeRatio ? 7 : 6;
            int bombardTime = 75;
            int fireballShootRate = lifeRatio < Phase2LifeRatio ? 6 : 9;
            int fadeOutTime = (int)MathHelper.Lerp(48f, 27f, 1f - lifeRatio);
            float skullShootSpeed = 11f;
            float horizontalTeleportOffset = MathHelper.Lerp(950f, 820f, 1f - lifeRatio);
            float verticalDestinationOffset = MathHelper.Lerp(600f, 475f, 1f - lifeRatio);
            Vector2 verticalDestination = target.Center - Vector2.UnitY * verticalDestinationOffset;
            ref float bombardCounter = ref npc.Infernum().ExtraAI[0];
            ref float attackState = ref npc.Infernum().ExtraAI[1];

            if (pissedOff)
            {
                fadeOutTime = (int)(fadeOutTime * 0.45);
                horizontalTeleportOffset *= 0.7f;
                fireballShootRate = 3;
            }
            if (BossRushEvent.BossRushActive)
            {
                skullShootSpeed *= 1.8f;
                fadeOutTime = (int)(fadeOutTime * 0.4);
                horizontalTeleportOffset *= 0.6f;
                fireballShootRate = 3;
            }

            switch ((int)attackState)
            {
                // Fade out and disappear into flames.
                case 0:
                    npc.velocity *= 0.92f;
                    npc.rotation = npc.velocity.X * 0.04f;
                    npc.Opacity = MathHelper.Clamp(npc.Opacity - 1f / fadeOutTime, 0f, 1f);

                    int brimstoneDustCount = (int)MathHelper.Lerp(2f, 8f, npc.Opacity);
                    for (int i = 0; i < brimstoneDustCount; i++)
                    {
                        Dust brimstoneFire = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height) * 0.5f, 267);
                        brimstoneFire.color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.4f, 0.9f));
                        brimstoneFire.velocity = -Vector2.UnitY * Main.rand.NextFloat(2f, 5.4f);
                        brimstoneFire.scale = MathHelper.SmoothStep(0.9f, 1.56f, Utils.GetLerpValue(2f, 5.4f, brimstoneFire.velocity.Y, true));
                        brimstoneFire.noGravity = true;
                    }

                    // Go to the next attack state and teleport once completely invisible.
                    if (npc.Opacity <= 0f)
                    {
                        Vector2 teleportOffset = Vector2.UnitX * horizontalTeleportOffset * (bombardCounter % 2f == 0f).ToDirectionInt() * Main.rand.NextFloat(0.65f, 1f);
                        attackTimer = 0f;
                        attackState++;
                        npc.Center = target.Center + teleportOffset;
                        npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();
                        npc.velocity = npc.SafeDirectionTo(verticalDestination) * npc.Distance(verticalDestination) / bombardTime;
                        npc.netUpdate = true;
                    }

                    // Use the closed shell animation.
                    frameType = (int)BrimmyFrameType.ClosedShell;
                    break;

                // Rapidly fade back in and move.
                case 1:
                    npc.Opacity = MathHelper.Clamp(npc.Opacity + 0.12f, 0f, 1f);
                    npc.rotation = npc.velocity.X * 0.04f;

                    if (attackTimer % fireballShootRate == fireballShootRate - 1f)
                    {
                        SoundEngine.PlaySound(SoundID.Item20, npc.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int skullDamage = (int)((1f - lifeRatio) * 35f) + 125;
                            float shootDelay = pissedOff || BossRushEvent.BossRushActive ? -8f : (attackTimer - bombardTime) / 5f;
                            Vector2 shootVelocity = npc.SafeDirectionTo(target.Center) * skullShootSpeed;
                            Utilities.NewProjectileBetter(npc.Center, shootVelocity, ModContent.ProjectileType<HomingBrimstoneSkull>(), skullDamage, 0f, -1, shootDelay);
                        }
                    }

                    if (attackTimer >= bombardTime)
                    {
                        attackState = 0f;
                        attackTimer = 0f;
                        bombardCounter++;

                        if (bombardCounter >= bombardCount)
                        {
                            bombardCounter = 0f;
                            SelectNextAttack(npc);
                        }

                        npc.netUpdate = true;
                    }

                    // Use the flying animation.
                    frameType = (int)BrimmyFrameType.TypicalFly;
                    break;
            }
        }

        public static void DoBehavior_BrimstoneRoseBurst(NPC npc, Player target, bool pissedOff, ref float attackTimer, ref float frameType)
        {
            // Use the flying animation.
            frameType = (int)BrimmyFrameType.TypicalFly;

            int totalRosesToSpawn = 10;
            int castingAnimationTime = 50;
            if (pissedOff || BossRushEvent.BossRushActive)
                totalRosesToSpawn += 5;

            Vector2 eyePosition = npc.Center + new Vector2(npc.spriteDirection * 20f, -70f);
            ref float attackState = ref npc.Infernum().ExtraAI[0];
            ref float roseCreationCounter = ref npc.Infernum().ExtraAI[1];
            ref float circleCenterX = ref npc.Infernum().ExtraAI[2];
            ref float circleCenterY = ref npc.Infernum().ExtraAI[3];
            Vector2 circleCenter = new(circleCenterX, circleCenterY);

            // Adjust sprite direction to look at the player.
            if (MathHelper.Distance(target.Center.X, npc.Center.X) > 45f)
                npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();
            npc.rotation = npc.velocity.X * 0.04f;

            if (circleCenterX == 0f)
            {
                circleCenterX = target.Center.X;
                circleCenterY = target.Center.Y;
                HatGirl.SayThingWhileOwnerIsAlive(target, "Stay near the center of your arena if you can. Those thorns are really good at cornering you!");
                npc.netUpdate = true;
            }

            // Hurt the player if they walk into the vines.
            else if (!target.WithinRange(circleCenter, RoseCircleRadius - 8f))
            {
                int roseDamage = Main.rand.Next(120, 135);
                target.Center = circleCenter + (target.Center - circleCenter).SafeNormalize(Vector2.Zero) * (RoseCircleRadius - 10f);
                target.Hurt(PlayerDeathReason.ByCustomReason($"{target.name} was violently pricked by roses."), roseDamage, 0);
            }

            switch ((int)attackState)
            {
                case 0:
                    npc.SimpleFlyMovement(npc.SafeDirectionTo(target.Center) * new Vector2(8f, 4f), 0.13f);
                    if (attackTimer >= 125f || npc.WithinRange(target.Center, 105f))
                    {
                        attackTimer = 0f;
                        attackState = 1f;
                        roseCreationCounter++;

                        if (roseCreationCounter >= 4f)
                        {
                            roseCreationCounter = 0f;
                            circleCenterX = 0f;
                            circleCenterY = 0f;
                            SelectNextAttack(npc);
                        }
                        npc.netUpdate = true;
                    }
                    break;
                case 1:
                    npc.velocity = npc.velocity.MoveTowards(Vector2.Zero, 0.4f) * 0.96f;

                    // Create the charge dust.
                    int dustCount = attackTimer >= 35f ? 2 : 1;
                    for (int i = 0; i < dustCount; i++)
                    {
                        float dustScale = i % 2 == 1 ? 1.65f : 0.8f;

                        Vector2 dustSpawnPosition = eyePosition + Main.rand.NextVector2CircularEdge(16f, 16f);
                        Dust chargeDust = Dust.NewDustPerfect(dustSpawnPosition, 267);
                        chargeDust.velocity = (eyePosition - dustSpawnPosition).SafeNormalize(Vector2.UnitY) * (dustCount == 2 ? 3.5f : 2.8f);
                        chargeDust.color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.222f, 0.777f));
                        chargeDust.scale = dustScale;
                        chargeDust.noGravity = true;
                    }

                    if (attackTimer >= castingAnimationTime)
                    {
                        SoundEngine.PlaySound(SoundID.Item72, npc.Center);

                        for (int i = 0; i < totalRosesToSpawn; i++)
                        {
                            // Generate sets of points where the roses will be spawned.
                            Vector2 roseSpawnPosition = circleCenter + Main.rand.NextVector2Unit() * Main.rand.NextFloat(150f, 920f);

                            Dust.QuickDustLine(eyePosition, roseSpawnPosition, 45f, Color.Red);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Utilities.NewProjectileBetter(roseSpawnPosition, Vector2.Zero, ModContent.ProjectileType<BrimstoneRose>(), 0, 0f, -1, 0f, pissedOff.ToInt());
                        }

                        attackTimer = 0f;
                        attackState = 0f;
                        npc.netUpdate = true;
                    }
                    break;
            }
        }

        public static void DoBehavior_FlameChargeSkullBlasts(NPC npc, Player target, float lifeRatio, bool pissedOff, ref float attackTimer, ref float frameType)
        {
            // Use the open eye fly animation.
            frameType = (int)BrimmyFrameType.OpenEye;

            int chargeTime = (int)MathHelper.Lerp(150f, 80f, 1f - lifeRatio);
            int totalBursts = 4;
            int burstRate = 45;
            Vector2 eyePosition = npc.Center + new Vector2(npc.spriteDirection * 20f, -70f);
            ref float attackState = ref npc.Infernum().ExtraAI[0];
            ref float burstCounter = ref npc.Infernum().ExtraAI[1];

            switch ((int)attackState)
            {
                // Teleport near the target and immediately go to the next attack state.
                case 0:
                    attackState++;
                    attackTimer = 0f;
                    Vector2 teleportDestination = target.Center - Vector2.UnitY * 350f;
                    CreateTeleportTelegraph(npc.Center, teleportDestination, 250);
                    npc.Center = target.Center - Vector2.UnitY * 325f;
                    npc.netUpdate = true;
                    break;

                // Charge prior to firing.
                case 1:
                    // Create the charge dust.
                    npc.velocity *= 0.5f;
                    npc.rotation = npc.velocity.X * 0.04f;

                    int dustCount = attackTimer >= 100f ? 2 : 1;
                    for (int i = 0; i < dustCount; i++)
                    {
                        float dustScale = i % 2 == 1 ? 1.5f : 0.8f;

                        Vector2 dustSpawnPosition = eyePosition + Main.rand.NextVector2Unit() * Main.rand.NextFloat(45f, 60f);
                        Dust chargeDust = Dust.NewDustPerfect(dustSpawnPosition, ModContent.DustType<BrimstoneCinderDust>());
                        chargeDust.velocity = (eyePosition - dustSpawnPosition).SafeNormalize(Vector2.UnitY) * (dustCount == 2 ? 7f : 5.6f);
                        chargeDust.scale = dustScale;
                        chargeDust.noGravity = true;
                    }

                    // Adjust sprite direction to look at the player.
                    if (MathHelper.Distance(target.Center.X, npc.Center.X) > 45f)
                        npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();

                    // Explode and go to the next attack state once done charging.
                    if (attackTimer >= chargeTime)
                    {
                        // Look at the player.
                        npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();

                        Utilities.CreateGenericDustExplosion(eyePosition, ModContent.DustType<BrimstoneCinderDust>(), 25, 7f, 1.3f);
                        Utilities.CreateGenericDustExplosion(eyePosition, (int)CalamityDusts.Brimstone, 35, 5.5f, 1.4f);

                        attackState++;
                        attackTimer = 0f;
                    }
                    break;

                // Release bursts of skulls and hellblasts in bursts.
                case 2:
                    if (attackTimer >= burstRate)
                    {
                        SoundEngine.PlaySound(CommonCalamitySounds.FlareSound, npc.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            // Release waving skulls.
                            int skullCount = (int)MathHelper.Lerp(4f, 9f, 1f - lifeRatio);
                            int skullDamage = (int)((1f - lifeRatio) * 35f) + 125;
                            float skullShootSpeed = 10f;

                            if (pissedOff)
                                skullCount += 8;
                            if (BossRushEvent.BossRushActive)
                            {
                                skullCount += 8;
                                skullShootSpeed *= 1.4f;
                            }

                            for (int i = 0; i < skullCount; i++)
                            {
                                float offsetAngle = MathHelper.Lerp(-0.74f, 0.74f, i / (float)(skullCount - 1f));
                                Vector2 shootVelocity = (target.Center - eyePosition).SafeNormalize(Vector2.UnitY).RotatedBy(offsetAngle) * skullShootSpeed;
                                Utilities.NewProjectileBetter(eyePosition, shootVelocity, ModContent.ProjectileType<BrimstoneSkull>(), skullDamage, 0f);
                            }

                            // And hellblasts.
                            for (int i = 0; i < skullCount / 2 + 2; i++)
                            {
                                float offsetAngle = Main.rand.NextFloat(-0.89f, 0.89f);
                                Vector2 shootVelocity = (target.Center - eyePosition).SafeNormalize(Vector2.UnitY).RotatedBy(offsetAngle) * Main.rand.NextFloat(0.8f, 1.6f);
                                Utilities.NewProjectileBetter(eyePosition, shootVelocity, ModContent.ProjectileType<BrimstoneHellblast>(), skullDamage, 0f);
                            }
                        }

                        npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();
                        attackTimer = 0f;
                        burstCounter++;

                        if (burstCounter >= totalBursts)
                        {
                            attackTimer = 0f;
                            attackState = 3f;
                            burstCounter = 0f;
                        }
                    }
                    break;

                // Sit in place for a bit prior to going to the next attack.
                case 3:
                    npc.velocity *= 0.95f;
                    npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();

                    if (attackTimer > 125f)
                    {
                        attackState = 0f;
                        SelectNextAttack(npc);
                    }
                    break;
            }
        }

        public static void DoBehavior_CocoonBulletHell(NPC npc, Player target, float lifeRatio, bool pissedOff, ref float attackTimer, ref float frameType)
        {
            // Use the cocoon animation.
            frameType = (int)BrimmyFrameType.ClosedShell;
            ref float shootTimer = ref npc.Infernum().ExtraAI[0];

            int fireReleaseRate = lifeRatio < Phase2LifeRatio ? 5 : 7;
            int bulletHellShootDelay = 145;
            int bulletHellTime = 520;
            float shootSpeedFactor = 1f;
            if (pissedOff)
                fireReleaseRate = 2;
            if (BossRushEvent.BossRushActive)
            {
                fireReleaseRate = 2;
                shootSpeedFactor = 1.6f;
            }

            // Rapidly slow down.
            npc.velocity *= 0.8f;
            npc.rotation = npc.velocity.X * 0.04f;

            npc.Calamity().DR = 0.85f;
            npc.Calamity().unbreakableDR = true;

            // Teleport below the player.
            if (attackTimer == 5f)
            {
                Vector2 teleportDestination = target.Center - Vector2.UnitY * 300f;

                CreateTeleportTelegraph(npc.Center, teleportDestination, 250);
                npc.Center = teleportDestination;
            }

            // Have a small delay prior to the bullet hell to allow the target to prepare.
            if (attackTimer < bulletHellShootDelay)
                return;

            // Release the bullet hell cinders.
            shootTimer++;
            if (shootTimer > fireReleaseRate && attackTimer < bulletHellTime + 60f)
            {
                SoundEngine.PlaySound(SoundID.Item100, npc.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int fireDamage = 130;
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 shootDirection = Main.rand.NextVector2Unit();
                        Vector2 fireSpawnPosition = npc.Center + npc.Size * shootDirection * 0.45f;
                        Vector2 fireShootVelocity = shootDirection * shootSpeedFactor * 12f;
                        Utilities.NewProjectileBetter(fireSpawnPosition, fireShootVelocity, ModContent.ProjectileType<BrimstoneFireball>(), fireDamage, 0f);
                    }

                    // Sometimes release predictive darts.
                    if (Main.rand.NextBool(8))
                    {
                        int dartCount = 3;
                        for (int i = 0; i < dartCount; i++)
                        {
                            float offsetAngle = MathHelper.Lerp(-0.64f, 0.64f, i / (float)(dartCount - 1f));
                            Vector2 dartVelocity = npc.SafeDirectionTo(target.Center + target.velocity * 25f).RotatedBy(offsetAngle) * shootSpeedFactor * 14f;
                            Utilities.NewProjectileBetter(npc.Center, dartVelocity, ModContent.ProjectileType<BrimstonePetal2>(), fireDamage, 0f);
                        }
                    }
                }
                shootTimer = 0f;
            }

            if (attackTimer > bulletHellTime + 120f)
            {
                shootTimer = 0f;
                SelectNextAttack(npc);
            }
        }

        public static void DoBehavior_EyeLaserbeams(NPC npc, Player target, float lifeRatio, bool pissedOff, ref float attackTimer, ref float frameType)
        {
            // Use the open eye fly animation.
            frameType = (int)BrimmyFrameType.OpenEye;
            ref float telegraphDirectionX = ref npc.Infernum().ExtraAI[0];
            ref float telegraphDirectionY = ref npc.Infernum().ExtraAI[1];

            int hoverTime = (int)MathHelper.Lerp(105f, 200f, 1f - lifeRatio);
            int totalLaserbeamBursts = 2;
            Vector2 eyePosition = npc.Center + new Vector2(npc.spriteDirection * 20f, -70f);

            if (pissedOff || BossRushEvent.BossRushActive)
                hoverTime -= 25;

            ref float attackState = ref npc.Infernum().ExtraAI[2];

            switch ((int)attackState)
            {
                // Teleport near the target and immediately go to the next attack state.
                // This also deletes any leftover laserbeams.
                case 0:
                    attackState++;
                    attackTimer = 0f;

                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<BrimstoneDeathray>())
                            Main.projectile[i].active = false;
                    }

                    Vector2 teleportDestination = target.Center - Main.rand.NextVector2CircularEdge(300f, 300f);
                    CreateTeleportTelegraph(npc.Center, teleportDestination, 250);
                    npc.Center = teleportDestination;
                    npc.netUpdate = true;
                    break;

                // Hover near the player for a bit and create charge dust.
                // This serves as a sort of telegraph as well as a way for Brimmy to redirect.
                case 1:
                    Vector2 hoverDestination = target.Center + new Vector2((target.Center.X < npc.Center.X).ToDirectionInt() * 300f, -180f);
                    Vector2 minVelocity = npc.SafeDirectionTo(target.Center) * MathHelper.Min(8f, npc.Distance(hoverDestination));
                    Vector2 maxVelocity = ((hoverDestination - npc.Center) / 30f).ClampMagnitude(0f, 36f);

                    // Hover more quickly if far from the destination.
                    npc.velocity = Vector2.Lerp(minVelocity, maxVelocity, Utils.GetLerpValue(150f, 425f, npc.Distance(hoverDestination), true));
                    npc.rotation = npc.velocity.X * 0.04f;

                    // Create the charge dust.
                    int dustCount = attackTimer >= 100f ? 2 : 1;
                    for (int i = 0; i < dustCount; i++)
                    {
                        float dustScale = i % 2 == 1 ? 1.5f : 0.8f;

                        Vector2 dustSpawnPosition = eyePosition + Main.rand.NextVector2Unit() * Main.rand.NextFloat(45f, 60f);
                        Dust chargeDust = Dust.NewDustPerfect(dustSpawnPosition, ModContent.DustType<BrimstoneCinderDust>());
                        chargeDust.velocity = (eyePosition - dustSpawnPosition).SafeNormalize(Vector2.UnitY) * (dustCount == 2 ? 7f : 5.6f) + npc.velocity;
                        chargeDust.scale = dustScale;
                        chargeDust.noGravity = true;
                    }

                    // Go to the next attack state after hovering for a small amount of time.
                    if (attackTimer >= hoverTime)
                    {
                        attackState++;
                        attackTimer = 0f;
                        npc.netUpdate = true;
                    }
                    break;

                // Sit for a short amount of time and release laserbeams.
                case 2:
                    float wrappedTime = attackTimer % 210f;
                    Vector2 deathrayDirection = new Vector2(telegraphDirectionX, telegraphDirectionY).SafeNormalize(Vector2.UnitX * npc.spriteDirection);

                    if (wrappedTime == 1f)
                    {
                        HatGirl.SayThingWhileOwnerIsAlive(target, "The beam predicts your position, try baiting it into shooting away from you!");
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            deathrayDirection = npc.SafeDirectionTo(target.Center);
                            Utilities.NewProjectileBetter(eyePosition, deathrayDirection, ModContent.ProjectileType<BrimstoneTelegraphRay>(), 0, 0f, -1, 0f, npc.whoAmI);
                        }
                    }

                    if (wrappedTime < 35f)
                        npc.velocity *= 0.9f;
                    else
                    {
                        npc.velocity = Vector2.Zero;

                        int laserbeamDamage = 215;
                        if (wrappedTime % 120f == 119f)
                        {
                            SoundEngine.PlaySound(SoundID.Item74, npc.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Utilities.NewProjectileBetter(eyePosition, deathrayDirection, ModContent.ProjectileType<BrimstoneDeathray>(), laserbeamDamage, 0f, -1, 0f, npc.whoAmI);
                        }

                        IEnumerable<Projectile> rays = Utilities.AllProjectilesByID(ModContent.ProjectileType<BrimstoneDeathray>());
                        if (Main.netMode != NetmodeID.MultiplayerClient && rays.Any() && Main.rand.NextBool(2))
                        {
                            Projectile deathray = rays.First();
                            Utilities.NewProjectileBetter(npc.Center, -deathray.velocity.RotatedByRandom(MathHelper.PiOver2) * 18f, ModContent.ProjectileType<BrimstonePetal2>(), 150, 0f);
                        }

                        if (attackTimer >= (totalLaserbeamBursts - 0.02f) * 210f)
                        {
                            attackState = 0f;
                            telegraphDirectionX = 0f;
                            telegraphDirectionY = 0f;
                            SelectNextAttack(npc);
                            return;
                        }
                    }

                    if (wrappedTime < 120f)
                    {
                        Vector2 idealDirection = (target.Center + target.velocity * 45f - eyePosition).SafeNormalize(Vector2.UnitY);
                        deathrayDirection = Vector2.Lerp(deathrayDirection, idealDirection, 0.02f).MoveTowards(idealDirection, 0.012f);
                        telegraphDirectionX = deathrayDirection.X;
                        telegraphDirectionY = deathrayDirection.Y;
                    }

                    npc.rotation = npc.velocity.X * 0.04f;
                    break;
            }

            npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();
        }

        public static void DoBehavior_DeathAnimation(NPC npc, Player target, ref float attackTimer, ref float frameType)
        {
            int flameCount = 6;
            int flameSummonDelay = 96;
            int fireSpinTime = 180;
            int fireConsumeTime = 108;
            int shellTime = 30;
            ref float flameOffsetAngle = ref npc.Infernum().ExtraAI[0];
            ref float stoneFormInterpolant = ref npc.Infernum().ExtraAI[1];
            ref float flameOffsetRadius = ref npc.Infernum().ExtraAI[2];
            ref float extraFramesNeededToAnimate = ref npc.Infernum().ExtraAI[3];
            ref float flameScaleFactor = ref npc.Infernum().ExtraAI[4];

            // Disable damage.
            npc.damage = 0;
            npc.dontTakeDamage = true;

            // Make the boss HP bar disappear.
            npc.Calamity().ShouldCloseHPBar = true;

            // Make music stop abruptly.
            if (!BossRushEvent.BossRushActive)
            {
                npc.ModNPC.SceneEffectPriority = SceneEffectPriority.BossHigh;
                npc.ModNPC.Music = 0;
            }

            // Don't rotate.
            npc.rotation = 0f;

            // Teleport above the player on the first frame.
            if (attackTimer == 1f)
            {
                npc.Center = target.Center - Vector2.UnitY * 300f;
                npc.velocity = Vector2.Zero;
                npc.Opacity = 1f;
                npc.netUpdate = true;
                stoneFormInterpolant = 0f;

                SoundEngine.PlaySound(InfernumSoundRegistry.VassalTeleportSound, target.Center);
                Utilities.CreateShockwave(npc.Center, 2, 7, 127f, false);
            }

            // Choose frames.
            frameType = (int)BrimmyFrameType.TypicalFly;
            if (attackTimer >= flameSummonDelay + fireSpinTime + fireConsumeTime)
                frameType = (int)BrimmyFrameType.ClosedShell;

            // Look at the target shortly after teleporting.
            if (attackTimer < 32f)
                npc.spriteDirection = (target.Center.X > npc.Center.X).ToDirectionInt();

            // Manage ritual flames.
            if (attackTimer >= flameSummonDelay && attackTimer < flameSummonDelay + fireSpinTime + fireConsumeTime)
            {
                // Create flames around Brimmy on the first frame that they should appear.
                if (attackTimer == flameSummonDelay)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsySummon, target.Center);

                    flameOffsetRadius = 200f;
                    flameOffsetAngle = 0f;
                    for (int i = 0; i < flameCount; i++)
                    {
                        Color fireColor = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.1f, 0.9f));

                        for (int j = 0; j < 20; j++)
                        {
                            Vector2 fireVelocity = -Vector2.UnitY.RotatedByRandom(0.93f) * Main.rand.NextFloat(0.7f, 12f);
                            HeavySmokeParticle fire = new(npc.Center + (MathHelper.TwoPi * i / flameCount).ToRotationVector2() * flameOffsetRadius, fireVelocity, fireColor, 50, Main.rand.NextFloat(0.9f, 1.32f), 1f, 0.01f, true);
                            GeneralParticleHandler.SpawnParticle(fire);
                        }
                    }
                    npc.netUpdate = true;
                }

                // Make the flames move.
                for (int i = 0; i < flameCount; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        float fireScale = Main.rand.NextFloat(0.6f, 0.95f) * (flameScaleFactor + 1f);
                        Color fireColor = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.56f, 0.9f));
                        fireColor = Color.Lerp(fireColor, Color.HotPink, (float)Math.Pow(Main.rand.NextFloat(0.5f), 2D));

                        Vector2 fireVelocity = -Vector2.UnitY.RotatedByRandom(0.93f) * Main.rand.NextFloat(0.6f, 5.4f);
                        Vector2 fireSpawnPosition = npc.Center + (MathHelper.TwoPi * i / flameCount + flameOffsetAngle).ToRotationVector2() * flameOffsetRadius + Main.rand.NextVector2Circular(7f, 7f);
                        HeavySmokeParticle fire = new(fireSpawnPosition, fireVelocity, fireColor, 20, fireScale, 1f, 0.01f, true);
                        GeneralParticleHandler.SpawnParticle(fire);
                    }
                }
                float flameOffsetAngleAcceleration = Utils.Remap(attackTimer - flameSummonDelay, 6f, 90f, 0.002f, MathHelper.Pi / 64f);
                flameOffsetAngle += flameOffsetAngleAcceleration;

                // Make animations slow down.
                extraFramesNeededToAnimate = MathHelper.Lerp(extraFramesNeededToAnimate, 24f, 0.01f);

                // Turn to stone.
                stoneFormInterpolant = Utils.GetLerpValue(-72f, -12f, attackTimer - flameSummonDelay - fireSpinTime, true);
            }

            // Make the flames converge on Brimmy.
            if (attackTimer >= flameSummonDelay + fireSpinTime)
            {
                if (Main.netMode != NetmodeID.Server && attackTimer == flameSummonDelay + fireSpinTime)
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/BrimflameAbility") with { Volume = 1.5f }, target.Center);
                flameOffsetRadius = MathHelper.Lerp(flameOffsetRadius, 25f, 0.02f);
                flameScaleFactor = MathHelper.Lerp(flameScaleFactor, 1.75f, 0.02f);
            }

            // Drop loot and fall the ground as a lifeless shell.
            if (attackTimer >= flameSummonDelay + fireSpinTime + fireConsumeTime + shellTime)
            {
                npc.active = false;
                npc.NPCLoot();
                npc.netUpdate = true;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(npc.Center, npc.velocity, ModContent.ProjectileType<Brimrose>(), 0, 0f);
            }
        }

        public static void CreateTeleportTelegraph(Vector2 start, Vector2 end, int dustCount, bool canCreateDust = true)
        {
            if (canCreateDust)
            {
                for (int i = 0; i < 40; i++)
                {
                    Dust magic = Dust.NewDustPerfect(start + Main.rand.NextVector2Circular(50f, 50f), 264);
                    magic.velocity = -Vector2.UnitY * Main.rand.NextFloat(2f, 4f);
                    magic.color = Color.Red;
                    magic.scale = 1.4f;
                    magic.fadeIn = 0.5f;
                    magic.noGravity = true;
                    magic.noLight = true;

                    magic = Dust.CloneDust(magic);
                    magic.position = end + Main.rand.NextVector2Circular(50f, 50f);
                }
            }

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustDrawPosition = Vector2.Lerp(start, end, i / (float)dustCount);

                Dust magic = Dust.NewDustPerfect(dustDrawPosition, 267);
                magic.velocity = -Vector2.UnitY * Main.rand.NextFloat(0.2f, 0.235f);
                magic.color = Color.OrangeRed;
                magic.color.A = 0;
                magic.scale = 0.8f;
                magic.fadeIn = 1.4f;
                magic.noGravity = true;
            }
        }

        public static void SelectNextAttack(NPC npc)
        {
            // Select a new target.
            npc.TargetClosest();

            float lifeRatio = npc.life / (float)npc.lifeMax;
            List<BrimmyAttackType> possibleAttacks = new()
            {
                BrimmyAttackType.FlameChargeSkullBlasts,
                BrimmyAttackType.BrimstoneRoseBurst,
                BrimmyAttackType.BrimstoneRoseBurst,
                BrimmyAttackType.FlameTeleportBombardment,
                BrimmyAttackType.GrimmBulletHellCopyLmao
            };
            possibleAttacks.AddWithCondition(BrimmyAttackType.EyeLaserbeams, lifeRatio < Phase2LifeRatio);

            possibleAttacks.Remove((BrimmyAttackType)(int)npc.ai[0]);

            npc.ai[0] = (int)Main.rand.Next(possibleAttacks);
            npc.ai[1] = 0f;
            npc.netUpdate = true;
        }

        #endregion AI

        #region Drawing

        public override void FindFrame(NPC npc, int frameHeight)
        {
            npc.frameCounter++;
            int frameUpdateRate = 13;

            // Make the animation update rates slow down over time during the death animation.
            if (npc.ai[0] == (int)BrimmyAttackType.DeathAnimation)
                frameUpdateRate += (int)npc.Infernum().ExtraAI[3];

            switch ((BrimmyFrameType)(int)npc.localAI[0])
            {
                case BrimmyFrameType.TypicalFly:
                    if (npc.frameCounter >= frameUpdateRate)
                    {
                        npc.frame.Y += frameHeight;
                        npc.frameCounter = 0f;
                    }
                    if (npc.frame.Y >= frameHeight * 4)
                        npc.frame.Y = 0;
                    break;
                case BrimmyFrameType.OpenEye:
                    if (npc.frameCounter >= frameUpdateRate)
                    {
                        npc.frame.Y += frameHeight;
                        npc.frameCounter = 0f;
                    }
                    if (npc.frame.Y >= frameHeight * 8 || npc.frame.Y < frameHeight * 4)
                        npc.frame.Y = frameHeight * 4;
                    break;
                case BrimmyFrameType.ClosedShell:
                    if (npc.frameCounter >= frameUpdateRate - 5)
                    {
                        npc.frame.Y += frameHeight;
                        npc.frameCounter = 0f;
                    }
                    if (npc.frame.Y >= frameHeight * 12 || npc.frame.Y < frameHeight * 8)
                        npc.frame.Y = frameHeight * 8;
                    break;
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor)
        {
            BrimmyAttackType attackState = (BrimmyAttackType)(int)npc.ai[0];
            UnifiedRandom roseRNG = new(npc.whoAmI + 466920161);
            if (attackState == BrimmyAttackType.BrimstoneRoseBurst)
            {
                float circleAngle = 0f;
                Texture2D vineTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/BrimstoneElemental/CharredVine", AssetRequestMode.ImmediateLoad).Value;
                Texture2D roseTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/BrimstoneElemental/BrimstoneRoseBorder", AssetRequestMode.ImmediateLoad).Value;
                Vector2 vineOrigin = vineTexture.Size() * 0.5f;
                Vector2 roseOrigin = roseTexture.Size() * 0.5f;
                while (circleAngle < MathHelper.TwoPi)
                {
                    float vineRotation = circleAngle;
                    Vector2 vineDrawPosition = new(npc.Infernum().ExtraAI[2], npc.Infernum().ExtraAI[3]);
                    vineDrawPosition += circleAngle.ToRotationVector2() * RoseCircleRadius - Main.screenPosition;
                    Main.spriteBatch.Draw(vineTexture, vineDrawPosition, null, Color.White, vineRotation, vineOrigin, 1f, SpriteEffects.None, 0f);

                    // A benefit of using radians is that a necessary angle increment can be easily computed by the formula:
                    // theta = arc length / radius.
                    circleAngle += vineTexture.Height / RoseCircleRadius;

                    if (roseRNG.NextBool(4))
                    {
                        float roseRotation = roseRNG.NextFloat(MathHelper.TwoPi);
                        float roseScale = roseRNG.NextFloat(0.7f, 1f);
                        Vector2 rosePosition = vineDrawPosition + roseRNG.NextVector2Circular(8f, 1.25f).RotatedBy(circleAngle);
                        Main.spriteBatch.Draw(roseTexture, rosePosition, null, Color.White, roseRotation, roseOrigin, roseScale, SpriteEffects.None, 0f);
                    }
                }
            }

            float stoneFormInterpolant = npc.ai[0] == (int)BrimmyAttackType.DeathAnimation ? npc.Infernum().ExtraAI[1] : 0f;
            Texture2D texture = TextureAssets.Npc[npc.type].Value;
            Texture2D stoneTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/BrimstoneElemental/BrimstoneElementalStone").Value;
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Vector2 origin = npc.frame.Size() * 0.5f;
            SpriteEffects direction = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.spriteBatch.Draw(texture, drawPosition, npc.frame, npc.GetAlpha(lightColor) * (1f - stoneFormInterpolant), npc.rotation, origin, npc.scale, direction, 0f);
            Main.spriteBatch.Draw(stoneTexture, drawPosition, npc.frame, npc.GetAlpha(lightColor) * stoneFormInterpolant, npc.rotation, origin, npc.scale, direction, 0f);

            return false;
        }
        #endregion

        #region Death Effects
        public override bool CheckDead(NPC npc)
        {
            // Just die as usual if the Brimstone Elemental is killed during the death animation. This is done so that Cheat Sheet and other butcher effects can kill her quickly.
            if (npc.ai[0] == (int)BrimmyAttackType.DeathAnimation)
                return true;

            // Clear projectiles.
            Utilities.DeleteAllProjectiles(true, ModContent.ProjectileType<BrimstoneDeathray>(), ModContent.ProjectileType<BrimstoneFireball>(), ModContent.ProjectileType<BrimstonePetal>(),
                ModContent.ProjectileType<BrimstonePetal2>(), ModContent.ProjectileType<BrimstoneRose>(), ModContent.ProjectileType<BrimstoneSkull>(), ModContent.ProjectileType<BrimstoneTelegraphRay>(),
                ModContent.ProjectileType<HomingBrimstoneSkull>());

            SelectNextAttack(npc);
            npc.ai[0] = (int)BrimmyAttackType.DeathAnimation;
            npc.life = npc.lifeMax;
            npc.active = true;
            npc.netUpdate = true;
            return false;
        }
        #endregion Death Effects
    }
}
