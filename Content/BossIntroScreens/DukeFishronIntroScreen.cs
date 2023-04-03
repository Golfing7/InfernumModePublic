using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace InfernumMode.Content.BossIntroScreens
{
    public class DukeFishronIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new(completionRatio =>
        {
            float skinColorInterpolant = Utils.GetLerpValue(0.77f, 1f, (float)Math.Sin(AnimationCompletion * -MathHelper.Pi * 4f + completionRatio * MathHelper.Pi) * 0.5f + 0.5f);
            Color skinColor = new(45, 140, 92);
            Color skinColor2 = new(53, 77, 132);
            return Color.Lerp(skinColor, skinColor2, skinColorInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay => "Terror of the Seas\nDuke Fishron";

        public override bool ShouldBeActive() => NPC.AnyNPCs(NPCID.DukeFishron);

        // Sounds are played in Duke Fishron's AI.
        public override SoundStyle? SoundToPlayWithTextCreation => null;
    }
}