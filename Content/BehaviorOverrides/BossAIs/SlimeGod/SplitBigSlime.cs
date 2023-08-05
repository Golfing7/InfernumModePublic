﻿using CalamityMod;
using CalamityMod.Events;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.SlimeGod
{
    public class SplitBigSlime : ModNPC
    {
        public int OwnerIndex => (int)NPC.ai[1];

        public ref float RedirectCountdown => ref NPC.ai[0];

        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            // DisplayName.SetDefault("Unstable Slime Spawn");
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = AIType = -1;
            NPC.damage = 0;
            NPC.width = 40;
            NPC.height = 30;
            NPC.defense = 11;
            NPC.lifeMax = 320;
            NPC.knockBackResist = 0f;
            AnimationType = 121;
            NPC.alpha = 35;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = true;
            NPC.canGhostHeal = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.buffImmune[BuffID.OnFire] = true;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(NPC.lifeMax);

        public override void ReceiveExtraAI(BinaryReader reader) => NPC.lifeMax = reader.ReadInt32();

        public override void AI()
        {
            if (!Main.npc.IndexInRange(OwnerIndex) || !Main.npc[OwnerIndex].active)
            {
                NPC.active = false;
                return;
            }

            NPC slimeGod = Main.npc[OwnerIndex];
            if (slimeGod.Infernum().ExtraAI[1] == 2f)
                NPC.active = false;

            if (!NPC.WithinRange(slimeGod.Center, Main.rand.NextFloat(380f, 520f)) || slimeGod.Infernum().ExtraAI[1] == 1f)
                RedirectCountdown = 60f;

            if (RedirectCountdown > 0f && !NPC.WithinRange(slimeGod.Center, 50f))
            {
                float flySpeed = BossRushEvent.BossRushActive ? 38f : 14f;
                flySpeed = MathF.Max(flySpeed, slimeGod.velocity.Length() * 0.7f);

                Vector2 destinationOffset = (TwoPi * NPC.whoAmI / 13f).ToRotationVector2() * 32f;
                NPC.velocity = (NPC.velocity * 34f + NPC.SafeDirectionTo(slimeGod.Center + destinationOffset) * flySpeed) / 35f;
                if (!NPC.WithinRange(slimeGod.Center, 175f))
                    NPC.Center = Vector2.Lerp(NPC.Center, slimeGod.Center, 0.05f);

                RedirectCountdown--;
            }

            NPC.rotation = Clamp(NPC.velocity.X * 0.05f, -0.2f, 0.2f);
            NPC.spriteDirection = (NPC.velocity.X < 0f).ToDirectionInt();
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.penetrate is > 1 or (-1))
                modifiers.FinalDamage *= 0.1f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int hitDirection = hit.HitDirection;
            for (int k = 0; k < 5; k++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hitDirection, -1f, 0, default, 1f);

            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hitDirection, -1f, 0, default, 1f);
            }
        }

        public override bool CheckDead()
        {
            if (!Main.npc.IndexInRange(OwnerIndex) || !Main.npc[OwnerIndex].active)
                return base.CheckDead();

            Main.npc[OwnerIndex].life -= NPC.lifeMax;
            Main.npc[OwnerIndex].HitEffect(0, NPC.lifeMax);
            if (Main.npc[OwnerIndex].life <= 0)
                Main.npc[OwnerIndex].NPCLoot();

            return base.CheckDead();
        }

        public override bool PreKill() => false;
    }
}
