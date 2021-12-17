using HarmonyLib;
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
            Log.Message("CaravanEnterMapUtility_Enter");
            if (__state?.Count > 0)
            {
                // check every pawn for missing sidearms and transfer ones picked up by other pawns back to the correct inventory
                foreach (var pawn in __state)
                    CarvanEquipmentUtility.TransferSidearmsToCorrectInventory(pawn, __state);
            }
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
            Log.Message("Dialog_SplitCaravan_TrySplitCaravan");
            if (__state?.Count > 0)
            {
                // check every pawn for missing sidearms and transfer ones picked up by other pawns back to the correct inventory
                foreach (var pawn in __state)
                    CarvanEquipmentUtility.TransferSidearmsToCorrectInventory(pawn, __state);
            }
        }
    }

    public static class CarvanEquipmentUtility
    {
        public static void TransferSidearmsToCorrectInventory(Pawn pawn, List<Pawn> allPawns)
        {
            // only check colonists (who have an inventory)
            if (pawn?.IsColonist == true && pawn.inventory?.innerContainer != null)
            {
                var dupeCounters = new Dictionary<ThingDefStuffDefPair, int>();

                // retrieve sidearm memory for pawn
                var memory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (memory == null)
                {
                    Log.Warning($"Could not retrieve '{nameof(CompSidearmMemory)}' for '{pawn}'");
                    return;
                }

                // check if pawn has all their weapons
                foreach (var weaponMemory in memory.RememberedWeapons)
                {
                    // remember how many of this weapon type the pawn should have
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;
                    else
                        dupeCounters[weaponMemory]++;

                    // only look for missing weapons
                    if (pawn.hasWeaponType(weaponMemory, dupeCounters[weaponMemory]))
                        continue;
                    //Log.Message($"'{pawn}' is missing '{weaponMemory.thing.defName}'");

                    // check all other caravan pawns for the weapon we are looking for
                    foreach (var other in allPawns)
                    {
                        // skip other if they are the pawn we are looking at or if they do not have an inventory
                        if (other == pawn || !(other.inventory?.innerContainer?.Count() > 0))
                            continue;

                        // check for remembered weapons if pawn is humanlike
                        int rememberedByOther = 0;
                        bool equipped = false;
                        if (other.def.race?.Humanlike == true)
                        {
                            // retrieve sidearm memory for other
                            var otherMemory = CompSidearmMemory.GetMemoryCompForPawn(other);
                            if (otherMemory != null)
                            {
                                // count all instances of weapon remembered by other
                                foreach (var otherWeapon in otherMemory.RememberedWeapons)
                                {
                                    if (otherWeapon.Equals(weaponMemory))
                                        rememberedByOther++;
                                }
                                // check if other has matching weapon equipped
                                if (other.equipment.Primary.matchesThingDefStuffDefPair(weaponMemory))
                                    equipped = true;
                                //Log.Message($"'{other}' remembers {rememberedByOther} '{weaponMemory.thing}' (equipped: {equipped})");
                            }
                        }

                        // iterate over all things in inventory
                        foreach (var thing in other.inventory.innerContainer)
                        {
                            // check if thing is weapon we are looking for and is not remembered by other
                            if (thing.matchesThingDefStuffDefPair(weaponMemory) && rememberedByOther-- == (equipped ? 1 : 0))
                            {
                                // transfer thing found in inventory of other to pawn
                                //Log.Message($"Transfer '{thing}' from '{other}' to '{pawn}' (inventory)");
                                other.inventory.innerContainer.TryTransferToContainer(thing, pawn.inventory.innerContainer, 1);
                                goto CONTINUE;
                            }
                        }
                        // other has weapon equipped, but does not remember it
                        if (equipped && rememberedByOther == 0)
                        {
                            // transfer primary of other to pawn
                            var thing = other.equipment.Primary;
                            //Log.Message($"Transfer '{thing}' from '{other}' to '{pawn}' (primary)");
                            thing.holdingOwner.TryTransferToContainer(thing, pawn.inventory.innerContainer, 1);
                            goto CONTINUE;
                        }
                    }
                    //Log.Message($"'{pawn}' could not find '{weaponMemory.thing.defName}' in any inventory");
                    // continue with next weapon
                    CONTINUE:;
                }
            }
        }
    }
}


