using InfernumMode.Core.OverridingSystem;
using Terraria;
using Terraria.ID;

namespace InfernumMode.Content.BehaviorOverrides.BossAIs.Golem
{
    public class GolemFistLeftBehaviorOverride : NPCBehaviorOverride
    {
        public override int NPCOverrideType => NPCID.GolemFistLeft;

        public override bool PreAI(NPC npc) => DoFistAI(npc, true);

        // public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color lightColor) => DrawFist(npc, Main.spriteBatch, lightColor, true);

        public static bool DoFistAI(NPC npc, bool leftFist)
        {
            npc.dontTakeDamage = true;
            npc.chaseable = false;
            npc.Opacity = 1f;
            npc.damage = Main.npc[(int)npc.ai[0]].damage >= 1 ? npc.defDamage : 0;
            return false;
        }

        /*public static bool DrawFist(NPC npc, SpriteBatch spriteBatch, Color lightColor, bool leftFist)
        {
            NPC body = Main.npc[(int)npc.ai[0]];
            SpriteEffects effect = leftFist ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D texture = Main.projectileTexture[ModContent.ProjectileType<FistBullet>()];
            Main.spriteBatch.Draw(texture, npc.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), lightColor, npc.rotation, npc.Center, 1f, effect, 0f);
            return false;
        }*/
    }
}
