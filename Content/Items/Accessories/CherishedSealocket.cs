﻿using CalamityMod.Items;
using InfernumMode.Content.Rarities.InfernumRarities;
using InfernumMode.Core.GlobalInstances.Players;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.Content.Items.Accessories
{
    public class CherishedSealocket : ModItem
    {
        public const int MaxHighDRHits = 2;

        public const int ForcefieldRechargeSeconds = 90;

        public const float ForcefieldDRMultiplier = 0.6f;

        public const float DamageBoostWhenForcefieldIsUp = 0.12f;

        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Cherished Sealocket");
            Tooltip.SetDefault($"Grants a water forcefield that dissipates after {MaxHighDRHits} hits\n" +
                $"When the forcefield is up, hard-hitting hits do {(int)(ForcefieldDRMultiplier * 100f)}% less damage overall, and you recieve a {(int)(DamageBoostWhenForcefieldIsUp * 100f)}% damage boost\n" +
                $"The forcefield reappears after {ForcefieldRechargeSeconds} seconds");
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.value = CalamityGlobalItem.Rarity9BuyPrice;
            Item.rare = ModContent.RarityType<InfernumVassalRarity>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            SealocketPlayer modPlayer = player.GetModPlayer<SealocketPlayer>();
            modPlayer.MechanicalEffectsApply = true;
            modPlayer.ForcefieldCanDraw = !hideVisual;
        }

        public override void UpdateVanity(Player player) =>
            player.GetModPlayer<SealocketPlayer>().ForcefieldCanDraw = true;
    }
}
