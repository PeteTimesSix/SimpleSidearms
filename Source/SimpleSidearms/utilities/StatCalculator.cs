using RimWorld;
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

        internal static float weightForType(Pawn pawn, WeaponSearchType type)
        {
            float total = 0;
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                float weight = thing.def.BaseMass;
                if (thing.def.IsRangedWeapon)
                {
                    if (type == WeaponSearchType.Both || type == WeaponSearchType.Ranged)
                        total += weight;
                }
                else if (thing.def.IsMeleeWeapon)
                {
                    if (type == WeaponSearchType.Both || type == WeaponSearchType.Melee)
                        total += weight;
                }
            }
            return total;
        }

        internal static bool fitsInCarryCapacity(ThingWithComps equipment, Pawn pawn, out string errString)
        {
            float weight = equipment.def.BaseMass;
            float maxCarry = MassUtility.Capacity(pawn);
            float remainingCarry = MassUtility.FreeSpace(pawn);



            {
                if (equipment.def.IsRangedWeapon)
                {
                    if (maxCarry * MaxTotalRangedMass.Value < weight)
                    {
                        errString = "Too heavy for ranged sidearm";
                        return false;
                    }
                    else if (maxCarry * MaxTotalRangedMass.Value < weight + weightForType(pawn, WeaponSearchType.Ranged))
                    {
                        errString = "Ranged sidearms too heavy";
                        return false;
                    }
                }
                else
                {
                    if (maxCarry * MaxSingleMeleeMass.Value < weight)
                    {
                        errString = "Too heavy for melee sidearm";
                        return false;
                    }
                    else if (maxCarry * MaxTotalMeleeMass.Value < weight + weightForType(pawn, WeaponSearchType.Melee))
                    {
                        errString = "Melee sidearms too heavy";
                        return false;
                    }
                }
            }
            if (maxCarry * MaxTotalMass.Value < weight + weightForType(pawn, WeaponSearchType.Both))
            {
                errString = "Sidearms too heavy";
                return false;
            }
            else if (remainingCarry < weight)
            {
                errString = "Too much weight";
                return false;
            }
            errString = "Nil";
            return true;
        }

        internal static float MeleeDPS(ThingWithComps weapon, float speedBias)
        {
            if (weapon == null)
                return 0;

            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;

            ThingDef wepDef = weapon.def;

            float damage = atkProps.AdjustedMeleeDamageAmount(atkVerb, atkVerb.CasterPawn, weapon);
            float warmup = atkProps.warmupTime;
            float cooldown = atkProps.AdjustedCooldownTicks(weapon);
            float dps = damage / ((warmup * speedBias + cooldown));
            return dps;
        }

        internal static float RangedDPSAverage(ThingWithComps weapon, float speedBias)
        {
            if (weapon == null)
                return 0;

            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;

            float damage = (atkProps.projectileDef == null) ? 0 : atkProps.projectileDef.projectile.damageAmountBase;
            float warmup = atkProps.warmupTime;
            float cooldown = weapon.def.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown, null);
            int burstShot = atkProps.burstShotCount;
            int ticksBetweenShots = atkProps.ticksBetweenBurstShots;
            float rawDps = (damage * burstShot) / (((warmup + cooldown)) + warmup * (speedBias - 1f) + (burstShot - 1) * (ticksBetweenShots / 60f));
            float DpsAvg = 0f;
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Short, weapon);
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Medium, weapon);
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Long, weapon);
            return DpsAvg / 3f;
        }

        internal static float RangedDPS(ThingWithComps weapon, float speedBias, float range)
        {
            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;

            //Log.Message("Range limits: > " + atkProps.minRange * atkProps.minRange + " < " + atkProps.range * atkProps.range + " range is "+range);

            if (atkProps.range * atkProps.range < range || atkProps.minRange * atkProps.minRange > range)
                return -1;

            float damage = (atkProps.projectileDef == null) ? 0 : atkProps.projectileDef.projectile.damageAmountBase;
            float warmup = atkProps.warmupTime;
            float cooldown = weapon.def.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown, null);
            int burstShot = atkProps.burstShotCount;
            int ticksBetweenShots = atkProps.ticksBetweenBurstShots;
            float rawDps = (damage * burstShot) / (((warmup + cooldown)) + warmup * (speedBias - 1f) + (burstShot - 1) * (ticksBetweenShots / 60f));
            float Dps = rawDps * GetHitChanceFactor(atkProps, weapon, range);

            return Dps;
        }

        internal static float GetHitChanceFactor(VerbProperties props, Thing equipment, float dist)
        {
            float num;
            if (dist <= 4f)
            {
                num = AdjustedAccuracy(props, RangeCategory.Touch, equipment);
            }
            else if (dist <= 15f)
            {
                num = Mathf.Lerp(AdjustedAccuracy(props, RangeCategory.Touch, equipment), AdjustedAccuracy(props, RangeCategory.Short, equipment), (dist - 4f) / 11f);
            }
            else if (dist <= 30f)
            {
                num = Mathf.Lerp(AdjustedAccuracy(props, RangeCategory.Short, equipment), AdjustedAccuracy(props, RangeCategory.Medium, equipment), (dist - 15f) / 15f);
            }
            else if (dist <= 50f)
            {
                num = Mathf.Lerp(AdjustedAccuracy(props, RangeCategory.Medium, equipment), AdjustedAccuracy(props, RangeCategory.Long, equipment), (dist - 30f) / 20f);
            }
            else
            {
                num = AdjustedAccuracy(props, RangeCategory.Long, equipment);
            }
            if (num < 0.01f)
            {
                num = 0.01f;
            }
            if (num > 1f)
            {
                num = 1f;
            }
            return num;
        }

        internal static float AdjustedAccuracy(VerbProperties props, RangeCategory cat, Thing equipment)
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
    }
}
