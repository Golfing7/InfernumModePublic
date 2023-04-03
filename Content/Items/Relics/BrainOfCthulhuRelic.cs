﻿using InfernumMode.Content.Tiles.Relics;
using Terraria.ModLoader;

namespace InfernumMode.Content.Items.Relics
{
    public class BrainOfCthulhuRelic : BaseRelicItem
    {
        public override string DisplayNameToUse => "Infernal Brain of Cthulhu Relic";

        public override int TileID => ModContent.TileType<BrainOfCthulhuRelicTile>();
    }
}
