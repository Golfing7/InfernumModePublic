﻿using CalamityMod;
using InfernumMode.Content.Projectiles.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace InfernumMode.Content.Tiles.Abyss
{
    public class HydrothermalVent : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Geyser");
            AddMapEntry(new Color(150, 100, 50), name);

            base.SetStaticDefaults();
        }

        public override bool IsTileDangerous(int i, int j, Player player) => true;

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile t = CalamityUtils.ParanoidTileRetrieval(i, j);

            Vector2 spawnPosition = new(i * 16f + 18f, j * 16f + 10f);
            if (!Main.gamePaused && t.TileFrameX % 36 == 0 && t.TileFrameY % 36 == 0 && Collision.CanHitLine(spawnPosition, 1, 1, spawnPosition - Vector2.UnitY * 100f, 1, 1))
            {
                float positionInterpolant = (i + j) * 0.041f % 1f;
                Vector2 smokeVelocity = -Vector2.UnitY.RotatedByRandom(0.11f) * Lerp(4.8f, 8.1f, positionInterpolant);
                smokeVelocity.X += Cos(TwoPi * positionInterpolant) * 1.7f;
                Projectile.NewProjectile(new EntitySource_WorldEvent(), spawnPosition, smokeVelocity, ModContent.ProjectileType<HydrothermalSmoke>(), Main.expertMode ? 56 : 98, 0f);
            }
        }
    }
}
