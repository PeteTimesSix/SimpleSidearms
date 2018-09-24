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
        internal static int countForType(Pawn pawn, WeaponSearchType type)
        {
            return GettersFilters.getWeaponsOfType(pawn, type).Count;
        }

        internal static float weightForType(Pawn pawn, WeaponSearchType type)
        {
            float total = 0;
            List<Thing> weapons = GettersFilters.getWeaponsOfType(pawn, type);
            foreach (Thing thing in weapons)
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

        internal static bool canCarrySidearm(ThingDef sidearm, Pawn pawn)
        {
            string whoCares;
            return canCarrySidearm(sidearm, pawn, out whoCares);
        }

        internal static bool canCarrySidearm(ThingDef sidearm, Pawn pawn, out string errString)
        {
            float maxCapacity = MassUtility.Capacity(pawn);
            float freeCapacity = MassUtility.FreeSpace(pawn);
            float sidearmWeight = sidearm.GetStatValueAbstract(StatDefOf.Mass);
            //ThingStuffPair sidearmAsThingStuffPair = new ThingStuffPair(sidearm.def, sidearm.Stuff);

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
                        if(!SimpleSidearms.LimitModeSingle_Selection.Value.InnerList.Contains<string>(sidearm.defName))
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
                        if (sidearmWeight >= (SimpleSidearms.LimitModeAmount_Absolute.Value - weightForType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.RelativeWeight:
                        if (sidearmWeight >= ((SimpleSidearms.LimitModeAmount_Relative.Value * maxCapacity) - weightForType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.Slots:
                        if (SimpleSidearms.LimitModeAmount_Slots.Value <= countForType(pawn, WeaponSearchType.Both))
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
                        if (sidearmWeight >= (SimpleSidearms.LimitModeAmountTotal_Absolute.Value - weightForType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.RelativeWeight:
                        if (sidearmWeight >= ((SimpleSidearms.LimitModeAmountTotal_Relative.Value * maxCapacity) - weightForType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.Slots:
                        if (SimpleSidearms.LimitModeAmountTotal_Slots.Value <= countForType(pawn, WeaponSearchType.Both))
                        {
                            errString = "SidearmPickupFail_AllSlotsFull".Translate();
                            return false;
                        }
                        break;
                }
                if (!sidearm.IsRangedWeapon)
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
                            if (!SimpleSidearms.LimitModeSingleMelee_Selection.Value.InnerList.Contains<string>(sidearm.defName))
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
                            if (sidearmWeight >= (SimpleSidearms.LimitModeAmountMelee_Absolute.Value - weightForType(pawn, WeaponSearchType.Melee)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            if (sidearmWeight >= ((SimpleSidearms.LimitModeAmountMelee_Relative.Value * maxCapacity) - weightForType(pawn, WeaponSearchType.Melee)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            if (SimpleSidearms.LimitModeAmountMelee_Slots.Value <= countForType(pawn, WeaponSearchType.Melee))
                            {
                                errString = "SidearmPickupFail_MeleeSlotsFull".Translate();
                                return false;
                            }
                            break;
                    }
                }
                else
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
                            if (!SimpleSidearms.LimitModeSingleRanged_Selection.Value.InnerList.Contains<string>(sidearm.defName))
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
                            if (sidearmWeight >= (SimpleSidearms.LimitModeAmountRanged_Absolute.Value - weightForType(pawn, WeaponSearchType.Ranged)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            if (sidearmWeight >= ((SimpleSidearms.LimitModeAmountRanged_Relative.Value * maxCapacity) - weightForType(pawn, WeaponSearchType.Ranged)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            if (SimpleSidearms.LimitModeAmountRanged_Slots.Value <= countForType(pawn, WeaponSearchType.Ranged))
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


        /*
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
        }*/

        private static float GetMeleeHitChance(Pawn pawn)
        {
            return pawn.GetStatValue(StatDefOf.MeleeHitChance, true);
        }

        private static List<Verb_MeleeAttack> GetUnarmedVerbs(Pawn pawn)
        {
            //List<Verb_MeleeAttack> meleeAtks = new List<Verb_MeleeAttack>();
            //List<Verb> allVerbs = pawn.verbTracker.AllVerbs;
            List<Verb_MeleeAttack> verbs = new List<Verb_MeleeAttack>();
            foreach (Verb_MeleeAttack verb in pawn.VerbTracker.AllVerbs.Where((Verb v) => (v is Verb_MeleeAttack)).Cast<Verb_MeleeAttack>())
            {
                if (verb.IsStillUsableBy(pawn))
                    verbs.Add(verb);
            }
            return verbs;
            /*return pawn.VerbProperties.Where((VerbProperties x) => (x.IsMeleeAttack & x as Verb_MeleeAttack))
            for (int i = 0; i < allVerbs.Count; i++)
            {
                if (allVerbs[i] is Verb_MeleeAttack && allVerbs[i].IsStillUsableBy(pawn))
                {
                    meleeAtks.Add(allVerbs[i] as Verb_MeleeAttack);
                }
            }
            foreach (Verb current in pawn.health.hediffSet.GetHediffsVerbs())
            {
                if (current is Verb_MeleeAttack && current.IsStillUsableBy(pawn))
                {
                    meleeAtks.Add(current as Verb_MeleeAttack);
                }
            }
            return meleeAtks;*/
        }

        private static float GetMeleeCooldown(Pawn pawn, ThingWithComps weapon)
        {
            if (pawn == null)
            {
                return 1f;
            }
            List<Verb_MeleeAttack> verbsList = GetUnarmedVerbs(pawn);
            if (weapon != null && weapon.TryGetComp<CompEquippable>() != null)
                verbsList.AddRange(weapon.TryGetComp<CompEquippable>().AllVerbs.OfType<Verb_MeleeAttack>().ToList<Verb_MeleeAttack>());

            if (verbsList.Count == 0)
            {
                return 1f;
            }
            float num = 0f;
            for (int i = 0; i < verbsList.Count; i++)
            {
                num += verbsList[i].verbProps.AdjustedMeleeSelectionWeight(verbsList[i], pawn);
            }
            float num2 = 0f;
            for (int j = 0; j < verbsList.Count; j++)
            {
                float selectionWeight = verbsList[j].verbProps.AdjustedMeleeSelectionWeight(verbsList[j], pawn);
                num2 += selectionWeight / num * (float)verbsList[j].verbProps.AdjustedCooldown(verbsList[j], pawn);
            }
            return num2;
        }

        private static float GetMeleeDamage(Pawn pawn, ThingWithComps weapon, Pawn target)
        {
            Log.Message("calcing melee damage for "+((weapon == null) ? "unarmed" : weapon.def.defName));

            if (pawn == null)
            {
                Log.Warning("attempted to calc meleeDPS for null pawn!");
                return 0;
            }

            if (weapon == null)
            {
                Log.Message("no weapon, getting unarmed");

                Log.Message("pawn unarmed atk dmg is: " + pawn.GetStatValue(StatDefOf.MeleeDPS));
                return pawn.GetStatValue(StatDefOf.MeleeDPS);
                //calc unarmed damage
                /*if (pawn.equipment != null)
                {
                    pawn.equipment.s
                    float meleeDPSWithWeapon = pawn.GetStatValue(StatDefOf.MeleeDPS);
                    ThingWithComps primary = pawn.equipment.Primary;
                    UnsafeUtilities.SetPropertyValue(pawn.equipment, "Primary", null);
                    float meleeDPSWithoutWeapon = pawn.GetStatValue(StatDefOf.MeleeDPS);
                    UnsafeUtilities.SetPropertyValue(pawn.equipment, "Primary", primary);
                    Log.Warning("DPS with " + primary.def.defName + ": " + meleeDPSWithWeapon + " without: " + meleeDPSWithoutWeapon);
                    return meleeDPSWithoutWeapon;
                }
                else
                {
                    Log.Message("pawn unarmed atk dmg is: "+ pawn.GetStatValue(StatDefOf.MeleeDPS));
                    return pawn.GetStatValue(StatDefOf.MeleeDPS);
                }*/
            }
            else
            {
                Log.Message("weapon dmg is: " + weapon.GetStatValue(StatDefOf.MeleeDPS));
                return weapon.GetStatValue(StatDefOf.MeleeDPS);
            }

            //I dont even know what to say, really
            /*
            List<Verb_MeleeAttack> verbs = GetUnarmedVerbs(pawn);

            if(weapon != null)
            {
                CompEquippable equippable = weapon.TryGetComp<CompEquippable>();
                if (equippable != null)
                {
                    foreach (Tool tool in weapon.def.tools)
                    {
                        foreach (Verb_MeleeAttack verb in weapon.GetComp<CompEquippable>().VerbTracker.AllVerbs.Where((Verb v) => (v is Verb_MeleeAttack)).Cast<Verb_MeleeAttack>())
                        {
                            if (verb.IsStillUsableBy(pawn))
                                verbs.Add(verb);
                        }
                    }
                }
            }

            if (weapon == null)
                Log.Message("melee damage for fists calculated from ");
            else
                Log.Message("melee damage for " + weapon.def.defName+ " calculated from ");

            string list = "";
            foreach (Verb_MeleeAttack verb in verbs)
            {
                list += verb + " " + verb.tool + "";
            }

            if (verbs.Count == 0)
            {
                return 0f;
            }
            float num = 0f;
            foreach (Verb_MeleeAttack verb in verbs)
            {
                num += verb.verbProps.AdjustedMeleeSelectionWeight(verb, pawn);
            }
            float num2 = 0f;
            foreach (Verb_MeleeAttack verb in verbs)
            {
                float selectionWeight = verb.verbProps.AdjustedMeleeSelectionWeight(verb, pawn);
                float damage = (float)verb.verbProps.AdjustedMeleeDamageAmount(verb, pawn);

                //TODO: Note to self: this is where all the armor related stuff should be

                //StatDef deflectionStat = verbsList[j].verbProps.meleeDamageDef.armorCategory.armorRatingStat;  
                //damage *= ReduceForArmorType(deflectionStat, target);

                num2 += selectionWeight / num * damage;
            }
            return num2;*/
        }

        //far as I know, this all now works completely differently again. Yay.

        /*internal static float ReduceForArmorType(StatDef deflectionStat, Pawn target)
        {
            float reduction = 1.0f;
            if (target != null)
            {
                BodyPartGroupDef bodyPartTest = BodyPartGroupDefOf.Torso;   //For statistical purposes, consider torso only

                if (target.apparel != null && target.apparel.WornApparel != null)
                {
                    foreach (Apparel apparel in target.apparel.WornApparel.Where(a => a.def.apparel.bodyPartGroups.Contains(bodyPartTest)))
                    {
                        reduction *= GetArmorTypeFactor(apparel.GetStatValue(deflectionStat, true));
                    }
                }

                reduction *= GetArmorTypeFactor(target.GetStatValue(deflectionStat, true));
            }
            return reduction;
        }

        internal static float GetArmorTypeFactor(float armorRating)
        {
            // Word from Tynan himself, praise be unto he
            // https://www.reddit.com/r/RimWorld/comments/2q542f/alpha_eight_armor_changes/
            
             //One armor value.
             //Up to 50%, damage resistance increases.
             //Past 50%, damage deflection increases.
             //Past 100%, damage deflection and resistance both increase at 1/4 rate up to a maximum of 90% and 90%.
             
            float resistance;
            float deflectChance;

            if (armorRating <= 0.50)
            {
                resistance = armorRating;
                deflectChance = 0;
            }
            else if (armorRating < 1.0f)
            {
                resistance = 0.5f;
                deflectChance = armorRating - 0.5f;
            }
            else
            {
                resistance = 0.5f + (armorRating - 1f) * 0.25f;
                deflectChance = 0.5f + (armorRating - 1f) * 0.25f;
            }

            if (resistance > 0.9f)
                resistance = 0.9f;
            if (deflectChance > 0.9f)
                deflectChance = 0.9f;

            // Game code computes deflect or resist, we're looking for just the statistical average:
            return (1.0f - resistance) * (1.0f - deflectChance);
        }*/

        internal static float MeleeDPS(Pawn pawn, ThingWithComps weapon, float speedBias, Pawn target)
        {
            if (pawn == null)
            {
                Log.Warning("attempted to calc meleeDPS for null pawn!");
                return 0;
            }
            float dps;
            if (weapon == null)
                dps = pawn.GetStatValue(StatDefOf.MeleeDPS);
            else
                dps = weapon.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS);
            //float dps = GetMeleeDamage(pawn, weapon, target) * GetMeleeHitChance(pawn) / GetMeleeCooldown(pawn, weapon);
            return dps;
        }

        internal static float RangedDPSAverage(ThingWithComps weapon, float speedBias)
        {
            if (weapon == null)
                return 0;

            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;

            float damage = (atkProps.defaultProjectile == null) ? 0 : atkProps.defaultProjectile.projectile.damageDef.defaultDamage;
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
            
            if (atkProps.range * atkProps.range < range || atkProps.minRange * atkProps.minRange > range)
                return -1;

            float hitChance = atkProps.GetHitChanceFactor(weapon, range);
            float damage = (atkProps.defaultProjectile == null) ? 0 : atkProps.defaultProjectile.projectile.GetDamageAmount(weapon);
            float warmup = atkProps.warmupTime; 
            float cooldown = weapon.def.GetStatValueAbstract(StatDefOf.RangedWeapon_Cooldown, null);
            int burstShot = atkProps.burstShotCount;
            int ticksBetweenShots = atkProps.ticksBetweenBurstShots;
            float rawDps = (damage * burstShot) / (((warmup + cooldown)) + warmup * (speedBias - 1f) + (burstShot - 1) * (ticksBetweenShots / 60f));
            float Dps = rawDps * hitChance;
            return Dps;
        }

        //implemented in vanilla now

        /*internal static float GetHitChanceFactor(VerbProperties props, Thing equipment, float dist)
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
        }*/

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
