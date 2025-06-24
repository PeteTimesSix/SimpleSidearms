using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using SimpleSidearms.rimworld;
using PeteTimesSix.SimpleSidearms.Utilities;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "InterfaceDrop")]
    public static class ITab_Pawn_Gear_InterfaceDrop_Prefix
    {
        [HarmonyPrefix]
        public static void InterfaceDrop(ITab_Pawn_Gear __instance, Thing t)
        {
            if (t.def.IsMeleeWeapon || t.def.IsRangedWeapon)
            {
                ThingWithComps thingWithComps = t as ThingWithComps;
                ThingOwner thingOwner = thingWithComps.holdingOwner;
                IThingHolder actualOwner = thingOwner.Owner;
                if (actualOwner is Pawn_InventoryTracker)
                {
                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn((actualOwner as Pawn_InventoryTracker).pawn);
                    if (pawnMemory == null)
                        return;
                    pawnMemory.InformOfDroppedSidearm(thingWithComps, true);
                }
                else if (actualOwner is Pawn_EquipmentTracker)
                {
                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn((actualOwner as Pawn_EquipmentTracker).ParentHolder as Pawn);
                    if (pawnMemory == null)
                        return;
                    pawnMemory.InformOfDroppedSidearm(thingWithComps, true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Pawn_GetGizmos_Postfix
    {
        [HarmonyPostfix]
        //public static void GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            //This postfix inserts the SimpleSidearms gizmo before all other gizmos
            if (__instance.IsValidSidearmsCarrierRightNow() && (__instance.IsColonistPlayerControlled
                || DebugSettings.godMode) && __instance.equipment != null && __instance.inventory != null
                )
            {
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(__instance);
                if (pawnMemory != null)
                {
                    List<ThingDefStuffDefPair> rangedWeaponMemories = new List<ThingDefStuffDefPair>();
                    List<ThingDefStuffDefPair> meleeWeaponMemories = new List<ThingDefStuffDefPair>();

                    var rememberedWeapons = pawnMemory.RememberedWeapons;
                    for (int i = rememberedWeapons.Count - 1; i >= 0; i--)
                    {
                        ThingDefStuffDefPair weapon = rememberedWeapons[i];
                        if (weapon.thing.IsMeleeWeapon)
                            meleeWeaponMemories.Add(weapon);
                        else if (weapon.thing.IsRangedWeapon)
                            rangedWeaponMemories.Add(weapon);
                    }

                    List<ThingWithComps> carriedWeapons = __instance.GetCarriedWeapons(includeTools: true);
                    yield return new Gizmo_SidearmsList(__instance, carriedWeapons, pawnMemory.RememberedWeapons, pawnMemory);

                    if (SimpleSidearms.Settings.ShowBrainscope)
                    {
                        yield return new Gizmo_Brainscope(__instance);
                    }
                }
            }

            foreach (var aGizmo in __result)
            {
                yield return aGizmo;
            }
        }
    }
}
