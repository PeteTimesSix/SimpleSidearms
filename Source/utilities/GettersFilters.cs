using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using static SimpleSidearms.Globals;
using static SimpleSidearms.hugsLibSettings.SettingsUIs;
using static SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.utilities
{
    public static class GettersFilters
    {
        internal static void getHeaviestWeapons(out float weightMelee, out float weightRanged)
        {
            weightMelee = float.MinValue;
            weightRanged = float.MinValue;
            foreach (ThingStuffPair weapon in ThingStuffPair.AllWith(t => t.IsWeapon))
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

        private static IEnumerable<ThingStuffPair> pregenedValidWeapons;

        public static IEnumerable<ThingStuffPair> getValidWeapons()
        {
            if (pregenedValidWeapons == null)
                pregenedValidWeapons = ThingStuffPair.AllWith(t => t.IsWeapon && t.weaponTags != null && t.PlayerAcquirable);
            return pregenedValidWeapons;
        }

        public static IEnumerable<ThingDef> getValidWeaponsThingDefsOnly()
        {
            return getValidWeapons().ToList().ConvertAll(t => t.thing).Distinct();
        }

        public static IEnumerable<ThingStuffPair> getValidSidearms()
        {
            return getValidWeapons().Where(w => StatCalculator.isValidSidearm(w, out _));
        }

        public static IEnumerable<ThingDef> getValidSidearmsThingDefsOnly()
        {
            return getValidSidearms().ToList().ConvertAll(t => t.thing).Distinct();
        }


        /*internal static List<ThingStuffPair> filterForWeaponType(IEnumerable<ThingStuffPair> weapons, WeaponSearchType type, bool allowThingDuplicates)
        {
            List<ThingStuffPair> filteredWeapons = new List<ThingStuffPair>();
            foreach(ThingStuffPair weapon in weapons)
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                switch (type)
                {
                    case WeaponSearchType.MeleeCapable:
                        if ((weapon.thing.IsMeleeWeapon || (weapon.thing.tools != null && weapon.thing.tools.Any((Tool x) => x.VerbsProperties.Any((VerbProperties y) => y.IsMeleeAttack)))) & (allowThingDuplicates || !filteredWeapons.Contains(weapon)))
                        {
                            filteredWeapons.Add(weapon);
                        }
                        break;
                    case WeaponSearchType.Melee:
                        if (weapon.thing.IsMeleeWeapon & (allowThingDuplicates || !filteredWeapons.Contains(weapon)))
                        {
                            filteredWeapons.Add(weapon);
                        }
                        break;
                    case WeaponSearchType.Ranged:
                        if (weapon.thing.IsRangedWeapon & (allowThingDuplicates || !filteredWeapons.Contains(weapon)))
                        {
                            filteredWeapons.Add(weapon);
                        }
                        break;
                    case WeaponSearchType.Both:
                    default:
                        if ((allowThingDuplicates || !filteredWeapons.Contains(weapon)))
                        {
                            filteredWeapons.Add(weapon);
                        }
                        break;
                }
            }
            return filteredWeapons;
        }*/

        /*internal static List<Thing> getWeaponsOfType(Pawn pawn, WeaponSearchType type)
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
        }*/

        /*

        internal static ThingWithComps findBestRangedWeapon(Pawn pawn, bool skipDangerous)
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
        }*/

        internal static IEnumerable<ThingStuffPair> filterForWeaponKind(IEnumerable<ThingStuffPair> options, WeaponSearchType type)
        {
            switch (type)
            {
                case WeaponSearchType.Melee:
                    return options.Where(t => t.thing.IsMeleeWeapon);
                case WeaponSearchType.Ranged:
                    return options.Where(t => t.thing.IsRangedWeapon);
                case WeaponSearchType.MeleeCapable:
                    return options.Where(t => t.thing.IsMeleeWeapon || (t.thing.IsWeapon && !t.thing.tools.NullOrEmpty()));
                case WeaponSearchType.Both:
                default:
                    return options.Where(t => t.thing.IsWeapon);
            }
        }

        internal static IEnumerable<ThingWithComps> filterForWeaponKind(IEnumerable<ThingWithComps> options, WeaponSearchType type)
        {
            switch (type)
            {
                case WeaponSearchType.Melee:
                    return options.Where(t => t.def.IsMeleeWeapon);
                case WeaponSearchType.Ranged:
                    return options.Where(t => t.def.IsRangedWeapon);
                case WeaponSearchType.MeleeCapable:
                    return options.Where(t => t.def.IsMeleeWeapon || (t.def.IsWeapon && !t.def.tools.NullOrEmpty()));
                case WeaponSearchType.Both:
                default:
                    return options.Where(t => t.def.IsWeapon);
            }
        }

        internal static float AverageSpeedRanged(IEnumerable<Thing> options)
        {
            int i = 0;
            float total = 0;
            foreach (Thing thing in options)
            {
                total += StatCalculator.RangedSpeed(thing as ThingWithComps);
                i++;
            }
            if (i > 0)
                return total / i;
            else
                return 0;
        }

        /*
        internal static ThingWithComps findBestRangedWeaponAtRange(Pawn pawn, float range, bool skipDangerous, out float resultDPS, out float averageSpeed)
        {
            if (pawn == null || pawn.Dead || pawn.equipment == null || pawn.inventory == null)
            {
                resultDPS = -1;
                averageSpeed = 0;
                return null;
            }

            IEnumerable<Thing> options = pawn.inventory.innerContainer.Where(t => t.def.IsRangedWeapon);


            float bestSoFar = float.MinValue;
            Thing best = null;

            averageSpeed = AverageSpeedRanged(options); 

            foreach (Thing thing in options)
            {
                if (!(thing is ThingWithComps))
                    continue;

                if (skipDangerous)
                    if (isDangerousWeapon(thing as ThingWithComps))
                        continue;

                float DPS = StatCalculator.RangedDPS(thing as ThingWithComps, SpeedSelectionBiasRanged.Value, averageSpeed, range);

                if (DPS > bestSoFar)
                {
                    bestSoFar = DPS;
                    best = thing;
                }
            }
            resultDPS = bestSoFar;
            return best as ThingWithComps;
        }*/

        internal static (ThingWithComps weapon, float dps, float averageSpeed) findBestRangedWeapon(Pawn pawn, LocalTargetInfo? target = null, bool skipDangerous = true, bool includeEquipped = true)
        {
            if (pawn == null || pawn.Dead || pawn.equipment == null || pawn.inventory == null)
                return (null,-1, 0);

            IEnumerable<ThingWithComps> options = pawn.getCarriedWeapons(includeEquipped).Where(t =>
            {
                return t.def.IsRangedWeapon;
            });

            if (options.Count() == 0)
                return (null, -1, 0);

            float averageSpeed = AverageSpeedRanged(options);

            if (target.HasValue)
            {
                //TODO: handle DPS vs. armor?
                CellRect cellRect = (!target.Value.HasThing) ? CellRect.SingleCell(target.Value.Cell) : target.Value.Thing.OccupiedRect();
                float range = cellRect.ClosestDistSquaredTo(pawn.Position);
                (ThingWithComps thing, float dps, float averageSpeed) best = (null, -1 , averageSpeed);
                foreach(ThingWithComps candidate in options) 
                {
                    float dps = StatCalculator.RangedDPS(candidate, SpeedSelectionBiasRanged.Value, averageSpeed, range);
                    if(dps > best.dps) 
                    {
                        best = (candidate, dps, averageSpeed);
                    }
                }
                return best;
            }
            else
            {
                (ThingWithComps thing, float dps, float averageSpeed) best = (null, -1, averageSpeed);
                foreach (ThingWithComps candidate in options)
                {
                    float dps = StatCalculator.RangedDPSAverage(candidate, SpeedSelectionBiasRanged.Value, averageSpeed);
                    if (dps > best.dps)
                    {
                        best = (candidate, dps, averageSpeed);
                    }
                }
                return best;
            }
        }

        internal static float AverageSpeedMelee(IEnumerable<Thing> options, Pawn pawn)
        {
            int i = 0;
            float total = 0;
            foreach (Thing thing in options)
            {
                total += StatCalculator.MeleeSpeed(thing as ThingWithComps, pawn);
                i++;
            }
            if (i > 0)
                return total / i;
            else
                return 0;
        }

        internal static bool findBestMeleeWeapon(Pawn pawn, out ThingWithComps result, bool includeEquipped = true, bool includeRangedWithBash = true, Pawn target = null)
        {
            result = null;
            if (pawn == null || pawn.Dead || pawn.equipment == null || pawn.inventory == null)
                return false;

            IEnumerable<Thing> options = pawn.getCarriedWeapons(includeEquipped).Where(t =>
            {
            return 
                t.def.IsMeleeWeapon ||
                (includeRangedWithBash && t.def.IsWeapon && !t.def.tools.NullOrEmpty());
            });

            if (options.Count() < 1)
                return false;

            float averageSpeed = AverageSpeedMelee(options, pawn);

            /*if (target != null)
            {
                //handle DPS vs. armor?
            }
            else*/
            {
                float resultDPS = options.Max(t => StatCalculator.getMeleeDPSBiased(t as ThingWithComps, pawn, SpeedSelectionBiasMelee.Value, averageSpeed));
                result = options.MaxBy(t => StatCalculator.getMeleeDPSBiased(t as ThingWithComps, pawn, SpeedSelectionBiasMelee.Value, averageSpeed)) as ThingWithComps;

                //check if pawn is better when punching
                if (pawn.GetStatValue(StatDefOf.MeleeDPS) > resultDPS)
                    result = null;

                return true;
            }
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
