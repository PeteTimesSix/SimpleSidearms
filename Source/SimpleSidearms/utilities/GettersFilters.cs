using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static SimpleSidearms.Globals;
using static SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.utilities
{
    public static class GettersFilters
    {
        internal static void getWeaponLists(out List<ThingWithComps> ranged, out List<ThingWithComps> melee, Pawn_InventoryTracker inventory)
        {
            ranged = new List<ThingWithComps>();
            melee = new List<ThingWithComps>();

            foreach (ThingWithComps item in inventory.innerContainer)
            {
                if (item.def.IsRangedWeapon)
                    ranged.Add(item);
                else if (item.def.IsMeleeWeapon)
                    melee.Add(item);
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
                if (type == WeaponSearchType.Both || type == WeaponSearchType.Ranged)
                {
                    if (thing.def.IsRangedWeapon)
                    {
                        things.Add(thing);
                    }

                }
                if (type == WeaponSearchType.Both || type == WeaponSearchType.Melee)
                {
                    if (thing.def.IsMeleeWeapon)
                    {
                        things.Add(thing);
                    }
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

                //Log.Message("DPS for " + thing.LabelShort + " is " + DPS);

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
            foreach(string tag in weaponTags)
            {
                switch (tag)
                {
                    case "Gun":
                        sidearmTags.Add("Gun");
                        sidearmTags.Add("Melee");
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
            }
            return sidearmTags.ToList();
        }

        internal static ThingWithComps findBestWeapon(Pawn pawn, bool ranged, bool skipDangerous/*, SelectionMode mode*/)
        {
            List<Thing> weapons;
            if (!ranged)
            {
                weapons = getWeaponsOfType(pawn, WeaponSearchType.Melee);
            }
            else
            {
                weapons = getWeaponsOfType(pawn, WeaponSearchType.Ranged);
            }

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
                else
                {
                    dpsAvg = StatCalculator.MeleeDPS(thing as ThingWithComps, SpeedSelectionBiasMelee.Value);
                }
                if (dpsAvg > bestSoFar)
                {
                    bestSoFar = dpsAvg;
                    best = thing;
                }
            }

            return best as ThingWithComps;

        }

        internal static bool isDangerousWeapon(ThingWithComps weapon)
        {
            CompEquippable equip = weapon.TryGetComp<CompEquippable>();
            if (equip == null)
                return false;
            if (equip.PrimaryVerb.verbProps.ai_IsIncendiary || equip.PrimaryVerb.verbProps.onlyManualCast || equip.PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
                return true;
            else
                return false;
        }
    }
}
