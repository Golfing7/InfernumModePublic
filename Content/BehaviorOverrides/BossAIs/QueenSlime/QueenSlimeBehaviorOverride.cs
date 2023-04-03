﻿using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using CalamityMod.NPCs.SlimeGod;
using System.Collections.Generic;
using InfernumMode.Core.OverridingSystem;
using InfernumMode.Content.BehaviorOverrides.BossAIs.Ravager;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.QueenSlime
{
    public class QueenSlimeBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.QueenSlimeBoss;

        public const float Phase2LifeRatio = 0.625f;

        public override float[] PhaseLifeRatioThresholds => new float[]
        {
            Phase2LifeRatio
        };

        #region Fields, Properties, and Enumerations
        public enum QueenSlimeAttackType
        {
            CrystalSlams,
            CrystalShatter,
            CrownDashes,
            CrystalShardBursts,
            CrownLasers,
            RepeatedSlams,
        }

        public enum QueenSlimeFrameType
        {
            HoverRedirect,
            SlamDownward,
            SlamDownwardPreparation,
            FlatState,
            JumpPreparation,
            Flying
        }

        public static Color DustColor
        {
            get
            {
                Color c1 = new(0, 160, 255);
                Color c2 = Color.Lerp(new Color(200, 200, 200), new Color(255, 80, 255), Main.rand.NextFloat());
                return Color.Lerp(c1, c2, Main.rand.NextFloat());
            }
        }
        #endregion Fields, Properties, and Enumerations

        #region AI and Behaviors
        public override bool PreAI(NPC npc)
        {
            npc.TargetClosestIfTargetIsInvalid();
            Player target = Main.player[npc.target];
            ref float attackType = ref npc.ai[0];
            ref float attackTimer = ref npc.ai[1];
            ref float crownIsAttached = ref npc.ai[2];
            ref float frameType = ref npc.localAI[0];
            ref float slamTelegraphInterpolant = ref npc.localAI[1];

            if (npc.ai[3] == 0f)
            {
                crownIsAttached = 1f;
                npc.RemoveWaterSlowness();
                npc.ai[3] = 1f;
                npc.netUpdate = true;
            }

            // Despawn if the target is dead.
            if (target.dead || !target.active)
            {
                npc.active = false;
                return false;
            }

            npc.damage = npc.defDamage;
            switch ((QueenSlimeAttackType)attackType)
            {
                case QueenSlimeAttackType.RepeatedSlams:
                    DoBehavior_RepeatedSlams(npc, target, false, ref attackTimer, ref frameType);
                    break;
                case QueenSlimeAttackType.CrystalShatter:
                    DoBehavior_CrystalShatter(npc, target, ref attackTimer, ref frameType);
                    break;
                case QueenSlimeAttackType.CrownDashes:
                    DoBehavior_CrownDashes(npc, target, ref attackTimer, ref frameType, ref crownIsAttached);
                    break;
                case QueenSlimeAttackType.CrystalShardBursts:
                    DoBehavior_CrystalShardBursts(npc, target, ref attackTimer, ref frameType);
                    break;
                case QueenSlimeAttackType.CrownLasers:
                    DoBehavior_CrownLasers(npc, target, ref attackTimer, ref frameType, ref crownIsAttached);
                    break;
                case QueenSlimeAttackType.CrystalSlams:
                    DoBehavior_RepeatedSlams(npc, target, true, ref attackTimer, ref frameType);
                    break;
            }
            slamTelegraphInterpolant = 0f;
            attackTimer++;
            return false;
        }

        public static void DoBehavior_RepeatedSlams(NPC npc, Player target, bool useCrystals, ref float attackTimer, ref float frameType)
        {
            int slamCount = 5;
            int hoverTime = 150;
            int slamDelay = 20;
            int slamTime = 180;
            int postSlamSitTime = 17;
            int gelSlamCount = 7;
            float gelShootSpeed = 9.5f;
            ref float slamCounter = ref npc.Infernum().ExtraAI[0];
            ref float readyToCollideWithTiles = ref npc.Infernum().ExtraAI[1];

            npc.noTileCollide = false;

            // Hover into position.
            if (attackTimer < hoverTime)
            {
                float hoverSpeed = MathHelper.Lerp(9f, 26f, Utils.GetLerpValue(0f, 18f, attackTimer, true));
                Vector2 hoverDestination = target.Center - Vector2.UnitY * 384f;
                npc.velocity = npc.SafeDirectionTo(hoverDestination) * MathHelper.Min(npc.Distance(hoverDestination), hoverSpeed);
                npc.rotation = npc.rotation.AngleLerp(0f, 0.1f);
                npc.noTileCollide = true;
                npc.damage = 0;

                // Stop in place when close to the hover position.
                if (npc.WithinRange(hoverDestination, 30f) && attackTimer >= 54f)
                {
                    attackTimer = hoverTime;
                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;
                }
                frameType = (int)QueenSlimeFrameType.HoverRedirect;
            }

            // Sit in place before slamming.
            else if (attackTimer < hoverTime + slamDelay)
            {
                npc.velocity = -Vector2.UnitY * Utils.GetLerpValue(0f, slamDelay - 6f, attackTimer - hoverTime, true) * 6f;
                frameType = (int)QueenSlimeFrameType.SlamDownward;
            }

            // Slam downward.
            else if (attackTimer < hoverTime + slamDelay + slamTime)
            {
                if (readyToCollideWithTiles == 0f && npc.Bottom.Y > target.Bottom.Y)
                {
                    readyToCollideWithTiles = 1f;
                    npc.netUpdate = true;
                }

                npc.noTileCollide = readyToCollideWithTiles == 0f;
                npc.velocity.X *= 0.8f;
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y + 0.9f, 0f, 10f);
                for (int i = 0; i < 5; i++)
                {
                    npc.position += npc.velocity;

                    // Slam into the ground once it's reached and create hit effects.
                    if (OnSolidGround(npc) && npc.Bottom.Y >= target.Top.Y)
                    {
                        SoundEngine.PlaySound(SlimeGodCore.ExitSound, npc.Center);

                        // Create a shockwave and bursts of gel that accelerate.
                        if (!useCrystals)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Utilities.NewProjectileBetter(npc.Bottom, Vector2.Zero, ModContent.ProjectileType<StompShockwave>(), 135, 0f);
                                for (int j = 0; j < gelSlamCount; j++)
                                {
                                    float offsetAngle = MathHelper.Lerp(-0.91f, 0.91f, j / (float)(gelSlamCount - 1f));
                                    Vector2 gelSpawnPosition = npc.Center;
                                    Vector2 gelShootVelocity = (target.Center - gelSpawnPosition).SafeNormalize(Vector2.UnitY).RotatedBy(offsetAngle) * gelShootSpeed;
                                    Utilities.NewProjectileBetter(gelSpawnPosition, gelShootVelocity, ModContent.ProjectileType<AcceleratingGel>(), 125, 0f);
                                }
                            }
                        }

                        // Create a circle of crystals.
                        else
                        {
                            int crystalCount = InPhase2(npc) ? 40 : 27;
                            for (int j = 0; j < crystalCount; j++)
                            {
                                float offsetAngle = MathHelper.TwoPi * j / crystalCount;
                                Vector2 crystalSpawnPosition = npc.Center + Vector2.UnitY * 20f;
                                Vector2 crystalShootVelocity = (target.Center - crystalSpawnPosition).SafeNormalize(Vector2.UnitY).RotatedBy(offsetAngle) * gelShootSpeed * 1.75f;
                                int spike = Utilities.NewProjectileBetter(crystalSpawnPosition, crystalShootVelocity, ModContent.ProjectileType<QueenSlimeCrystalSpike>(), 130, 0f);
                                if (Main.projectile.IndexInRange(spike))
                                    Main.projectile[spike].ai[1] = -2f;
                            }
                        }

                        npc.velocity = Vector2.Zero;
                        attackTimer = hoverTime + slamDelay + slamTime;
                        npc.netUpdate = true;
                        break;
                    }
                }

                frameType = (int)QueenSlimeFrameType.SlamDownward;
            }

            // Sit in place after slamming.
            else
            {
                while (Collision.SolidCollision(npc.Bottom, 1, 1) && !npc.noTileCollide)
                    npc.position.Y--;
                npc.position = (npc.position / 16f).Floor() * 16f;
                if (!OnSolidGround(npc))
                    npc.position.Y += 16f;

                frameType = (int)QueenSlimeFrameType.FlatState;

                if (attackTimer >= hoverTime + slamDelay + slamTime + postSlamSitTime)
                {
                    npc.noTileCollide = false;
                    readyToCollideWithTiles = 0f;

                    slamCounter++;
                    if (slamCounter >= slamCount)
                        SelectNextAttack(npc);

                    attackTimer = 0f;
                    npc.netUpdate = true;
                }
            }
        }

        public static void DoBehavior_CrystalShatter(NPC npc, Player target, ref float attackTimer, ref float frameType)
        {
            int crystalCreationCount = 5;
            int crystalCount = 9;
            int jumpDelay = 15;
            int crystalShatterTime = 80;
            float fallSpeed = 0.8f;

            if (InPhase2(npc))
            {
                crystalCount += 2;
                fallSpeed += 0.225f;
            }

            ref float crystalCreationCounter = ref npc.Infernum().ExtraAI[0];

            // Sit and prepare to jump.
            if (attackTimer != jumpDelay)
            {
                npc.velocity.X *= 0.98f;
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y + fallSpeed, -21f, 15.99f);

                if (attackTimer < jumpDelay)
                {
                    if (!OnSolidGround(npc))
                        attackTimer = 0f;
                    frameType = (int)QueenSlimeFrameType.JumpPreparation;
                }
                else
                {
                    frameType = (int)QueenSlimeFrameType.SlamDownward;
                    if (OnSolidGround(npc) && Math.Abs(npc.velocity.Y) < 2f)
                    {
                        npc.velocity.X *= 0.9f;
                        frameType = (int)QueenSlimeFrameType.FlatState;
                    }
                }
            }

            // Jump and create crystals.
            if (attackTimer == jumpDelay)
            {
                crystalCreationCounter++;
                if (Main.netMode != NetmodeID.MultiplayerClient && crystalCreationCounter < crystalCreationCount)
                {
                    for (int i = 0; i < crystalCount; i++)
                    {
                        Vector2 crystalSpawnOffset = (MathHelper.TwoPi * i / crystalCount).ToRotationVector2() * 450f;
                        Utilities.NewProjectileBetter(target.Center + crystalSpawnOffset, Vector2.Zero, ModContent.ProjectileType<ShatteringCrystal>(), 125, 0f, -1, 0f, MathHelper.TwoPi * i / crystalCount);
                    }
                }

                if (crystalCreationCounter < crystalCreationCount)
                {
                    SoundEngine.PlaySound(SoundID.Item68, target.Center);
                    npc.velocity.X = Math.Sign(npc.SafeDirectionTo(target.Center).X) * 12f;
                    npc.velocity.Y = -21f;
                }
                else
                {
                    npc.noTileCollide = true;
                    SelectNextAttack(npc);
                }

                SoundEngine.PlaySound(SlimeGodCore.ExitSound, npc.Center);
                npc.netUpdate = true;
            }

            npc.noTileCollide = true;
            if (Collision.SolidCollision(npc.Bottom + Vector2.UnitY * 16f, npc.width, 1))
            {
                npc.position.Y -= 4f;
                npc.noTileCollide = false;
            }

            if (attackTimer >= jumpDelay + crystalShatterTime)
                attackTimer = 0f;
        }

        public static void DoBehavior_CrownDashes(NPC npc, Player target, ref float attackTimer, ref float frameType, ref float crownIsAttached)
        {
            int slamCount = 5;
            int hoverTime = 150;
            int slamDelay = 25;
            int slamTime = 180;
            int postSlamSitTime = 25;
            ref float hasSummonedCrown = ref npc.Infernum().ExtraAI[0];
            ref float crownShouldReturn = ref npc.Infernum().ExtraAI[1];
            ref float slamCounter = ref npc.Infernum().ExtraAI[2];
            ref float readyToCollideWithTiles = ref npc.Infernum().ExtraAI[3];

            npc.noTileCollide = false;

            // Sit in place if the crown should return.
            crownShouldReturn = (slamCounter >= slamCount).ToInt();
            if (crownShouldReturn == 1f && attackTimer < hoverTime + slamDelay + slamTime)
                attackTimer = hoverTime + slamDelay + slamTime;

            if (Main.netMode != NetmodeID.MultiplayerClient && hasSummonedCrown == 0f)
            {
                Utilities.NewProjectileBetter(CrownPosition(npc), -Vector2.UnitY * 3f, ModContent.ProjectileType<QueenSlimeCrown>(), 135, 0f);

                hasSummonedCrown = 1f;
                crownIsAttached = 0f;
                npc.netUpdate = true;
            }

            // Hover into position.
            if (attackTimer < hoverTime)
            {
                float hoverSpeed = MathHelper.Lerp(8f, 26f, Utils.GetLerpValue(0f, 18f, attackTimer, true));
                Vector2 hoverDestination = target.Center - Vector2.UnitY * 384f;
                npc.velocity = npc.SafeDirectionTo(hoverDestination) * MathHelper.Min(npc.Distance(hoverDestination), hoverSpeed);
                npc.rotation = npc.rotation.AngleLerp(0f, 0.1f);
                npc.noTileCollide = true;
                npc.damage = 0;

                // Stop in place when close to the hover position.
                if (npc.WithinRange(hoverDestination, 30f))
                {
                    attackTimer = hoverTime;
                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;
                }
                frameType = (int)QueenSlimeFrameType.HoverRedirect;
            }

            // Sit in place before slamming.
            else if (attackTimer < hoverTime + slamDelay)
            {
                npc.velocity = -Vector2.UnitY * Utils.GetLerpValue(0f, slamDelay - 6f, attackTimer - hoverTime, true) * 6f;
                frameType = (int)QueenSlimeFrameType.SlamDownward;
            }

            // Slam downward.
            else if (attackTimer < hoverTime + slamDelay + slamTime)
            {
                if (readyToCollideWithTiles == 0f && npc.Bottom.Y > target.Bottom.Y)
                {
                    readyToCollideWithTiles = 1f;
                    npc.netUpdate = true;
                }

                npc.noTileCollide = readyToCollideWithTiles == 0f;
                npc.velocity.X *= 0.8f;
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y + 0.9f, 0f, 10f);
                for (int i = 0; i < 3; i++)
                {
                    npc.position += npc.velocity;

                    // Slam into the ground once it's reached.
                    if (OnSolidGround(npc) && npc.Bottom.Y >= target.Top.Y)
                    {
                        slamCounter++;
                        SoundEngine.PlaySound(SlimeGodCore.ExitSound, npc.Center);
                        npc.velocity = Vector2.Zero;
                        attackTimer = hoverTime + slamDelay + slamTime;
                        npc.netUpdate = true;
                        break;
                    }
                }

                frameType = (int)QueenSlimeFrameType.SlamDownward;
            }

            // Sit in place after slamming.
            else
            {
                while (Collision.SolidCollision(npc.Bottom, 1, 1) && !npc.noTileCollide)
                    npc.position.Y--;
                npc.position = (npc.position / 16f).Floor() * 16f;
                if (!OnSolidGround(npc))
                    npc.position.Y += 16f;

                frameType = (int)QueenSlimeFrameType.FlatState;

                if (attackTimer >= hoverTime + slamDelay + slamTime + postSlamSitTime)
                {
                    npc.noTileCollide = false;
                    readyToCollideWithTiles = 0f;
                    attackTimer = 0f;
                    npc.netUpdate = true;
                }
            }
        }

        public static void DoBehavior_CrystalShardBursts(NPC npc, Player target, ref float attackTimer, ref float frameType)
        {
            int burstReleaseRate = 75;
            int burstCount = 8;
            int shardCount = 11;
            float shardSpread = 0.92f;
            float wrappedattackTimer = attackTimer % burstReleaseRate;

            // Hover above the target.
            Vector2 hoverDestination = target.Center - Vector2.UnitY * 450f;
            Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 25f;
            float distanceFromDestination = npc.Distance(hoverDestination);

            // Slow down before firing.
            idealVelocity *= Utils.GetLerpValue(0f, 30f, burstReleaseRate - wrappedattackTimer, true);
            if (distanceFromDestination < 80f)
                idealVelocity *= 0.65f;
            if (distanceFromDestination < 40f)
                idealVelocity = npc.velocity;
            npc.SimpleFlyMovement(idealVelocity, 0.65f);

            // Release a spread of shards at the target.
            // The spread intentionally has an opening.
            if (wrappedattackTimer == burstReleaseRate - 1f)
            {
                SoundEngine.PlaySound(SoundID.Item68, npc.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float openAreaAngle = Main.rand.NextFloatDirection() * shardSpread * 0.5f;
                    for (int i = 0; i < shardCount; i++)
                    {
                        float shootOffsetAngle = MathHelper.Lerp(-shardSpread, shardSpread, i / (float)(shardCount - 1f)) + Main.rand.NextFloatDirection() * 0.005f;
                        if (MathHelper.Distance(openAreaAngle, shootOffsetAngle) < 0.23f)
                            continue;

                        Vector2 laserShootVelocity = npc.SafeDirectionTo(target.Center).RotatedBy(shootOffsetAngle) * Main.rand.NextFloat(2.6f, 3f);
                        Utilities.NewProjectileBetter(npc.Center + laserShootVelocity * 2f, laserShootVelocity, ModContent.ProjectileType<AcceleratingGel>(), 130, 0f);
                    }
                }

                npc.netUpdate = true;
            }

            npc.rotation = npc.velocity.X * 0.025f;
            frameType = (int)QueenSlimeFrameType.Flying;

            if (attackTimer >= burstReleaseRate * burstCount)
            {
                npc.rotation = 0f;
                SelectNextAttack(npc);
            }
        }

        public static void DoBehavior_CrownLasers(NPC npc, Player target, ref float attackTimer, ref float frameType, ref float crownIsAttached)
        {
            int gelReleaseRate = 90;
            int gelReleaseCount = 8;
            ref float crownShouldReturn = ref npc.Infernum().ExtraAI[0];
            ref float hasSummonedCrown = ref npc.Infernum().ExtraAI[1];

            if (Main.netMode != NetmodeID.MultiplayerClient && hasSummonedCrown == 0f)
            {
                Utilities.NewProjectileBetter(CrownPosition(npc), -Vector2.UnitY * 3f, ModContent.ProjectileType<QueenSlimeCrown>(), 0, 0f);

                hasSummonedCrown = 1f;
                crownIsAttached = 0f;
                npc.netUpdate = true;
            }

            // Hover above the target.
            Vector2 hoverDestination = target.Center - Vector2.UnitY * 325f;
            Vector2 idealVelocity = npc.SafeDirectionTo(hoverDestination) * 24f;
            float distanceFromDestination = npc.Distance(hoverDestination);

            // Slow down before firing.
            if (distanceFromDestination < 80f)
                idealVelocity *= 0.65f;
            if (distanceFromDestination < 40f)
                idealVelocity = npc.velocity;
            npc.SimpleFlyMovement(idealVelocity, 0.65f);

            // Release bursts of gel that fall downward.
            if (Main.netMode != NetmodeID.MultiplayerClient && attackTimer % gelReleaseRate == gelReleaseRate - 1f && attackTimer < gelReleaseRate * gelReleaseCount)
            {
                for (float i = -20f; i < 20f; i += Main.rand.NextFloat(2.9f, 3.2f))
                {
                    Vector2 gelVelocity = new(i, Main.rand.NextFloat(-12f, -10f));
                    Utilities.NewProjectileBetter(npc.Center, gelVelocity, ModContent.ProjectileType<FallingGel>(), 125, 0f);
                }
            }

            npc.rotation = npc.velocity.X * 0.025f;
            frameType = (int)QueenSlimeFrameType.Flying;

            if (attackTimer >= gelReleaseRate * (gelReleaseCount - 1f) - 36f)
                crownShouldReturn = 1f;

            if (attackTimer >= gelReleaseRate * (gelReleaseCount + 3.6f))
            {
                npc.rotation = 0f;
                SelectNextAttack(npc);
            }
        }

        public static void SelectNextAttack(NPC npc)
        {
            switch ((QueenSlimeAttackType)npc.ai[0])
            {
                case QueenSlimeAttackType.CrystalSlams:
                    npc.ai[0] = (int)QueenSlimeAttackType.CrownDashes;
                    break;
                case QueenSlimeAttackType.CrownDashes:
                    npc.ai[0] = (int)QueenSlimeAttackType.CrystalShatter;
                    break;
                case QueenSlimeAttackType.CrystalShatter:
                    npc.ai[0] = InPhase2(npc) ? (int)QueenSlimeAttackType.CrystalShardBursts : (int)QueenSlimeAttackType.CrystalSlams;
                    break;
                case QueenSlimeAttackType.CrystalShardBursts:
                    npc.ai[0] = (int)QueenSlimeAttackType.CrownLasers;
                    break;
                case QueenSlimeAttackType.CrownLasers:
                    npc.ai[0] = (int)QueenSlimeAttackType.RepeatedSlams;
                    break;
                case QueenSlimeAttackType.RepeatedSlams:
                    npc.ai[0] = (int)QueenSlimeAttackType.CrystalSlams;
                    break;
            }

            for (int i = 0; i < 5; i++)
                npc.Infernum().ExtraAI[i] = 0f;
            npc.ai[1] = 0f;
            npc.netUpdate = true;
        }

        #endregion AI and Behaviors

        #region Drawing and Frames

        public static Vector2 CrownPosition(NPC npc)
        {
            Vector2 crownPosition = new(npc.Center.X, npc.Top.Y - 12f);
            float crownOffset = 0f;
            int frameHeight;
            if (npc.frame.Height == 0)
                frameHeight = 122;
            else
                frameHeight = npc.frame.Height;
            switch (npc.frame.Y / frameHeight)
            {
                case 1:
                    crownOffset -= 10f;
                    break;
                case 3:
                case 5:
                case 6:
                    crownOffset += 10f;
                    break;
                case 4:
                case 12:
                case 13:
                case 14:
                case 15:
                    crownOffset += 18f;
                    break;
                case 7:
                case 8:
                    crownOffset -= 14f;
                    break;
                case 9:
                    crownOffset -= 16f;
                    break;
                case 10:
                    crownOffset -= 4f;
                    break;
                case 11:
                    crownOffset += 20f;
                    break;
                case 20:
                    crownOffset -= 14f;
                    break;
                case 21:
                case 23:
                    crownOffset -= 18f;
                    break;
                case 22:
                    crownOffset -= 22f;
                    break;
            }

            crownPosition.Y += crownOffset;
            if (npc.rotation != 0f)
                crownPosition = crownPosition.RotatedBy(npc.rotation, npc.Bottom);
            return crownPosition;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[npc.type].Value;
            Vector2 drawBottom = npc.Bottom - Main.screenPosition;
            drawBottom.Y += 2f;
            int frame = npc.frame.Y / npc.frame.Height;
            Rectangle frameThing = texture.Frame(2, Main.npcFrameCount[npc.type], frame / Main.npcFrameCount[npc.type], frame % Main.npcFrameCount[npc.type]);
            frameThing.Inflate(0, -2);
            Vector2 origin = frameThing.Size() * new Vector2(0.5f, 1f);
            Color color = Color.Lerp(Color.White, drawColor, 0.5f);
            if (InPhase2(npc))
                Main.instance.DrawNPCDirect_QueenSlimeWings(npc, Main.spriteBatch, Main.screenPosition, npc.Center, drawColor);

            Texture2D crystalTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/QueenSlime/QueenSlimeCrystal").Value;
            Rectangle crystalFrame = crystalTexture.Frame();
            Vector2 crystalOrigin = crystalFrame.Size() * 0.5f;
            Vector2 crystalDrawPosition = npc.Center;
            float crystalDrawOffset = 0f;
            switch (frame)
            {
                case 1:
                case 6:
                    crystalDrawOffset -= 10f;
                    break;
                case 3:
                case 5:
                    crystalDrawOffset += 10f;
                    break;
                case 4:
                case 12:
                case 13:
                case 14:
                case 15:
                    crystalDrawOffset += 18f;
                    break;
                case 7:
                case 8:
                    crystalDrawOffset -= 14f;
                    break;
                case 9:
                    crystalDrawOffset -= 16f;
                    break;
                case 10:
                    crystalDrawOffset -= 18f;
                    break;
                case 11:
                    crystalDrawOffset += 20f;
                    break;
                case 20:
                    crystalDrawOffset -= 14f;
                    break;
                case 21:
                case 23:
                    crystalDrawOffset -= 18f;
                    break;
                case 22:
                    crystalDrawOffset -= 22f;
                    break;
            }

            crystalDrawPosition.Y += crystalDrawOffset;
            if (npc.rotation != 0f)
                crystalDrawPosition = crystalDrawPosition.RotatedBy(npc.rotation, npc.Bottom);

            crystalDrawPosition -= Main.screenPosition;

            spriteBatch.EnterShaderRegion();
            GameShaders.Misc["QueenSlime"].Apply();

            bool slamAttack = (QueenSlimeAttackType)npc.ai[0] is QueenSlimeAttackType.RepeatedSlams or QueenSlimeAttackType.CrownDashes;
            if (slamAttack && npc.velocity.Y != 0f)
            {
                float scaleFactor = 1f;
                if (npc.ai[2] == 1f)
                    scaleFactor = 6f;

                for (int i = 7; i >= 0; i--)
                {
                    float afterimageOpacity = 1f - i / 8f;
                    Vector2 afterimageDrawBottom = Vector2.Lerp(npc.oldPos[i], npc.oldPos[0], 0.56f) + new Vector2(npc.width * 0.5f, npc.height);
                    afterimageDrawBottom -= (npc.Bottom - Vector2.Lerp(afterimageDrawBottom, npc.Bottom, 0.75f)) * scaleFactor;
                    afterimageDrawBottom -= Main.screenPosition;
                    Color afterimageColor = color * afterimageOpacity;
                    spriteBatch.Draw(texture, afterimageDrawBottom, frameThing, afterimageColor, npc.rotation, origin, npc.scale, SpriteEffects.FlipHorizontally, 0f);
                }
            }
            spriteBatch.ExitShaderRegion();

            spriteBatch.Draw(crystalTexture, crystalDrawPosition, crystalFrame, color, npc.rotation, crystalOrigin, 1f, SpriteEffects.FlipHorizontally, 0f);
            GameShaders.Misc["QueenSlime"].Apply();

            spriteBatch.EnterShaderRegion();

            DrawData drawData = new(texture, drawBottom, frameThing, npc.GetAlpha(color), npc.rotation, origin, npc.scale, SpriteEffects.FlipHorizontally, 0);
            GameShaders.Misc["QueenSlime"].Apply(drawData);
            drawData.Draw(spriteBatch);
            spriteBatch.ExitShaderRegion();

            if (npc.ai[2] == 0f)
                return false;

            Texture2D crownTexture = ModContent.Request<Texture2D>("InfernumMode/Content/BehaviorOverrides/BossAIs/QueenSlime/QueenSlimeCrown").Value;
            frameThing = crownTexture.Frame();
            origin = frameThing.Size() * 0.5f;

            spriteBatch.Draw(crownTexture, CrownPosition(npc) - Main.screenPosition, frameThing, color, npc.rotation, origin, 1f, SpriteEffects.FlipHorizontally, 0f);
            return false;
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            npc.frame.Width = 180;

            int frame = npc.frame.Y / frameHeight;
            switch ((QueenSlimeFrameType)npc.localAI[0])
            {
                case QueenSlimeFrameType.HoverRedirect:
                    npc.frameCounter++;
                    if (npc.frameCounter >= 6)
                        frame++;
                    frame = Utils.Clamp(frame, 4, 6);
                    break;
                case QueenSlimeFrameType.SlamDownward:
                    frame = 10;
                    break;
                case QueenSlimeFrameType.SlamDownwardPreparation:
                    if (frame is < 8 or > 10)
                    {
                        frame = 8;
                        npc.frameCounter = -1.0;
                    }

                    npc.frameCounter++;
                    if (npc.frameCounter >= 8D)
                    {
                        npc.frameCounter = 0D;

                        frame++;
                        if (frame >= 10)
                            frame = 10;
                    }
                    break;
                case QueenSlimeFrameType.FlatState:
                    if (frame is < 11 or > 16)
                        npc.frameCounter = 0;

                    frame = (int)MathHelper.Lerp(16f, 11f, MathHelper.Clamp((float)npc.frameCounter / 12f, 0f, 1f));
                    npc.frameCounter++;
                    break;
                case QueenSlimeFrameType.JumpPreparation:
                    if (frame is < 16 or > 19)
                    {
                        npc.frameCounter = 0;
                        frame = 16;
                    }

                    npc.frameCounter++;
                    if (npc.frameCounter >= 8)
                    {
                        frame++;
                        if (frame > 19)
                            frame = 16;
                        npc.frameCounter = 0;
                    }
                    break;
                case QueenSlimeFrameType.Flying:
                    if (frame is < 20 or > 23)
                    {
                        if (frame is < 4 or > 7)
                        {
                            frame = 4;
                            npc.frameCounter = -1.0;
                        }

                        if ((npc.frameCounter += 1D) >= 4.0)
                        {
                            npc.frameCounter = 0.0;
                            frame++;
                            if (frame >= 7)
                                frame = npc.ai[2] == 1f ? 7 : 22;
                        }
                    }
                    else if ((npc.frameCounter += 1D) >= 5.0)
                    {
                        npc.frameCounter = 0.0;
                        frame++;
                        if (frame >= 24)
                            frame = 20;
                    }
                    break;
            }
            npc.frame.Y = frame * frameHeight;
        }

        #endregion Drawing and Frames

        #region Misc Utilities

        public static bool InPhase2(NPC npc) => npc.life < npc.lifeMax * Phase2LifeRatio;

        public static bool OnSolidGround(NPC npc)
        {
            bool solidGround = false;
            for (int i = -8; i < 8; i++)
            {
                Tile ground = CalamityUtils.ParanoidTileRetrieval((int)(npc.Bottom.X / 16f) + i, (int)(npc.Bottom.Y / 16f) + 1);
                bool notAFuckingTree = ground.TileType is not TileID.Trees and not TileID.PineTree and not TileID.PalmTree;
                if (ground.HasUnactuatedTile && notAFuckingTree && (Main.tileSolid[ground.TileType] || Main.tileSolidTop[ground.TileType]))
                {
                    solidGround = true;
                    break;
                }
            }
            return solidGround;
        }
        #endregion Misc Utilities

        #region Tips
        public override IEnumerable<Func<NPC, string>> GetTips()
        {
            yield return n => "Keep your feet working! This gelatinous queen will stop at nothing to crush her foes!";
            yield return n => "Short hops may help better than trying to fly away from all the crystal shrapnel!";
        }
        #endregion Tips
    }
}
