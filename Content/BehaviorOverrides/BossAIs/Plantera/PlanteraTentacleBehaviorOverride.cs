﻿using InfernumMode.Core.OverridingSystem;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Plantera
{
    public class PlanteraTentacleBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.PlanterasTentacle;

        public override bool PreAI(NPC npc)
        {
            // Die if Plantera is absent or not using tentacles.
            if (!Main.npc.IndexInRange(NPC.plantBoss) || Main.npc[NPC.plantBoss].ai[0] != (int)PlanteraBehaviorOverride.PlanteraAttackState.TentacleSnap)
            {
                npc.life = 0;
                npc.HitEffect();
                npc.checkDead();
                npc.netUpdate = true;
                return false;
            }

            // Ensure that the tentacle always draws, even when far offscreen.
            NPCID.Sets.MustAlwaysDraw[npc.type] = true;

            float attachAngle = npc.ai[0];
            ref float attachOffset = ref npc.ai[1];
            ref float time = ref npc.ai[2];
            ref float wiggleSineAngle = ref npc.Infernum().ExtraAI[0];

            // Reel inward prior to snapping.
            if (time > -20f && time < 5f)
                attachOffset = Lerp(attachOffset, 45f, 0.05f);

            wiggleSineAngle += Utils.Remap(time, -67f, -20f, 0f, Pi / 9f + npc.whoAmI * 0.1f);
            float wingleOffset = Sin(wiggleSineAngle) * 0.021f;

            // Reach outward swiftly in hopes of hitting a target.
            if (time > 30f)
            {
                attachOffset = Lerp(attachOffset, 3900f, 0.021f);
                wingleOffset = 0f;
            }

            if (time == 30f)
                SoundEngine.PlaySound(SoundID.Item74, npc.Center);

            if (time > 70f)
            {
                npc.scale *= 0.85f;

                // Die once small enough.
                npc.Opacity = npc.scale;
                if (npc.scale < 0.01f)
                {
                    npc.life = 0;
                    npc.HitEffect();
                    npc.checkDead();
                    npc.active = false;
                }
            }

            attachAngle += wingleOffset;
            npc.Center = Main.npc[NPC.plantBoss].Center + attachAngle.ToRotationVector2() * (attachOffset + wingleOffset * 150f);
            npc.rotation = attachAngle + Pi;
            npc.dontTakeDamage = true;

            time++;
            return false;
        }
    }
}
