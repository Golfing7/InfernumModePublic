using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;

namespace InfernumMode.Content.BossIntroScreens
{
    public class CeaselessVoidIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => new(completionRatio =>
        {
            float voidInterpolant = Utils.GetLerpValue(0.77f, 1f, Sin(AnimationCompletion * Pi * 3f + completionRatio * Pi) * 0.5f + 0.5f);
            Color metalColor = new(167, 181, 209);
            Color voidColor = new(12, 18, 27);
            return Color.Lerp(metalColor, voidColor, voidInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override string TextToDisplay => "The Never-Ending\nCeaseless Void";

        public override bool ShouldBeActive() => CalamityGlobalNPC.voidBoss != -1 && Main.npc[CalamityGlobalNPC.voidBoss].ai[0] != 0f;

        public override SoundStyle? SoundToPlayWithTextCreation => null;
    }
}