using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace InfernumMode.Content.BossIntroScreens
{
    public class QueenBeeIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new(_ =>
        {
            float colorFadeInterpolant = MathF.Sin(AnimationCompletion * MathHelper.Pi * 3f) * 0.5f + 0.5f;
            return Color.Lerp(Color.Yellow, Color.Orange, colorFadeInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay => "Hive Matriarch\nQueen Bee";

        public override bool ShouldBeActive() => NPC.AnyNPCs(NPCID.QueenBee);

        public override SoundStyle? SoundToPlayWithTextCreation => new SoundStyle("CalamityMod/Sounds/Custom/AbilitySounds/PlagueReaperAbility");
    }
}