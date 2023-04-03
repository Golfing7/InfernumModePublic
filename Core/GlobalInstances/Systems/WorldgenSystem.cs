using InfernumMode.Content.WorldGeneration;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace InfernumMode.Core.GlobalInstances.Systems
{
    public class WorldgenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int finalCleanupIndex = tasks.FindIndex(g => g.Name == "Final Cleanup");
            if (finalCleanupIndex != -1)
            {
                tasks.Insert(++finalCleanupIndex, new PassLegacy("Prov Arena", (progress, config) =>
                {
                    progress.Message = "Constructing a temple for an ancient goddess";
                    ProfanedGarden.Generate(progress, config);
                }));
                tasks.Insert(++finalCleanupIndex, new PassLegacy("Desert Digout Area", LostColosseumEntrance.Generate));
            }
        }
    }
}
