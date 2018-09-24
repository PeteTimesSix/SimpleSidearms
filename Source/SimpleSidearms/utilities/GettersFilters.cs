using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using static SimpleSidearms.Globals;
using static SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.utilities
{
    public static class GettersFilters
    {
        internal static void getHeaviestWeapons(List<ThingStuffPair> list, out float weightMelee, out float weightRanged)
        {
            weightMelee = float.MinValue;
            weightRanged = float.MinValue;
            foreach (ThingStuffPair weapon in list)
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                float mass = weapon.thing.GetStatValueAbstract(StatDefOf.Mass);
                if (weapon.thing.IsRangedWeapon)
                {
                    if (mass > weightRanged)
                        weightRanged = mass;
                }
                else if (weapon.thing.IsMeleeWeapon)
                {
                    if (mass > weightMelee)
                        weightMelee = mass;
                }
            }
        }

        internal static void getWeaponLists(out List<Thing> ranged, out List<Thing> melee, Pawn_InventoryTracker inventory)
        {
            ranged = new List<Thing>();
            melee = new List<Thing>();

            foreach (Thing item in inventory.innerContainer)
            {
                if (item.def.IsRangedWeapon)
                    ranged.Add(item);
                else if (item.def.IsMeleeWeapon)
                    melee.Add(item);
            }
        }

        internal static List<ThingStuffPair> filterForType(List<ThingStuffPair> list, WeaponSearchType type, bool allowThingDuplicates)
        {
            List<ThingStuffPair> returnList = new List<ThingStuffPair>();
            List<ThingDef> things = new List<ThingDef>();
            foreach(ThingStuffPair weapon in list)
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                switch (type)
                {
                    case WeaponSearchType.MeleeCapable:
                        if ((weapon.thing.IsMeleeWeapon || (weapon.thing.tools != null && weapon.thing.tools.Any((Tool x) => x.VerbsProperties.Any((VerbProperties y) => y.IsMeleeAttack)))) & (allowThingDuplicates || !things.Contains(weapon.thing)))
                        {
                            returnList.Add(weapon);
                            things.Add(weapon.thing);
                        }
                        break;
                    case WeaponSearchType.Melee:
                        if (weapon.thing.IsMeleeWeapon & (allowThingDuplicates || !things.Contains(weapon.thing)))
                        {
                            returnList.Add(weapon);
                            things.Add(weapon.thing);
                        }
                        break;
                    case WeaponSearchType.Ranged:
                        if (weapon.thing.IsRangedWeapon & (allowThingDuplicates || !things.Contains(weapon.thing)))
                        {
                            returnList.Add(weapon);
                            things.Add(weapon.thing);
                        }
                        break;
                    case WeaponSearchType.Both:
                    default:
                        if ((allowThingDuplicates || !things.Contains(weapon.thing)))
                        {
                            returnList.Add(weapon);
                            things.Add(weapon.thing);
                        }
                        break;
                }
            }
            return returnList;
        }

        internal static void excludeNeolithic(List<ThingStuffPair> list)
        {
            for (int i = list.Count - 1; i>= 0; i--)
            {
                ThingStuffPair weapon = list[i];
                if (weapon.thing.weaponTags.Contains("NeolithicMelee") || weapon.thing.weaponTags.Contains("NeolithicRanged") || weapon.thing.weaponTags.Contains("Neolithic"))
                {
                    list.RemoveAt(i);
                }
            }
        }

        internal static Pawn extractSelectedPawn()
        {
            try { 
                List<object> selection = Find.Selector.SelectedObjects;
                if (selection.Count != 1)
                    return null;
                if (!(selection[0] is ISelectable))
                    return null;
                if (!(selection[0] is Pawn))
                    return null;
                return selection[0] as Pawn;
                }
            catch(InvalidCastException e)
            {
                return null;
            }
        }

        internal static List<Thing> getWeaponsOfType(Pawn pawn, WeaponSearchType type)
        {
            List<Thing> things = new List<Thing>();
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                float weight = thing.def.BaseMass;
                switch (type)
                {
                    case WeaponSearchType.MeleeCapable:
                        if ((thing.def.IsMeleeWeapon || (thing.def.tools != null && thing.def.tools.Any((Tool x) => (x.VerbsProperties != null && x.VerbsProperties.Any((VerbProperties y) => y.IsMeleeAttack))))))
                        {
                            things.Add(thing);
                        }
                        break;
                    case WeaponSearchType.Melee:
                        if (thing.def.IsMeleeWeapon)
                        {
                            things.Add(thing);
                        }
                        break;
                    case WeaponSearchType.Ranged:
                        if (thing.def.IsRangedWeapon)
                        {
                            things.Add(thing);
                        }
                        break;
                    case WeaponSearchType.Both:
                    default:
                        if (thing.def.IsWeapon)
                        {
                            things.Add(thing);
                        }
                        break;
                }
            }
            return things;
        }

        internal static ThingWithComps findBestRangedWeaponAtRanged(Pawn pawn, float range, bool skipDangerous, out float resultDPS)
        {
            List<Thing> weapons;

            weapons = getWeaponsOfType(pawn, WeaponSearchType.Ranged);

            float bestSoFar = float.MinValue;
            Thing best = null;

            foreach (Thing thing in weapons)
            {
                if (!(thing is ThingWithComps))
                    continue;

                if (skipDangerous)
                    if (isDangerousWeapon(thing as ThingWithComps))
                        continue;

                float DPS = StatCalculator.RangedDPS(thing as ThingWithComps, SpeedSelectionBiasRanged.Value, range);
                
                if (DPS > bestSoFar)
                {
                    bestSoFar = DPS;
                    best = thing;
                }
            }
            resultDPS = bestSoFar;
            return best as ThingWithComps;
        }

        internal static List<string> weaponTagsToSidearmTags(List<string> weaponTags)
        {
            HashSet<string> sidearmTags = new HashSet<string>();

            sidearmTags.Add("Gun");
            sidearmTags.Add("Melee");
            sidearmTags.Add("NeolithicMelee");
            //because bowmen bringing pilas makes sense... machine gunners, not so much
            if (weaponTags.Contains("NeolithicRanged"))
                sidearmTags.Add("NeolithicRanged");

            /*
            foreach (string tag in weaponTags)
            {
                switch (tag)
                {
                    case "Gun":
                        break;
                    case "EliteGun":    //assault rifle, LMG
                        sidearmTags.Add("Gun");
                        sidearmTags.Add("Melee");
                        break;
                    case "AdvancedGun": //charge rifle
                        sidearmTags.Add("Gun");
                        sidearmTags.Add("Melee");
                        break;
                    case "GunHeavy":
                        sidearmTags.Add("Gun");
                        sidearmTags.Add("Melee");
                        break;
                    case "SniperRifle":
                        sidearmTags.Add("Gun");
                        sidearmTags.Add("Melee");
                        break;
                    case "Melee":
                        sidearmTags.Add("Melee");
                        break;
                    case "NeolithicMelee":
                        sidearmTags.Add("NeolithicMelee");
                        break;
                    case "NeolithicRanged":
                        sidearmTags.Add("NeolithicMelee");
                        break;
                    case "GrenadeDestructive":
                        break;
                    case "GrenadeEMP":
                        break;
                }
            }*/
            return sidearmTags.ToList();
        }

        internal static ThingWithComps findBestRangedWeapon(Pawn pawn, bool skipDangerous/*, SelectionMode mode*/)
        {
            List<Thing> weapons = getWeaponsOfType(pawn, WeaponSearchType.Ranged);

            float bestSoFar = float.MinValue;
            Thing best = null;

            foreach (Thing thing in weapons)
            {
                if (!(thing is ThingWithComps))
                    continue;

                if (skipDangerous)
                    if (isDangerousWeapon(thing as ThingWithComps))
                        continue;

                float dpsAvg = -1f;

                if (thing.def.IsRangedWeapon)
                {
                    dpsAvg = StatCalculator.RangedDPSAverage(thing as ThingWithComps, SpeedSelectionBiasRanged.Value);
                }
                if (dpsAvg > bestSoFar)
                {
                    bestSoFar = dpsAvg;
                    best = thing;
                }
            }

            return best as ThingWithComps;
        }

        internal static ThingWithComps findBestMeleeWeapon(Pawn pawn, bool skipDangerous/*, SelectionMode mode*/, Pawn target)
        {
            List<Thing> weapons = getWeaponsOfType(pawn, WeaponSearchType.MeleeCapable);

            if (pawn.equipment != null && pawn.equipment.Primary != null)
                weapons.Add(pawn.equipment.Primary);
            weapons.Add(null);  //for considering unarmed attacks

            /*string candList = "";
            foreach (Thing thing in weapons)
                candList += (thing == null) ? " unarmed" : " " + thing.def.defName;
            Log.Message("Considering melee swap, candidates:"+candList);*/

            Thing best = null;
            float bestSoFar = float.MinValue;
            
            foreach (Thing thing in weapons)
            {
                if (skipDangerous)
                    if (isDangerousWeapon(thing as ThingWithComps))
                        continue;

                float dpsAvg = -1f;

                dpsAvg = StatCalculator.MeleeDPS(pawn, thing as ThingWithComps, SpeedSelectionBiasMelee.Value, target);

                /*if(thing == null)
                    Log.Message("DPS for unarmed is " + dpsAvg);
                else
                    Log.Message("DPS for " + thing.def.defName + " is " + dpsAvg);*/

                if (dpsAvg > bestSoFar)
                {
                    bestSoFar = dpsAvg;
                    best = thing;
                }

            }

            /*if (best == null)
                Log.Message("best: unarmed");
            else
                Log.Message("best: " + best.def.defName);*/
            
            return best as ThingWithComps;
        }

        internal static bool isDangerousWeapon(ThingWithComps weapon)
        {
            if (weapon == null)
                return false;
            CompEquippable equip = weapon.TryGetComp<CompEquippable>();
            if (equip == null)
                return false;
            if (equip.PrimaryVerb.IsIncendiary() || equip.PrimaryVerb.verbProps.onlyManualCast || equip.PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
                return true;
            else
                return false;
        }
    }
}
