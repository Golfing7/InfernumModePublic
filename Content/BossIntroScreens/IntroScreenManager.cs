﻿using CalamityMod.Events;
using InfernumMode.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernumMode.Content.BossIntroScreens
{
    public class IntroScreenManager : ModSystem
    {
        internal static List<BaseIntroScreen> IntroScreens = new();

        public static bool ScreenIsObstructed
        {
            get => IntroScreens.Any(s => s.ShouldCoverScreen && s.ShouldBeActive() && s.AnimationCompletion < 1f) && InfernumConfig.Instance.BossIntroductionAnimationsAreAllowed && Main.netMode == NetmodeID.SinglePlayer;
        }

        public static bool ShouldDisplayJokeIntroText
        {
            get
            {
                int introTextDisplayChance = Utilities.IsAprilFirst() ? 5 : 500;
                return Main.rand.NextBool(introTextDisplayChance);
            }
        }

        public static void UpdateScreens()
        {
            foreach (BaseIntroScreen introScreen in IntroScreens)
                introScreen.Update();
        }

        public static void Draw()
        {
            UpdateScreens();
            foreach (BaseIntroScreen introScreen in IntroScreens)
            {
                if (introScreen.ShouldBeActive() && !BossRushEvent.BossRushActive)
                {
                    if (introScreen.AnimationTimer < introScreen.AnimationTime)
                    {
                        introScreen.Draw(Main.spriteBatch);
                        break;
                    }
                    else if (!introScreen.CaresAboutBossEffectCondition)
                        introScreen.AnimationTimer = 0;
                }
            }
        }
    }
}
