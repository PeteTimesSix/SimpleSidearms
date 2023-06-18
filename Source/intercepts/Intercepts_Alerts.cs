using HarmonyLib;
using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{

    [HarmonyPatch(typeof(Alert_HunterLacksRangedWeapon), "HuntersWithoutRangedWeapon")]
    [HarmonyPatch(MethodType.Getter)]
    public static class Alert_HunterLacksRangedWeapon_HuntersWithoutRangedWeapon_Postfix
    {
        [HarmonyPostfix]
        public static void HuntersWithoutRangedWeapon(Alert_HunterLacksRangedWeapon __instance, List<Pawn> __result)
        {
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                Pawn pawn = __result[i];
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (pawnMemory != null && pawn.IsValidSidearmsCarrierRightNow() && pawnMemory.IsUsingAutotool(true, true))
                {
                    __result.Remove(pawn);
                }
            }
        }
    }
}
