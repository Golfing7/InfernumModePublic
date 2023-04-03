using CalamityMod.NPCs.Perforator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace InfernumMode.Content.Skies
{
    public class PerforatorScene : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player)
        {
            int perforatorHiveID = ModContent.NPCType<PerforatorHive>();
            int perforatorHive = NPC.FindFirstNPC(perforatorHiveID);
            NPC perforatorHiveNPC = perforatorHive >= 0 ? Main.npc[perforatorHive] : null;
            bool enabled = perforatorHiveNPC != null && perforatorHiveNPC.localAI[1] > 0f;
            return enabled;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals("InfernumMode:Perforators", isActive);
        }
    }

    public class PerforatorSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private int HiveIndex = -1;

        public override void Update(GameTime gameTime)
        {
            if (isActive && intensity < 1f)
            {
                intensity += 0.01f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.01f;
            }

            if (NPC.FindFirstNPC(ModContent.NPCType<PerforatorHive>()) == -1)
                Deactivate();
        }

        private float GetIntensity()
        {
            if (UpdatePIndex())
            {
                float x = 0f;
                if (HiveIndex != -1)
                {
                    x = Vector2.Distance(Main.player[Main.myPlayer].Center, Main.npc[HiveIndex].Center);
                }
                return (1f - Utils.SmoothStep(3000f, 6000f, x)) * Main.npc[HiveIndex].localAI[1] * 0.25f;
            }
            return 0.7f;
        }

        public override Color OnTileColor(Color inColor)
        {
            float intensity = GetIntensity();
            return new Color(Vector4.Lerp(new Vector4(0.6f, 0f, 0f, 1f), inColor.ToVector4(), 1f - intensity));
        }

        private bool UpdatePIndex()
        {
            int ProvType = ModContent.NPCType<PerforatorHive>();
            if (HiveIndex >= 0 && Main.npc[HiveIndex].active && Main.npc[HiveIndex].type == ProvType)
            {
                return true;
            }
            HiveIndex = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ProvType)
                {
                    HiveIndex = i;
                    break;
                }
            }
            return HiveIndex != -1;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                float intensity = GetIntensity();
                Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Crimson * intensity);
            }
        }

        public override float GetCloudAlpha()
        {
            return 0f;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            return isActive || intensity > 0f;
        }
    }
}
