using CalamityMod.NPCs.Ravager;
using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Ravager
{
    public class RavagerClawRightBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => ModContent.NPCType<RavagerClawRight>();

        public override bool PreAI(NPC npc) => RavagerClawLeftBehaviorOverride.DoClawAI(npc, false);

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor) => RavagerClawLeftBehaviorOverride.DrawClaw(npc, Main.spriteBatch, lightColor, false);
    }
}
