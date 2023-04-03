using CalamityMod.NPCs.AstrumDeus;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace InfernumMode.Content.BossIntroScreens
{
    public class AstrumDeusIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new(completionRatio =>
        {
            float colorInterpolant = (float)Math.Sin(completionRatio * MathHelper.Pi * 4f + AnimationCompletion * 1.45f * MathHelper.TwoPi) * 0.5f + 0.5f;
            return Color.Lerp(new(68, 221, 204), new(255, 100, 80), colorInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay => "The Star Weaver\nAstrum Deus";

        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<AstrumDeusHead>());

        public override SoundStyle? SoundToPlayWithTextCreation => null;
    }
}