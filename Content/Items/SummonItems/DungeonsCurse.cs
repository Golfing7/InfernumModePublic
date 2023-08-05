using CalamityMod;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.Items.SummonItems
{
    public class DungeonsCurse : ModItem
    {
        public int frameCounter;
        public int frame;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            // DisplayName.SetDefault("Dungeon's Curse");
            /* Tooltip.SetDefault("Summons Skeletron\n" +
                "Skeletron enrages during daytime\n" +
                "It becomes nighttime if this item is used during daytime\n" +
                "Not consumable"); */
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 5; // Abeemination / Deer Thing
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 54;
            Item.rare = ItemRarityID.Green;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = false;
        }

        public override bool CanUseItem(Player player) => !NPC.AnyNPCs(NPCID.SkeletronHead);

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<AncientBoneDust>(), 3);
            recipe.AddIngredient(ItemID.Vertebrae, 7);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<AncientBoneDust>(), 3);
            recipe.AddIngredient(ItemID.RottenChunk, 7);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = ModContent.Request<Texture2D>("InfernumMode/Content/Items/SummonItems/DungeonsCurse_Animated").Value;
            Rectangle f = Item.GetCurrentFrame(ref frame, ref frameCounter, 8, 5);
            Main.spriteBatch.Draw(texture, position, f, Color.White, 0f, f.Size() * new Vector2(0.16f, 0.25f), scale, SpriteEffects.None, 0);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("InfernumMode/Content/Items/SummonItems/DungeonsCurse_Animated").Value;
            Main.spriteBatch.Draw(texture, Item.position - Main.screenPosition, Item.GetCurrentFrame(ref frame, ref frameCounter, 8, 5), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            return false;
        }

        public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Ensure that it's night-time.
                if (Main.dayTime)
                {
                    Main.time = 0.0;
                    Main.dayTime = !Main.dayTime;
                    CalamityNetcode.SyncWorld();
                }

                Vector2 spawnPosition = player.Center - Vector2.UnitY * 800f;
                NPC.NewNPC(player.GetSource_ItemUse(Item), (int)spawnPosition.X, (int)spawnPosition.Y, NPCID.SkeletronHead);
            }
            return true;
        }
    }
}
