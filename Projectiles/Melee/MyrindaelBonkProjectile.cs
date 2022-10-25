﻿using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using InfernumMode.Items.Weapons.Melee;
using InfernumMode.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Projectiles.Melee
{
    public class MyrindaelBonkProjectile : ModProjectile
    {
        public PrimitiveTrail PierceAfterimageDrawer = null;

        public Player Owner => Main.player[Projectile.owner];

        public float LungeProgression => Time / Myrindael.LungeTime;

        public ref float Time => ref Projectile.ai[0];

        public override string Texture => "InfernumMode/Items/Weapons/Melee/Myrindael";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Myrindael");
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Myrindael.LungeTime;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        public override void AI()
        {
            // Die if no longer holding the click button or otherwise cannot use the item.
            if (!Owner.channel || Owner.dead || !Owner.active || Owner.noItems)
            {
                Projectile.Kill();
                if (Owner.velocity.Length() > 16f)
                    Owner.velocity *= 0.4f;

                return;
            }

            if (Time == 1f)
                SoundEngine.PlaySound(InfernumSoundRegistry.VassalSlashSound, Projectile.Center);

            // Stick to the owner.
            Projectile.Center = Owner.MountedCenter;

            float angularVelocity = MathHelper.Pi * (float)Math.Pow(LungeProgression, 3D) * 0.024f;
            float currentRotation = Projectile.velocity.ToRotation();
            float idealRotation = Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
            Projectile.velocity = currentRotation.AngleTowards(idealRotation, angularVelocity).ToRotationVector2();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Owner.heldProj = Projectile.whoAmI;

            Owner.fallStart = (int)(Owner.position.Y / 16f);

            float velocityPower = Utils.Remap(CalamityUtils.Convert01To010(LungeProgression), 0f, 1f, 0.25f, 1f);
            Vector2 newVelocity = Projectile.velocity * Myrindael.LungeSpeed * (0.15f + 0.85f * velocityPower);
            Owner.velocity = newVelocity;
            Owner.Calamity().LungingDown = Projectile.timeLeft >= 5;

            // Release anime-like streak particle effects at the side of the owner to indicate motion.
            if (Main.rand.NextBool(2))
            {
                Vector2 energySpawnPosition = Owner.Center + Main.rand.NextVector2Circular(90f, 90f) + Owner.velocity * 2f;
                Vector2 energyVelocity = -Owner.velocity.SafeNormalize(Vector2.UnitX * Owner.direction) * Main.rand.NextFloat(6f, 8.75f);
                Particle energyLeak = new SquishyLightParticle(energySpawnPosition, energyVelocity, Main.rand.NextFloat(0.55f, 0.9f), Color.Yellow, 30, 3.4f, 4.5f);
                GeneralParticleHandler.SpawnParticle(energyLeak);
            }

            Time++;
        }

        public override void Kill(int timeLeft)
        {
            Owner.Calamity().LungingDown = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            // Create lightning from the sky.
            SoundEngine.PlaySound(CommonCalamitySounds.LargeWeaponFireSound with { Volume = 0.3f }, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 lightningSpawnPosition = target.Center + new Vector2(Main.rand.NextFloatDirection() * 30f, -1100f);
                    int lightning = Utilities.NewProjectileBetter(lightningSpawnPosition, Vector2.UnitY * Main.rand.NextFloat(8.5f, 11f), ModContent.ProjectileType<MyrindaelLightning>(), Projectile.damage, 0f, Projectile.owner);
                    if (Main.projectile.IndexInRange(lightning))
                    {
                        Main.projectile[lightning].ai[0] = Main.projectile[lightning].velocity.ToRotation();
                        Main.projectile[lightning].ai[1] = Main.rand.Next(100);
                    }
                }
            }

            Owner.velocity = Owner.velocity.SafeNormalize(Vector2.Zero) * -10f;
            Projectile.Kill();
        }

        public override Color? GetAlpha(Color lightColor) => lightColor * Projectile.Opacity * (float)(1f - Math.Pow(LungeProgression, 5D));

        public float PierceWidthFunction(float completionRatio)
        {
            float width = Utils.GetLerpValue(0f, 0.1f, completionRatio, true) * Projectile.scale * 20f;
            width *= 1f - (float)Math.Pow(LungeProgression, 5D);
            return width;
        }

        public Color PierceColorFunction(float completionRatio) => Color.Lime * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();

            Color mainColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f) % 1, Color.Cyan, Color.DeepSkyBlue, Color.Turquoise, Color.Blue);
            Color secondaryColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + 0.2f) % 1, Color.Cyan, Color.DeepSkyBlue, Color.Turquoise, Color.Blue);

            mainColor = Color.Lerp(Color.White, mainColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));
            secondaryColor = Color.Lerp(Color.White, secondaryColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));

            // Initialize the trail drawer.
            PierceAfterimageDrawer ??= new(PierceWidthFunction, PierceColorFunction, null, GameShaders.Misc["CalamityMod:ExobladePierce"]);

            Vector2 trailOffset = Projectile.Size * 0.5f - Main.screenPosition + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 90f;
            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(mainColor);
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(secondaryColor);
            GameShaders.Misc["CalamityMod:ExobladePierce"].Apply();
            PierceAfterimageDrawer.Draw(Projectile.oldPos.Take(12), trailOffset, 53);

            Main.spriteBatch.ExitShaderRegion();

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new(0, texture.Height);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, origin, Projectile.scale, 0, 0);
            return false;
        }
    }
}
