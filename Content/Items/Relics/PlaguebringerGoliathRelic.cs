﻿using InfernumMode.Content.Tiles.Relics;
using Terraria.ModLoader;

namespace InfernumMode.Content.Items.Relics
{
    public class PlaguebringerGoliathRelic : BaseRelicItem
    {
        public override string DisplayNameToUse => "Infernal Plaguebringer Goliath Relic";

        public override int TileID => ModContent.TileType<PlaguebringerGoliathRelicTile>();
    }
}
