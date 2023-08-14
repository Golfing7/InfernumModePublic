using CalamityMod.NPCs.SupremeCalamitas;
using InfernumMode.Assets.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BossIntroScreens
{
    public class SCalIntroScreen : BaseIntroScreen
    {
        public override TextColorData TextColor => Color.White;

        public override Color ScreenCoverColor => Color.Black;

        public override int AnimationTime => 300;

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => true;

        public override float TextScale => MajorBossTextScale;

        public override Effect ShaderToApplyToLetters => InfernumEffectsRegistry.SCalIntroLetterShader.Shader;

        public override void PrepareShader(Effect shader)
        {
            shader.Parameters["uColor"].SetValue(Color.Red.ToVector3());
            shader.Parameters["uSecondaryColor"].SetValue(Color.Orange.ToVector3());
            shader.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin").Value;
        }

        public override bool ShouldBeActive() => NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>());

        public override SoundStyle? SoundToPlayWithTextCreation => null;

        public override SoundStyle? SoundToPlayWithLetterAddition => SoundID.Item100;

        public override bool CanPlaySound => LetterDisplayCompletionRatio(AnimationTimer) >= 1f;

        public override float LetterDisplayCompletionRatio(int animationTimer)
        {
            float completionRatio = Utils.GetLerpValue(TextDelayInterpolant, 0.92f, animationTimer / (float)AnimationTime, true);

            // If the completion ratio exceeds the point where the name is displayed, display all letters.
            int startOfLargeTextIndex = TextToDisplay.Value.IndexOf('\n');
            int currentIndex = (int)(completionRatio * TextToDisplay.Value.Length);
            if (currentIndex >= startOfLargeTextIndex)
                completionRatio = 1f;

            return completionRatio;
        }
    }
}