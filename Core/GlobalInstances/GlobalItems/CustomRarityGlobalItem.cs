﻿using InfernumMode.Content.Rarities.InfernumRarities;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Core.GlobalInstances.GlobalItems
{
    public class CustomRarityGlobalItem : GlobalItem
    {
        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            // If the item is of the rarity, and the line is the item name.
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                if (item.rare == ModContent.RarityType<InfernumRedRarity>())
                {
                    // Draw the custom tooltip line.
                    InfernumRedRarity.DrawCustomTooltipLine(line);
                    return false;
                }
                else if (item.rare == ModContent.RarityType<InfernumVassalRarity>())
                {
                    // Draw the custom tooltip line.
                    InfernumVassalRarity.DrawCustomTooltipLine(line);
                    return false;
                }
                else if (item.rare == ModContent.RarityType<InfernumProfanedRarity>())
                {
                    // Draw the custom tooltip line.
                    InfernumProfanedRarity.DrawCustomTooltipLine(line);
                    return false;
                }
                else if (item.rare == ModContent.RarityType<InfernumHatgirlRarity>())
                {
                    InfernumHatgirlRarity.DrawCustomTooltipLine(line);
                    return false;
                }
            }
            return true;
        }
    }
}
