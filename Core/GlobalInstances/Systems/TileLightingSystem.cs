using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Core.GlobalInstances.Systems
{
    public class TileLightingSystem : ModSystem
    {
        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            // Cached for performance reasons. Profiling revealed that all of the LocalPlayer/Center getters were causing slowdowns.
            Vector2 playerCenter = Main.LocalPlayer.Center;

            int crystalID = ModContent.TileType<LumenylCrystals>();
            for (int dx = -160; dx < 160; dx++)
            {
                for (int dy = -50; dy < 50; dy++)
                {
                    int i = (int)(playerCenter.X / 16f + dx);
                    int j = (int)(playerCenter.Y / 16f + dy);
                    if (!WorldGen.InWorld(i, j, 1))
                        continue;

                    Tile t = Main.tile[i, j];
                    if (t.TileType != crystalID)
                        continue;

                    Texture2D texture = TextureAssets.Tile[crystalID].Value;
                    Vector2 drawPosition = new Vector2(i, j) * 16f;
                    Rectangle tileFrame = new(t.TileFrameX, t.TileFrameY, 18, 18);
                    ScreenOverlaysSystem.ThingsToDrawOnTopOfBlur.Add(new(texture, drawPosition, tileFrame, Color.White * 0.75f, 0f, Vector2.Zero, 1f, 0, 0));
                }
            }
        }
    }
}