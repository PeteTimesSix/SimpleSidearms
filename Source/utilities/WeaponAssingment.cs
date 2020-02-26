using RimWorld;
using SimpleSidearms.intercepts;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.Sound;
using static SimpleSidearms.Globals;
using static SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.utilities
{
    public static class WeaponAssingment
    {

        public static bool equipSpecificWeaponTypeFromInventory(Pawn pawn, ThingStuffPair weapon, bool dropCurrent, bool intentionalDrop)
        {
            ThingWithComps match = pawn.inventory.innerContainer.Where(t => { return t is ThingWithComps && t.toThingStuffPair() == weapon; }).OrderByDescending(t => t.MarketValue).FirstOrDefault() as ThingWithComps;
            if (match != null)
                return equipSpecificWeaponFromInventory(pawn, match, dropCurrent, intentionalDrop);
            else
                return false;
        }

        public static bool equipSpecificWeaponFromInventory(Pawn pawn, ThingWithComps weapon, bool dropCurrent, bool intentionalDrop)
        {
            if (weapon != null)
            {
                if (weapon.stackCount > 1)
                    weapon = weapon.SplitOff(1) as ThingWithComps; //if this cast doesnt work the world has gone mad
                else
                    pawn.inventory.innerContainer.Remove(weapon);
            }
            return equipSpecificWeapon(pawn, weapon, dropCurrent, intentionalDrop);
        }

        public static bool equipSpecificWeapon(Pawn pawn, ThingWithComps weapon, bool dropCurrent, bool intentionalDrop)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);

            if (pawn == null || pawn.Dead || pawnMemory == null || pawn.equipment == null || pawn.inventory == null)
                return false;

            if (weapon == pawn.equipment.Primary) //attepmpting to equip already-equipped weapon
            {
                Log.Warning("attepmpting to equip already-equipped weapon");
                return false;
            }

                //drop current on the ground
            if (dropCurrent && pawn.equipment.Primary != null)
            {
                pawnMemory.InformOfDroppedSidearm(weapon, intentionalDrop);
                ThingWithComps discarded;
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out discarded, pawn.Position, false);
            }   
                //or put it in inventory
            else if (pawn.equipment.Primary != null)
            {
                ThingWithComps oldPrimary = pawn.equipment.Primary;
                pawn.equipment.Remove(oldPrimary);
                pawn.inventory.innerContainer.TryAdd(oldPrimary, true);
            }

            if (weapon == null)
            {
               
            }
            else
            {
                if (weapon.stackCount > 1)
                    weapon = weapon.SplitOff(1) as ThingWithComps; //if this cast doesnt work the world has gone mad

                Pawn_EquipmentTracker_AddEquipment_Postfix.sourcedBySimpleSidearms = true;
                pawn.equipment.AddEquipment(weapon as ThingWithComps);
                Pawn_EquipmentTracker_AddEquipment_Postfix.sourcedBySimpleSidearms = false;

                if (weapon.def.soundInteract != null)
                {
                    weapon.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                }
            }

            //avoid hunting stackoverflowexception
            if (pawn.jobs != null && pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.Hunt)
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);

            return true;
        }


        public static void equipBestWeaponFromInventoryByPreference(Pawn pawn, DroppingModeEnum drop, GoldfishModule.PrimaryWeaponMode? modeOverride = null, Pawn target = null)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);

            if (pawn == null || pawn.Dead || pawnMemory == null || pawn.equipment == null || pawn.inventory == null)
                return;

            GoldfishModule.PrimaryWeaponMode mode = modeOverride == null ? pawnMemory.primaryWeaponMode : modeOverride.Value;

            if (pawn.Drafted && 
                (pawnMemory.ForcedUnarmedWhileDrafted || pawnMemory.ForcedUnarmed && pawnMemory.ForcedWeaponWhileDrafted == null))
            {
                if (pawn.equipment.Primary != null)
                {
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(drop), false);
                    if (success)
                        return;
                }
                else
                    return;
                    
            }
            if (pawn.Drafted && pawnMemory.ForcedWeaponWhileDrafted != null)
            {
                if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingStuffPair() != pawnMemory.ForcedWeaponWhileDrafted.Value)
                {
                    var requiredWeapon = pawnMemory.ForcedWeaponWhileDrafted.Value;
                    bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(drop), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            if (pawnMemory.ForcedUnarmed)
            {
                if (pawn.equipment.Primary != null)
                {
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(drop), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            if (pawnMemory.ForcedWeapon != null)
            {
                if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingStuffPair() != pawnMemory.ForcedWeapon.Value)
                {
                    var requiredWeapon = pawnMemory.ForcedWeapon.Value;
                    bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(drop), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            
            if (mode == GoldfishModule.PrimaryWeaponMode.Ranged ||
                ((mode == GoldfishModule.PrimaryWeaponMode.BySkill) && (pawn.getSkillWeaponPreference() == GoldfishModule.PrimaryWeaponMode.Ranged)))
            {

                if (pawnMemory.DefaultRangedWeapon != null && pawn.hasWeaponSomewhere(pawnMemory.DefaultRangedWeapon.Value))
                {
                    if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingStuffPair() != pawnMemory.DefaultRangedWeapon.Value)
                    {
                        var requiredWeapon = pawnMemory.DefaultRangedWeapon.Value;
                        bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(drop), false);
                        if (success)
                            return;
                    }
                    else
                        return;
                }

                else
                {
                    ThingWithComps result;
                    (ThingWithComps weapon, float dps, float averageSpeed) bestWeapon = GettersFilters.findBestRangedWeapon(pawn, null, pawn.IsColonistPlayerControlled);
                    if (bestWeapon.weapon != null)
                    {
                        if (pawn.equipment.Primary != bestWeapon.weapon)
                        {
                            bool success = equipSpecificWeaponFromInventory(pawn, bestWeapon.weapon, MiscUtils.shouldDrop(drop), false);
                            if (success)
                                return;
                        }
                        else
                            return;
                    }
                }
            }

            //all that's left is either melee preference or no ranged weapon found - so in either case, we want to equip a melee weapon.

            /*if (mode == GoldfishModule.PrimaryWeaponMode.Melee ||
                ((mode == GoldfishModule.PrimaryWeaponMode.BySkill) && (pawn.getSkillWeaponPreference() == GoldfishModule.PrimaryWeaponMode.Melee)))*/
            {
                //Log.Message("melee mode");
                //prefers melee
                if (pawnMemory.PreferredUnarmed)
                {
                    if (pawn.equipment.Primary != null)
                    {
                        bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(drop), false);
                        if (success)
                            return;
                    }
                    else
                        return;

                }
                else 
                {
                    if (pawnMemory.PreferredMeleeWeapon != null && pawn.hasWeaponSomewhere(pawnMemory.PreferredMeleeWeapon.Value))
                    {
                        if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingStuffPair() != pawnMemory.PreferredMeleeWeapon.Value)
                        {
                            var requiredWeapon = pawnMemory.PreferredMeleeWeapon.Value;
                            bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(drop), false);
                            if (success)
                                return;
                        }
                        else
                            return;
                    }
                    else
                    {
                        ThingWithComps result;
                        bool foundAlternative = GettersFilters.findBestMeleeWeapon(pawn, out result, includeRangedWithBash: false);
                        if (foundAlternative)
                        {
                            if (pawn.equipment.Primary != result)
                            {
                                bool success = equipSpecificWeaponFromInventory(pawn, result, MiscUtils.shouldDrop(drop), false);
                                if (success)
                                    return;
                            }
                            else
                                return;
                        }
                    }
                }
            }
            return;
        }

        //When hit in Close-Quarter Combat 
        internal static void doCQC(Pawn pawn, Pawn attacker)
        {

            if (CQCAutoSwitch == true)
            {
                if (attacker != null)
                {
                    if (attacker.MentalStateDef == MentalStateDefOf.SocialFighting && pawn.MentalStateDef == MentalStateDefOf.SocialFighting)
                        return;

                    if(attacker.equipment != null)
                    {
                        if (attacker.equipment.Primary != null)
                        {
                            if (attacker.equipment.Primary.def.IsRangedWeapon)
                                return;
                        }
                    }
                    

                    if (CQCTargetOnly.Value == true && attacker != pawn.mindState.lastAttackedTarget.Thing)
                    {
                        return;
                    }

                    if (!OptimalMelee && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsMeleeWeapon)
                        return;

                    bool changed = tryCQCWeaponSwapToMelee(pawn, attacker, DroppingModeEnum.InDistress);

                    //change targets if shooting something else, or has no set target (or nothing)
                    if (changed && (attacker != pawn.mindState.enemyTarget || pawn.mindState.enemyTarget == null))
                    {
                        if (pawn.jobs.curJob.def == JobDefOf.AttackStatic)
                        {
                            Job atkJob = JobMaker.MakeJob(JobDefOf.AttackMelee, attacker);
                            atkJob.maxNumMeleeAttacks = 1;
                            atkJob.expiryInterval = 200;
                            pawn.jobs.TryTakeOrderedJob(atkJob);
                        }
                    }
                }
            }
        }

        public static void chooseOptimalMeleeForAttack(Pawn pawn, Pawn target)
        {
            if (!OptimalMelee || target == null || (target.MentalStateDef == MentalStateDefOf.SocialFighting && pawn.MentalStateDef == MentalStateDefOf.SocialFighting))
                    return;

            tryCQCWeaponSwapToMelee(pawn, target, DroppingModeEnum.Combat);
        }

        public static bool tryCQCWeaponSwapToMelee(Pawn pawn, Pawn target, DroppingModeEnum drop)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);

            if (pawn == null || pawn.Dead || pawnMemory == null || pawn.equipment == null || pawn.inventory == null)
                return false;

            if (!pawn.RaceProps.Humanlike)
                return false;

            if (pawn.equipment.Primary != null)
            {
                if (pawn.equipment.Primary.def.destroyOnDrop)
                    return false;
            }

            if (pawnMemory.IsCurrentWeaponForced(false))
                return false;

            var current = pawn.equipment.Primary;
            equipBestWeaponFromInventoryByPreference(pawn, drop, GoldfishModule.PrimaryWeaponMode.Melee, target: target);
            return (current != pawn.equipment.Primary);
        }


        public static bool trySwapToMoreAccurateRangedWeapon(Pawn pawn, LocalTargetInfo target, bool dropCurrent, bool skipDangerous = true)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);

            if (pawn == null || pawn.Dead || pawnMemory == null || pawn.equipment == null || pawn.inventory == null)
                return false;

            if (pawnMemory.IsCurrentWeaponForced(false))
                return false;

            (ThingWithComps weapon, float dps, float averageSpeed) bestWeapon = GettersFilters.findBestRangedWeapon(pawn, target, skipDangerous, true);

            if (bestWeapon.weapon == null)
                return false;

            CellRect cellRect = (!target.HasThing) ? CellRect.SingleCell(target.Cell) : target.Thing.OccupiedRect();
            float range = cellRect.ClosestDistSquaredTo(pawn.Position);
            float currentDPS = StatCalculator.RangedDPS(pawn.equipment.Primary, SpeedSelectionBiasRanged.Value, bestWeapon.averageSpeed, range);
            
            if (bestWeapon.dps < currentDPS + ANTI_OSCILLATION_FACTOR)
                return false;

            equipSpecificWeaponFromInventory(pawn, bestWeapon.weapon, dropCurrent, false);
            return true;
        }

        public static void dropSidearm(Pawn pawn, Thing weapon, bool intentional)
        {
            if (weapon == null)
                return;
            if (pawn.IsQuestLodger() && intentional)
                return;

            ThingWithComps discarded1;
            Thing discarded2;
            if (pawn.equipment.Primary == weapon)
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out discarded1, pawn.Position, false);
            else
                pawn.inventory.innerContainer.TryDrop(weapon, pawn.Position, pawn.Map, ThingPlaceMode.Near, out discarded2, null);

            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
            if (pawnMemory == null)
                return;
            pawnMemory.InformOfDroppedSidearm(weapon, intentional);
        }
    }
}
