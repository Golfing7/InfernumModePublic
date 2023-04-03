﻿using InfernumMode.Content.Tiles.Relics;
using Terraria.ModLoader;

namespace InfernumMode.Content.Items.Relics
{
    public class GiantClamRelic : BaseRelicItem
    {
        public override string DisplayNameToUse => "Infernal Giant Clam Relic";

        public override int TileID => ModContent.TileType<GiantClamRelicTile>();
    }
}
