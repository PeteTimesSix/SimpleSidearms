using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using static SimpleSidearms.Globals;
using static SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.utilities
{
    public static class StatCalculator
    {
        public static int countForLimitType(Pawn pawn, WeaponSearchType type)
        {
            return GettersFilters.filterForWeaponKind(pawn.getCarriedWeapons(), type).Count();
        }

        public static float weightForLimitType(Pawn pawn, WeaponSearchType type)
        {
            float total = 0;
            IEnumerable<ThingWithComps> weapons = GettersFilters.filterForWeaponKind(pawn.getCarriedWeapons(), type);
            foreach (ThingWithComps thing in weapons)
            {
                switch (type)
                {
                    case WeaponSearchType.MeleeCapable:
                        if ((thing.def.IsMeleeWeapon || (thing.def.tools != null && thing.def.tools.Any((Tool x) => x.VerbsProperties.Any((VerbProperties y) => y.IsMeleeAttack)))))
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                    case WeaponSearchType.Melee:
                        if (thing.def.IsMeleeWeapon)
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                    case WeaponSearchType.Ranged:
                        if (thing.def.IsRangedWeapon)
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                    case WeaponSearchType.Both:
                    default:
                        if (thing.def.IsWeapon)
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                }
            }
            return total;
        }

        public static bool isValidSidearm(ThingDefStuffDefPair sidearm, out string errString)
        {
            float sidearmWeight = sidearm.thing.GetStatValueAbstract(StatDefOf.Mass, sidearm.stuff);

            if (!SimpleSidearms.SeparateModes)
            {
                switch (SimpleSidearms.LimitModeSingle.Value)
                {
                    case LimitModeSingleSidearm.AbsoluteWeight:
                        if (sidearmWeight >= SimpleSidearms.LimitModeSingle_Absolute.Value)
                        {
                            errString = "SidearmPickupFail_TooHeavyForSidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.Selection:
                        if (!SimpleSidearms.LimitModeSingle_Selection.Value.InnerList.Contains<ThingDef>(sidearm.thing))
                        {
                            errString = "SidearmPickupFail_NotASidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.None:
                    default:
                        break;
                }
            }
            else
            {
                if (sidearm.thing.IsMeleeWeapon)
                {
                    switch (SimpleSidearms.LimitModeSingleMelee.Value)
                    {
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= SimpleSidearms.LimitModeSingleMelee_Absolute.Value)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection:
                            if (!SimpleSidearms.LimitModeSingleMelee_Selection.Value.InnerList.Contains<ThingDef>(sidearm.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.None:
                        default:
                            break;
                    }
                }
                else if(sidearm.thing.IsRangedWeapon)
                {
                    switch (SimpleSidearms.LimitModeSingleRanged.Value)
                    {
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= SimpleSidearms.LimitModeSingleRanged_Absolute.Value)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection:
                            if (!SimpleSidearms.LimitModeSingleRanged_Selection.Value.InnerList.Contains<ThingDef>(sidearm.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.None:
                        default:
                            break;
                    }
                }
            }
            errString = "SidearmPickupPass".Translate();
            return true;
        }

        public static bool canCarrySidearmInstance(ThingWithComps sidearmThing, Pawn pawn, out string errString)
        {
            //nicked from EquipmentUtility.CanEquip 
            CompBladelinkWeapon compBladelinkWeapon = sidearmThing.TryGetComp<CompBladelinkWeapon>();
            if (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn != null && compBladelinkWeapon.bondedPawn != pawn)
            {
                errString = "BladelinkBondedToSomeoneElse".Translate();
                return false;
            }
            if (EquipmentUtility.IsBiocoded(sidearmThing) && pawn != sidearmThing.TryGetComp<CompBiocodableWeapon>().CodedPawn)
            {
                errString = "BiocodedCodedForSomeoneElse".Translate();
                return false;
            }
            if (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn == null)
            {
                errString = "SidearmPickupFail_NotYetBladelinkBonded".Translate();
                return false;
            }

            ThingDefStuffDefPair sidearm = sidearmThing.toThingDefStuffDefPair();
            
            return canCarrySidearmType(sidearm, pawn, out errString);
        }

        public static bool canCarrySidearmType(ThingDefStuffDefPair sidearm, Pawn pawn, out string errString)
        {
            float maxCapacity = MassUtility.Capacity(pawn);
            float freeCapacity = MassUtility.FreeSpace(pawn);
            float sidearmWeight = sidearm.thing.GetStatValueAbstract(StatDefOf.Mass, sidearm.stuff);

            if (((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0) && (!sidearm.isTool()))
            {
                errString = "SidearmPickupFail_NotAToolForPacifist".Translate(pawn.LabelShort);
                return false;
            }

            //this is duplicated in the switches later but Id rather not risk accidentaly deleting a case that might come up
            if (!isValidSidearm(sidearm, out errString))
                return false;

            if (sidearmWeight >= freeCapacity)
            {
                errString = "SidearmPickupFail_NoFreeSpace".Translate();
                return false;
            }

            if (!SimpleSidearms.SeparateModes)
            {
                switch (SimpleSidearms.LimitModeSingle.Value)
                {
                    case LimitModeSingleSidearm.None:
                        break;
                    case LimitModeSingleSidearm.AbsoluteWeight:
                        if(sidearmWeight >= SimpleSidearms.LimitModeSingle_Absolute.Value)
                        {
                            errString = "SidearmPickupFail_TooHeavyForSidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.RelativeWeight:
                        if(sidearmWeight >= SimpleSidearms.LimitModeSingle_Relative.Value * maxCapacity)
                        {
                            errString = "SidearmPickupFail_TooHeavyForSidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.Selection:
                        if(!SimpleSidearms.LimitModeSingle_Selection.Value.InnerList.Contains<ThingDef>(sidearm.thing))
                        {
                            errString = "SidearmPickupFail_NotASidearm".Translate();
                            return false;
                        }
                        break;
                }
                switch (SimpleSidearms.LimitModeAmount.Value)
                {
                    case LimitModeAmountOfSidearms.MaximumCarryWeightOnly:
                        break;
                    case LimitModeAmountOfSidearms.AbsoluteWeight:
                        if (sidearmWeight >= (SimpleSidearms.LimitModeAmount_Absolute.Value - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.RelativeWeight:
                        if (sidearmWeight >= ((SimpleSidearms.LimitModeAmount_Relative.Value * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.Slots:
                        if (SimpleSidearms.LimitModeAmount_Slots.Value <= countForLimitType(pawn, WeaponSearchType.Both))
                        {
                            errString = "SidearmPickupFail_AllSlotsFull".Translate();
                            return false;
                        }
                        break;
                }
            }
            else
            {
                switch (SimpleSidearms.LimitModeAmountTotal.Value)
                {
                    case LimitModeAmountOfSidearms.MaximumCarryWeightOnly:
                        break;
                    case LimitModeAmountOfSidearms.AbsoluteWeight:
                        if (sidearmWeight >= (SimpleSidearms.LimitModeAmountTotal_Absolute.Value - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.RelativeWeight:
                        if (sidearmWeight >= ((SimpleSidearms.LimitModeAmountTotal_Relative.Value * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.Slots:
                        if (SimpleSidearms.LimitModeAmountTotal_Slots.Value <= countForLimitType(pawn, WeaponSearchType.Both))
                        {
                            errString = "SidearmPickupFail_AllSlotsFull".Translate();
                            return false;
                        }
                        break;
                }
                if (sidearm.thing.IsMeleeWeapon)
                {
                    switch (SimpleSidearms.LimitModeSingleMelee.Value)
                    {
                        case LimitModeSingleSidearm.None:
                            break;
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= SimpleSidearms.LimitModeSingleMelee_Absolute.Value)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.RelativeWeight:
                            if (sidearmWeight >= SimpleSidearms.LimitModeSingleMelee_Relative.Value * maxCapacity)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection:
                            if (!SimpleSidearms.LimitModeSingleMelee_Selection.Value.InnerList.Contains<ThingDef>(sidearm.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmMelee".Translate();
                                return false;
                            }
                            break;
                    }
                    switch (SimpleSidearms.LimitModeAmountMelee.Value)
                    {
                        case LimitModeAmountOfSidearms.MaximumCarryWeightOnly:
                            break;
                        case LimitModeAmountOfSidearms.AbsoluteWeight:
                            if (sidearmWeight >= (SimpleSidearms.LimitModeAmountMelee_Absolute.Value - weightForLimitType(pawn, WeaponSearchType.Melee)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            if (sidearmWeight >= ((SimpleSidearms.LimitModeAmountMelee_Relative.Value * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Melee)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            if (SimpleSidearms.LimitModeAmountMelee_Slots.Value <= countForLimitType(pawn, WeaponSearchType.Melee))
                            {
                                errString = "SidearmPickupFail_MeleeSlotsFull".Translate();
                                return false;
                            }
                            break;
                    }
                }
                else if(sidearm.thing.IsRangedWeapon)
                {
                    switch (SimpleSidearms.LimitModeSingleRanged.Value)
                    {
                        case LimitModeSingleSidearm.None:
                            break;
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= SimpleSidearms.LimitModeSingleRanged_Absolute.Value)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.RelativeWeight:
                            if (sidearmWeight >= SimpleSidearms.LimitModeSingleRanged_Relative.Value * maxCapacity)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection: 
                            if (!SimpleSidearms.LimitModeSingleRanged_Selection.Value.InnerList.Contains<ThingDef>(sidearm.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmRanged".Translate();
                                return false;
                            }
                            break;
                    }
                    switch (SimpleSidearms.LimitModeAmountRanged.Value)
                    {
                        case LimitModeAmountOfSidearms.MaximumCarryWeightOnly:
                            break;
                        case LimitModeAmountOfSidearms.AbsoluteWeight:
                            if (sidearmWeight >= (SimpleSidearms.LimitModeAmountRanged_Absolute.Value - weightForLimitType(pawn, WeaponSearchType.Ranged)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            if (sidearmWeight >= ((SimpleSidearms.LimitModeAmountRanged_Relative.Value * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Ranged)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            if (SimpleSidearms.LimitModeAmountRanged_Slots.Value <= countForLimitType(pawn, WeaponSearchType.Ranged))
                            {
                                errString = "SidearmPickupFail_RangedSlotsFull".Translate();
                                return false;
                            }
                            break;
                    }
                }
            }
            errString = "SidearmPickupPass".Translate();
            return true;
        }

        public static float AdjustedAccuracy(VerbProperties props, RangeCategory cat, Thing equipment)
        {
            if (equipment != null)
            {
                StatDef stat = null;
                switch (cat)
                {
                    case RangeCategory.Touch:
                        stat = StatDefOf.AccuracyTouch;
                        break;
                    case RangeCategory.Short:
                        stat = StatDefOf.AccuracyShort;
                        break;
                    case RangeCategory.Medium:
                        stat = StatDefOf.AccuracyMedium;
                        break;
                    case RangeCategory.Long:
                        stat = StatDefOf.AccuracyLong;
                        break;
                }
                return equipment.GetStatValue(stat, true);
            }
            switch (cat)
            {
                case RangeCategory.Touch:
                    return props.accuracyTouch;
                case RangeCategory.Short:
                    return props.accuracyShort;
                case RangeCategory.Medium:
                    return props.accuracyMedium;
                case RangeCategory.Long:
                    return props.accuracyLong;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static float RangedSpeed(ThingWithComps weapon)
        {
            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;
            float warmup = atkProps.warmupTime;
            float cooldown = weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
            int burstShot = atkProps.burstShotCount;
            int ticksBetweenShots = atkProps.ticksBetweenBurstShots;
            float speedFactor = (((warmup + cooldown)) + (burstShot - 1) * (ticksBetweenShots / 60f));
            return speedFactor;
        }

        public static float RangedDPSAverage(ThingWithComps weapon, float speedBias, float averageSpeed)
        {
            if (weapon == null)
                return 0;

            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;
            float damage = (atkProps.defaultProjectile == null) ? 0 : atkProps.defaultProjectile.projectile.GetDamageAmount(weapon);
            int burstShot = atkProps.burstShotCount;
            float speedFactor = RangedSpeed(weapon);
            float speedFactorBase = speedFactor;

            float diffFromAverage = speedFactor - averageSpeed;
            diffFromAverage *= (speedBias - 1);
            speedFactor += diffFromAverage;

            float rawDps = (damage * burstShot) / speedFactor;
            //Log.Message(weapon.LabelCap + " dps:" + rawDps + "dam:" + damage * burstShot + " spdfac:" + speedFactor + " spdFacBase:" + speedFactorBase);
            float DpsAvg = 0f;
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Short, weapon);
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Medium, weapon);
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Long, weapon);
            return DpsAvg / 3f;
        }

        public static float RangedDPS(ThingWithComps weapon, float speedBias, float averageSpeed, float range)
        {
            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;

            if (atkProps.range * atkProps.range < range || atkProps.minRange * atkProps.minRange > range)
                return -1;

            float hitChance = atkProps.GetHitChanceFactor(weapon, range);
            float damage = (atkProps.defaultProjectile == null) ? 0 : atkProps.defaultProjectile.projectile.GetDamageAmount(weapon);
            int burstShot = atkProps.burstShotCount;
            float speedFactor = RangedSpeed(weapon);
            float speedFactorBase = speedFactor;

            float diffFromAverage = speedFactor - averageSpeed;
            diffFromAverage *= (speedBias - 1);
            speedFactor += diffFromAverage;

            float rawDps = (damage * burstShot) / speedFactor;
            float Dps = rawDps * hitChance;
            //Log.Message(weapon.LabelCap + " dps:" + rawDps + "dam:"+damage*burstShot + " spdfac:" + speedFactor + " spdFacBase:" + speedFactorBase);
            return Dps;
        }

        public static float MeleeSpeed(ThingWithComps weapon, Pawn pawn)
        {
            List<VerbProperties> verbProps;
            List<Tool> tools;
            GetVerbsAndTools(weapon, out verbProps, out tools);
            float speed = (from x in VerbUtility.GetAllVerbProperties(verbProps, tools)
                           where x.verbProps.IsMeleeAttack
                           select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, pawn, weapon, null, false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedCooldown(x.tool, pawn, weapon));
            return speed;
        }

        public static float getMeleeDPSBiased(ThingWithComps weapon, Pawn pawn, float speedBias, float averageSpeed)
        {
            //nicked from StatWorker_MeleeAverageDPS
            List<VerbProperties> verbProps;
            List<Tool> tools;
            GetVerbsAndTools(weapon, out verbProps, out tools);
            float damage = (from x in VerbUtility.GetAllVerbProperties(verbProps, tools)
                         where x.verbProps.IsMeleeAttack
                         select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, pawn, weapon, null, false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeDamageAmount(x.tool, pawn, weapon, null));
            float speed = MeleeSpeed(weapon, pawn);
            float speedBase = speed;

            float diffFromAverage = speed - averageSpeed;
            diffFromAverage *= (speedBias - 1);
            speed += diffFromAverage;

            //Log.Message(weapon.LabelCap + " damage:" + damage + " spdfac:" + speed + " spdFacBase:" + speedBase);
            if (speed == 0f)
            {
                return 0f;
            }
            return damage / speed;
        }

        public static void GetVerbsAndTools(ThingWithComps weapon, out List<VerbProperties> verbs, out List<Tool> tools)
        {
            if (weapon.def.isTechHediff)
            {
                HediffDef hediffDef = FindTechHediffHediff(weapon);
                if (hediffDef == null)
                {
                    verbs = null;
                    tools = null;
                    return;
                }
                HediffCompProperties_VerbGiver hediffCompProperties_VerbGiver = hediffDef.CompProps<HediffCompProperties_VerbGiver>();
                if (hediffCompProperties_VerbGiver == null)
                {
                    verbs = null;
                    tools = null;
                    return;
                }
                verbs = hediffCompProperties_VerbGiver.verbs;
                tools = hediffCompProperties_VerbGiver.tools;
            }
            else
            {
                verbs = weapon.def.Verbs;
                tools = weapon.def.tools;
            }
        }

        public static HediffDef FindTechHediffHediff(ThingWithComps weapon)
        {
            List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                if (allDefsListForReading[i].addsHediff != null && allDefsListForReading[i].IsIngredient(weapon.def))
                {
                    return allDefsListForReading[i].addsHediff;
                }
            }
            return null;
        }
    }
}
