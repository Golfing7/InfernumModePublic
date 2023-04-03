using CalamityMod;
using InfernumMode.Content.Subworlds;
using InfernumMode.Content.Tiles;
using InfernumMode.Core.GlobalInstances.Systems;
using SubworldLibrary;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Core.GlobalInstances
{
    public class InfernumGlobalTile : GlobalTile
    {
        public static bool ShouldNotBreakDueToAboveTile(int x, int y)
        {
            int[] invincibleTiles = new int[]
            {
                ModContent.TileType<ColosseumPortal>(),
                ModContent.TileType<ProvidenceSummoner>(),
                ModContent.TileType<ProvidenceRoomDoorPedestal>(),
            };

            Tile checkTile = CalamityUtils.ParanoidTileRetrieval(x, y);
            Tile aboveTile = CalamityUtils.ParanoidTileRetrieval(x, y - 1);

            // Prevent tiles below invincible tiles from being destroyed. This is like chests in vanilla.
            return aboveTile.HasTile && checkTile.TileType != aboveTile.TileType && invincibleTiles.Contains(aboveTile.TileType);
        }

        public override bool CanExplode(int i, int j, int type)
        {
            if (ShouldNotBreakDueToAboveTile(i, j))
                return false;

            if (WorldSaveSystem.ProvidenceArena.Intersects(new(i, j, 1, 1)) || SubworldSystem.IsActive<LostColosseum>())
                return false;

            return base.CanExplode(i, j, type);
        }

        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            if (ShouldNotBreakDueToAboveTile(i, j))
                return false;

            if (WorldSaveSystem.ProvidenceArena.Intersects(new(i, j, 1, 1)) || SubworldSystem.IsActive<LostColosseum>())
                return false;

            return base.CanKillTile(i, j, type, ref blockDamaged);
        }

        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            bool tombstonesShouldSpontaneouslyCombust = WorldSaveSystem.ProvidenceArena.Intersects(new(i, j, 16, 16)) || SubworldSystem.IsActive<LostColosseum>();
            if (tombstonesShouldSpontaneouslyCombust && type is TileID.Tombstones)
                WorldGen.KillTile(i, j);
        }
    }
}
