using HarmonyLib;
using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.intercepts
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
    public static class PawnGenerator_GenerateGearFor_Postfix
    {
        [HarmonyPostfix]
        public static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
        {
            float modifiedChance = SimpleSidearms.SidearmSpawnChance;
            float modifiedBudgetMultiplier = SimpleSidearms.SidearmBudgetMultiplier.Value;
            bool more = true;
            int sanityLimiter = 0;

            while (more && modifiedChance > 0 && modifiedBudgetMultiplier > 0 && sanityLimiter < 10)
            {

                more = PawnSidearmsGenerator.TryGenerateSidearmFor(pawn, modifiedChance, modifiedBudgetMultiplier, request);
                modifiedChance -= SimpleSidearms.SidearmSpawnChanceDropoff.Value;
                modifiedBudgetMultiplier -= SimpleSidearms.SidearmBudgetDropoff.Value;
                sanityLimiter++;
            }

        }
    }
}
