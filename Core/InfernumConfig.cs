﻿using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace InfernumMode.Core
{
    [BackgroundColor(96, 30, 53, 216)]
    public class InfernumConfig : ModConfig
    {
        public static InfernumConfig Instance => ModContent.GetInstance<InfernumConfig>();

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(true)]
        public bool BossIntroductionAnimationsAreAllowed { get; set; }

        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(true)]
        public bool DisplayTipsInChat { get; set; }

        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(false)]
        public bool ReducedGraphicsConfig { get; set; }

        [BackgroundColor(224, 127, 180, 192)]
        [SliderColor(224, 165, 56, 128)]
        [Range(0f, 1f)]
        [DefaultValue(0f)]
        public float SaturationBloomIntensity { get; set; }

        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(true)]
        public bool FlashbangOverlays { get; set; }

        [BackgroundColor(224, 127, 180, 192)]
        [DefaultValue(false)]
        public bool CreditsRecordings { get; set; }

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) => false;
    }

}
