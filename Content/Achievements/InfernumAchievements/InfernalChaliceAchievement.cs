﻿using Terraria;
using Terraria.ModLoader.IO;

namespace InfernumMode.Content.Achievements.InfernumAchievements
{
    public class InfernalChaliceAchievement : Achievement
    {
        #region Overrides
        public override void Initialize()
        {
            Name = "Baptized By Hellfire";
            Description = "Complete the final challenge, and earn your reward\n[c/777777:Obtain the Infernal Chalice]";
            TotalCompletion = 1;
            PositionInMainList = 6;
            UpdateCheck = AchievementUpdateCheck.ItemPickup;
        }
        // This takes in itemID, but doesn't need it as its checked before this is sent due to only needing
        // to check one thing.
        public override void ExtraUpdate(Player player, int itemID)
        {
            CurrentCompletion++;
        }
        public override void SaveProgress(TagCompound tag)
        {
            tag["ChaliceCurrentCompletion"] = CurrentCompletion;
            tag["ChaliceDoneCompletionEffects"] = DoneCompletionEffects;
        }
        public override void LoadProgress(TagCompound tag)
        {
            CurrentCompletion = tag.Get<int>("ChaliceCurrentCompletion");
            DoneCompletionEffects = tag.Get<bool>("ChaliceDoneCompletionEffects");
        }
        #endregion
    }
}
