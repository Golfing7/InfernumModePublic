using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernumMode.Content.Tiles.Misc
{
    public class GolemArena : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Arena");
            AddMapEntry(new(95, 31, 0), name);
            CalamityUtils.SetMerge(Type, TileID.LihzahrdBrick);
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

        public override bool CanExplode(int i, int j) => false;

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
            {
                if (!NPC.AnyNPCs(NPCID.Golem))
                {
                    WorldGen.KillTile(i, j, false, false, false);
                    if (!Main.tile[i, j].HasTile && Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j, 0f, 0, 0, 0);
                    }
                }
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 40f / 255f;
            g = Main.DiscoR / 255f / 4.25f + 40f / 255f;
            b = 74f / 255f;
        }
    }
}
