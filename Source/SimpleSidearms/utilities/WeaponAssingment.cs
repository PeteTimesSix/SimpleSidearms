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
        public static bool SetPrimary(Pawn pawn, Thing toSwapTo, bool intentionalEquip, bool fromInventory, bool dropCurrent, bool intentionalDrop)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);

            if (toSwapTo != null)
            {
                if (pawn.equipment != null && pawn.equipment.Primary != null &&
                    toSwapTo.thingIDNumber == pawn.equipment.Primary.thingIDNumber)
                {
                    return false;
                }

                if (toSwapTo.stackCount > 1)
                {
                    toSwapTo = toSwapTo.SplitOff(1);
                }

                if (fromInventory)
                {
                    if (pawn.inventory.Contains(toSwapTo))
                        pawn.inventory.innerContainer.Remove(toSwapTo);
                }
            }
            else if (pawn.equipment != null && pawn.equipment.Primary == null)
                return false;

            if (dropCurrent)
            {
                if(pawnMemory != null)
                    pawnMemory.DropPrimary(intentionalDrop);

                if(pawn.equipment.Primary != null)
                {
                    ThingWithComps whocares;
                    pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out whocares, pawn.Position, false);
                }
            }
            else
            {
                if (pawn.equipment.Primary != null)
                {
                    ThingWithComps oldPrimary = pawn.equipment.Primary;
                    pawn.equipment.Remove(pawn.equipment.Primary);
                    pawn.inventory.innerContainer.TryAdd(oldPrimary, true);
                }
            }

            if(toSwapTo != null)
            {
                Pawn_EquipmentTracker_AddEquipment_Postfix.sourcedBySimpleSidearms = true;
                pawn.equipment.AddEquipment(toSwapTo as ThingWithComps);
                Pawn_EquipmentTracker_AddEquipment_Postfix.sourcedBySimpleSidearms = false;

                if (toSwapTo.def.soundInteract != null)
                {
                    toSwapTo.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                }
                if (pawnMemory != null)
                    pawnMemory.SetPrimary(toSwapTo.def, intentionalEquip);
            }
            else
            {
                if (pawnMemory != null)
                    pawnMemory.SetPrimaryEmpty(intentionalEquip);
            }
            
            //avoid hunting stackoverflowexception
            if (pawn.jobs != null && pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.Hunt)
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);

            return true;    
        }

        internal static void reequipPrimaryIfNeededAndAvailable(Pawn pawn, GoldfishModule pawnMemory)
        {
            if (pawn == null || pawnMemory == null)
                return;
            if(pawn.equipment.Primary == null)
            {
                if (pawnMemory.NoPrimary)
                    return;
                else
                {
                    foreach(Thing thing in pawn.inventory.innerContainer)
                    {
                        if (thing.def.defName.Equals(pawnMemory.primary))
                        {
                            SetPrimary(pawn, thing, true, true, false, false);
                            return;
                        }
                    }
                }
            }
            else
            {
                if (pawnMemory.primary.Equals(pawn.equipment.Primary.def.defName))
                    return;
                else
                {
                    foreach (Thing thing in pawn.inventory.innerContainer)
                    {
                        if (thing.def.defName.Equals(pawnMemory.primary))
                        {
                            SetPrimary(pawn, thing, true, true, false, false);
                            return;
                        }
                    }
                }
            }
        }

        //When hit in Close-Quarter Combat 
        internal static void doCQC(Pawn pawn, Pawn attacker)
        {
            if(pawn.jobs != null)
            {
                if(pawn.jobs.curDriver is JobDriver_EquipSidearmCombat)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }

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

                    bool changed = tryCQCWeaponSwapToMelee(pawn, attacker);

                    //change targets if shooting something else, or has no set target (or nothing)
                    if (changed && (attacker != pawn.mindState.enemyTarget || pawn.mindState.enemyTarget == null))
                    {
                        if (pawn.jobs.curJob.def == JobDefOf.AttackStatic)
                        {
                            Job atkJob = new Job(JobDefOf.AttackMelee, attacker)
                            {
                                maxNumMeleeAttacks = 1,
                                expiryInterval = 200
                            };
                            pawn.jobs.TryTakeOrderedJob(atkJob);
                        }
                    }
                }
            }
        }
        
        internal static void chooseOptimalMeleeForAttack(Pawn pawn, Pawn target)
        {
            if (!OptimalMelee || target == null || (target.MentalStateDef == MentalStateDefOf.SocialFighting && pawn.MentalStateDef == MentalStateDefOf.SocialFighting))
                    return;

            tryCQCWeaponSwapToMelee(pawn, target);
        }

        internal static bool tryCQCWeaponSwapToMelee(Pawn pawn, Pawn target)
        {
            if (pawn.Dead)
                return false;

            if (!pawn.RaceProps.Humanlike)
                return false;

            if (pawn.equipment == null)
                return false;

            if (pawn.equipment.Primary == null && pawn.IsColonistPlayerControlled && (CQCFistSwitch == false))
            {
                return false;
            }
            if (pawn.equipment.Primary != null)
            {
                if (pawn.equipment.Primary.def.destroyOnDrop)
                    return false;
            }
            return tryMeleeWeaponSwap(pawn, MiscUtils.shouldDrop(DroppingModeEnum.Panic), true, pawn.IsColonistPlayerControlled, target);
        }

        internal static void weaponSwapSpecific(Pawn pawn, Thing toSwapTo, bool intentionalEquip, bool dropCurrent, bool intentionalDrop)
        {
            if (pawn.Dead)
                return;
           
            if (toSwapTo != null && toSwapTo as ThingWithComps == null)
            {
                Log.Warning("Warning: Could not convert " + toSwapTo.LabelShort + " to ThingWithComps, aborting swap");
                return;
            }

            SetPrimary(pawn, toSwapTo, intentionalEquip, true, dropCurrent, intentionalDrop);
           
        }

        internal static bool tryRangedWeaponSwap(Pawn pawn, bool dropCurrent, bool skipDangerous)
        {
            if (pawn.Dead)
                return false;
            Thing best = null;
            
            if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
            {
                best = GettersFilters.findBestRangedWeapon(pawn, skipDangerous/*, RangedSelectionMode*/);
            }

            if (best == null)
                return false;

            if (best is ThingWithComps)
            {
                ThingWithComps bestThing = (ThingWithComps)best;

                SetPrimary(pawn, bestThing, false, true, dropCurrent, false);

                return true;
            }
            return false;
        }

        internal static bool tryMeleeWeaponSwap(Pawn pawn, bool dropCurrent, bool considerUnarmed, bool skipDangerous, Pawn target = null)
        {
            if (pawn.Dead)
                return false;

            ThingWithComps current = null;
            if (pawn.equipment != null & pawn.equipment.Primary != null)
                current = pawn.equipment.Primary;

            ThingWithComps best = GettersFilters.findBestMeleeWeapon(pawn, skipDangerous/*, MeleeSelectionMode*/, target);

            if (current != best && (best != null || considerUnarmed))
                return SetPrimary(pawn, best, false, true, dropCurrent, false);
            
            return false;
        }
        
        internal static bool trySwapToMoreAccurateRangedWeapon(Pawn pawn, bool dropCurrent, float range, bool skipDangerous)
        {
            if (pawn.Dead)
                return false;
            Thing betterWeapon = null;
            float betterDPS;
            
            if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
            {
                betterWeapon = GettersFilters.findBestRangedWeaponAtRanged(pawn, range, skipDangerous, out betterDPS);
            }
            else
            {
                return false;
            }
            if (betterWeapon == null)
                return false;

            float currentDPS = StatCalculator.RangedDPS(pawn.equipment.Primary, SpeedSelectionBiasRanged.Value, range);
            
            if (betterDPS < currentDPS + ANTI_OSCILLATION_FACTOR)
                return false;

            if (betterWeapon is ThingWithComps)
            {
                SetPrimary(pawn, betterWeapon, false, true, dropCurrent, false);

                return true;
            }
            return false;
        }

        internal static void dropSidearm(Pawn pawn, Thing interactedWeapon)
        {
            Thing whoCares;
            pawn.inventory.innerContainer.TryDrop(interactedWeapon, pawn.Position, pawn.Map, ThingPlaceMode.Near, out whoCares, null);

            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
            if (pawnMemory == null)
                return;
            pawnMemory.ForgetSidearm(interactedWeapon.def);
        }

        internal static void forgetSidearmMemory(Pawn pawn, ThingDef interactedWeaponMemory)
        { 
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
            if (pawnMemory == null)
                return;
            pawnMemory.ForgetSidearm(interactedWeaponMemory);
        }
    }
}
