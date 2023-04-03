﻿using InfernumMode.Content.Tiles.Relics;
using Terraria.ModLoader;

namespace InfernumMode.Content.Items.Relics
{
    public class CalamitasCloneRelic : BaseRelicItem
    {
        public override string DisplayNameToUse => "Infernal Calamitas Clone Relic";

        public override int TileID => ModContent.TileType<CalamitasCloneRelicTile>();
    }
}
