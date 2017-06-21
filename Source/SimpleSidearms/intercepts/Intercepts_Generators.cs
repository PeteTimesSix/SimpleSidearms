using Harmony;
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
    static class PawnGenerator_GenerateGearFor_Postfix
    {
        [HarmonyPostfix]
        private static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
        {
            PawnSidearmsGenerator.TryGenerateSidearmsFor(pawn);
        }
    }

    //run reset right after its run for the regular weapon generator
    [HarmonyPatch(typeof(PawnWeaponGenerator), "Reset")]
    static class PawnWeaponGenerator_Reset_Postfix
    {
        [HarmonyPostfix]
        private static void Reset()
        {
            PawnSidearmsGenerator.Reset();
        }
    }
}
