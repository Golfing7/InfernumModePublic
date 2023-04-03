﻿using InfernumMode.Content.Rarities.InfernumRarities;
using Microsoft.Xna.Framework;
using Terraria;

namespace InfernumMode.Content.Rarities.Sparkles
{
    public class RelicSparkle : RaritySparkle
    {
        public RelicSparkle(SparkleType type, int lifetime, float scale, float initialRotation, float rotationSpeed, Vector2 position, Vector2 velocity)
        {
            Type = type;
            Lifetime = lifetime;
            Scale = 0;
            MaxScale = scale;
            Rotation = initialRotation;
            RotationSpeed = rotationSpeed;
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(Color.OrangeRed, Color.Red, Main.rand.NextFloat(0, 1f));
            Texture = InfernumRarityHelper.SparkleTexure;
            BaseFrame = null;
        }
    }
}
