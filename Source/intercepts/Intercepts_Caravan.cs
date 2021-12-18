using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{
    [HarmonyPatch(typeof(CaravanEnterMapUtility), "Enter", new Type[] { typeof(Caravan), typeof(Map), typeof(Func<Pawn, IntVec3>), typeof(CaravanDropInventoryMode), typeof(bool)})]
    public static class CaravanEnterMapUtility_Enter
    {
        [HarmonyPrefix]
        public static void Prefix(Caravan caravan, out List<Pawn> __state)
        {
            __state = new List<Pawn>();
            if (caravan != null)
            {
                // save a list of all pawns that were in the caravan
                foreach (var pawn in caravan.pawns)
                    __state.Add(pawn);
            }
        }
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> __state)
        {
            if (!(__state?.Count > 0))
                return;

            // check for missing sidearms and transfer ones picked up by other pawns back to the correct inventory
            CaravanUtility.TransferWeaponsToCorrectInventory(__state);
        }
    }
    [HarmonyPatch(typeof(Dialog_SplitCaravan), "TrySplitCaravan")]
    public static class Dialog_SplitCaravan_TrySplitCaravan
    {
        [HarmonyPrefix]
        public static void Prefix(ref Dialog_SplitCaravan __instance, out List<Pawn> __state)
        {
            __state = new List<Pawn>();
            if (__instance?.GetType().GetField("caravan", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(__instance) is Caravan caravan)
            {
                // save a list of all pawns that were in the caravan
                foreach (var pawn in caravan.pawns)
                    __state.Add(pawn);
            }
        }
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> __state)
        {
            if (!(__state?.Count > 0))
                return;

            // check for missing sidearms and transfer ones picked up by other pawns back to the correct inventory
            CaravanUtility.TransferWeaponsToCorrectInventory(__state);
        }
    }


    [HarmonyPatch(typeof(Caravan_TraderTracker), "ColonyThingsWillingToBuy")]
    public class Caravan_TraderTracker_ColonyThingsWillingToBuy
    {
        [HarmonyPostfix]
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn playerNegotiator)
        {
            // get all pawns in the caravan
            var pawns = playerNegotiator?.GetCaravan()?.pawns;
            if (pawns?.Count > 0)
                return CaravanUtility.RemoveRememberedWeaponsFromThingList(__result, pawns);

            // just in case there is no caravan for some reason
            return __result;
        }
    }
    [HarmonyPatch(typeof(Settlement_TraderTracker), "ColonyThingsWillingToBuy")]
    public class Settlement_TraderTracker_ColonyThingsWillingToBuy
    {
        [HarmonyPostfix]
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn playerNegotiator)
        {
            // get all pawns in the caravan
            var pawns = playerNegotiator?.GetCaravan()?.pawns;
            if (pawns?.Count > 0)
                return CaravanUtility.RemoveRememberedWeaponsFromThingList(__result, pawns);

            // just in case there is no caravan for some reason
            return __result;
        }
    }

    // TODO: Also patch Dialog_SplitCaravan.AddItemsToTransferables or rather AddToTransferables to hide remembered weapons from the Split-Caravan dialog?


    public static class CaravanUtility
    {
        public static void TransferWeaponsToCorrectInventory(List<Pawn> pawns)
        {
            if (!(pawns?.Count > 0))
                return;

            var missingWeaponsForPawn = new Dictionary<Pawn, List<ThingDefStuffDefPair>>();
            var availableThings = new List<ThingWithComps>();
            var availableBiocodedThings = new Dictionary<Pawn, List<ThingWithComps>>();
            foreach (var pawn in pawns)
            {
                var pawnThings = new List<ThingWithComps>();

                if (pawn.equipment?.Primary != null)
                    pawnThings.Add(pawn.equipment.Primary);
                var inventory = pawn.inventory?.innerContainer;
                if (inventory != null)
                {
                    foreach (var thing in inventory)
                        if ((thing.def.IsMeleeWeapon || thing.def.IsRangedWeapon) 
                            && thing is ThingWithComps thingWithComps)
                            pawnThings.Add(thingWithComps);
                }

                // check if we are looking at a colonist with an inventory
                if (pawn?.IsColonist == true && inventory != null)
                {
                    var pawnWeapons = new List<ThingDefStuffDefPair>(CompSidearmMemory.GetMemoryCompForPawn(pawn)?.RememberedWeapons);

                    // first remove things biocoded to this pawn; no-one else can use them anyway
                    for (int i = 0; i < pawnThings.Count;)
                    {
                        var thing = pawnThings[i];

                        // check if thing is biocoded to this pawn
                        var biocode = thing.TryGetComp<CompBiocodable>();
                        if (biocode?.Biocoded == true)
                        {
                            if (biocode.CodedPawn == pawn)
                            {
                                // remove from remembered weapon list
                                var weaponMemory = thing.toThingDefStuffDefPair();
                                if (weaponMemory != null)
                                    pawnWeapons.Remove(weaponMemory);
                            }
                            else
                            {
                                // add biocoded thing to their own list
                                var codedPawn = biocode.CodedPawn;
                                if (!availableBiocodedThings.ContainsKey(codedPawn))
                                    availableBiocodedThings.Add(codedPawn, new List<ThingWithComps>());
                                availableBiocodedThings[codedPawn].Add(thing);
                            }

                            // remove from thing list
                            pawnThings.Remove(thing);
                            continue;
                        }
                        i++;
                    }

                    // then remove things that fit the remembered weapons found on the pawn
                    if (pawnWeapons.Count > 0)
                    {
                        for (int i = 0; i < pawnThings.Count;)
                        {
                            var thing = pawnThings[i];

                            // check if thing is remembered
                            var weaponMemory = thing.toThingDefStuffDefPair();
                            if (weaponMemory != null && pawnWeapons.Contains(weaponMemory))
                            {
                                // remove remembered weapon and the thing that fits it
                                pawnWeapons.Remove(weaponMemory);
                                pawnThings.Remove(thing);
                                continue;
                            }
                            i++;
                        }
                    }

                    // finally all weapons still in the pawn's remembered weapons list are missing; we will look for them on other pawns
                    if (pawnWeapons.Count > 0 && !missingWeaponsForPawn.ContainsKey(pawn))
                        missingWeaponsForPawn.Add(pawn, new List<ThingDefStuffDefPair>());
                    foreach (var weapon in pawnWeapons)
                        missingWeaponsForPawn[pawn].Add(weapon);
                }

                // all things in the pawn's thing list are not remembered weapons for this pawn; they can be used by other pawns
                foreach (var thing in pawnThings)
                    availableThings.Add(thing);
            }

            // sort by quality; this way we should give every pawn the best weapon, unless they got a biocoded one
            availableThings.SortByDescending((thing) =>
            {
                QualityUtility.TryGetQuality(thing, out QualityCategory qc);
                return (int)qc;
            });

            // transfer unassigned weapons to pawns missing sidearms
            foreach (var entry in missingWeaponsForPawn)
            {
                var pawn = entry.Key;
                var missingWeapons = entry.Value;

                List<ThingWithComps> biocodedToThisPawn = null;
                if (availableBiocodedThings.ContainsKey(pawn))
                    biocodedToThisPawn = availableBiocodedThings[pawn];

                // iterate over every missing weapon
                for (int i = 0; i < missingWeapons.Count;)
                {
                    var weaponMemory = missingWeapons[i];

                    // check available biocoded weapons first
                    if (biocodedToThisPawn != null)
                    {
                        // iterate over biocoded items
                        for (int j = 0; biocodedToThisPawn.Count > j;)
                        {
                            // get ThingDefStuffDefPair for thing
                            var thing = biocodedToThisPawn[j];
                            var thingMemory = thing.toThingDefStuffDefPair();

                            // check if thing fits weapon memory
                            if (weaponMemory.Equals(thingMemory))
                            {
                                Log.Message($"Transferring '{thing}' from '{ThingOwnerUtility.GetAnyParent<Pawn>(thing)}' ({thing.ParentHolder}) to '{pawn}' (biocoded)");
                                // transfer weapon
                                if (thing.holdingOwner.TryTransferToContainer(thing, pawn.inventory.innerContainer, 1) == 1)
                                {
                                    // remove weapon from missing weapons list and thing from unassigned things list
                                    missingWeapons.Remove(weaponMemory);
                                    biocodedToThisPawn.Remove(thing);
                                    goto CONTINUE;
                                }

                                // if it gets here, tranferring the weapon failed; this should obviously not happen
                                Log.Error($"Failed to transfer '{thing}' from '{ThingOwnerUtility.GetAnyParent<Pawn>(thing)}' ({thing.ParentHolder}) to '{pawn}'! (biocoded)");
                            }
                            j++;
                        }
                    }

                    // for each missing weapon, we check all things to find it
                    for (int j = 0; j < availableThings.Count;)
                    {
                        // get ThingDefStuffDefPair for thing
                        var thing = availableThings[j];
                        var thingMemory = thing.toThingDefStuffDefPair();

                        // all things in this list should have a ThingDefStuffDefPair, remove it if it does not
                        if (thingMemory == null)
                        {
                            Log.Warning($"'{thing}' had null ThingDefStuffDefPair; removing from unassigned things");
                            availableThings.Remove(thing);
                            continue;
                        }

                        // check if thing fits weapon memory
                        if (weaponMemory.Equals(thingMemory))
                        {
                            Log.Message($"Transferring '{thing}' from '{ThingOwnerUtility.GetAnyParent<Pawn>(thing)}' ({thing.ParentHolder}) to '{pawn}' (standard)");
                            // transfer weapon
                            if (thing.holdingOwner.TryTransferToContainer(thing, pawn.inventory.innerContainer, 1) == 1)
                            {
                                // remove weapon from missing weapons list and thing from unassigned things list
                                missingWeapons.Remove(weaponMemory);
                                availableThings.Remove(thing);
                                goto CONTINUE;
                            }

                            // if it gets here, tranferring the weapon failed; this should obviously not happen
                            Log.Error($"Failed to transfer '{thing}' from '{ThingOwnerUtility.GetAnyParent<Pawn>(thing)}' ({thing.ParentHolder}) to '{pawn}'! (standard)");
                        }
                        j++;
                    }
                    i++;

                    // if missing weapon was found go here without increasing the index
                    CONTINUE:;
                }
            }
        }

        public static IEnumerable<Thing> RemoveRememberedWeaponsFromThingList(IEnumerable<Thing> things, IEnumerable<Pawn> pawns)
        {
            // get remembered weapons for all pawns
            var rememberedNonEquippedCount = new Dictionary<ThingDefStuffDefPair, int>();
            foreach (var pawn in pawns)
            {
                // check if the pawn is a colonist
                if (!pawn.IsColonist)
                    continue;

                // retrieve siderarm memory
                var memory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (memory == null)
                    continue;

                // iterate over every remembered weapon
                foreach (var weapon in memory.RememberedWeapons)
                {
                    // ignore equipped remembered weapons
                    if (pawn.equipment?.Primary?.matchesThingDefStuffDefPair(weapon) == true)
                        continue;
                           
                    // count remembered weapons in inventory
                    if (rememberedNonEquippedCount.ContainsKey(weapon))
                        rememberedNonEquippedCount[weapon]++;
                    else
                        rememberedNonEquippedCount[weapon] = 1;
                }
            }

            // sort items by quality so the highest quality items will be reserved for pawns
            var sorted = things.ToList();
            sorted.SortByDescending((thing) =>
            {
                QualityUtility.TryGetQuality(thing, out QualityCategory qc);
                return (int)qc; // NOTE: what about high quality weapons with low hitpoints?
            });

            // check for biocoded weapons and remove them from the list if the biocoded pawns are part of the caravan and remembers them
            for (int i = 0; i < sorted.Count;)
            {
                var thing = sorted[i];
                // check if thing is biocodable, is biocoded and if the pawn whom it is biocoded to is part of the caravan
                if (thing.TryGetComp<CompBiocodable>() is CompBiocodable biocode 
                    && biocode.Biocoded 
                    && biocode.CodedPawn is Pawn pawn
                    && pawns.Contains(pawn))
                {
                    // if the pawn whom this weapon is biocoded to remembers this weapon, remove it from the output
                    var pair = thing.toThingDefStuffDefPair();
                    var memory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                    if (memory != null && memory.RememberedWeapons.Contains(pair))
                    {
                        // if the pawn does not have the weapon in their inventory, also decrease the non-equipped count (it is already taken care of)
                        if (pawn.equipment.Primary != thing && !pawn.inventory.innerContainer.Contains(thing))
                            rememberedNonEquippedCount[pair]--;

                        // remove thing from output
                        sorted.Remove(thing);
                        continue;
                    }
                }
                i++;
            }

            // iterate over quality-sorted list of things in caravan inventory
            foreach (var thing in sorted)
            {
                // if thing is a remembered weapon, skip it; this removes it from the output
                var stuffDefPair = thing.toThingDefStuffDefPair();
                if (stuffDefPair != null
                    && rememberedNonEquippedCount.ContainsKey(stuffDefPair)
                    && rememberedNonEquippedCount[stuffDefPair] > 0)
                {
                    //Log.Message($"'{thing}' is in inventory and remembered, removing from item list");
                    rememberedNonEquippedCount[stuffDefPair]--;
                    continue;
                }

                // otherwise return it
                yield return thing;
            }
        }
    }
}


