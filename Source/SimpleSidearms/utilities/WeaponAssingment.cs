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
        public static void SetPrimary(Pawn pawn, Thing toSwapTo, bool intentionalEquip, bool fromInventory, bool dropCurrent, bool intentionalDrop)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
            if (pawnMemory == null)
                return;

            if (toSwapTo != null)
            {
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

            if (dropCurrent)
            {
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
                pawnMemory.SetPrimary(toSwapTo.def, intentionalEquip);
            }
            else
            {
                pawnMemory.SetPrimaryEmpty(intentionalEquip);
            }

        }

        internal static void reequipPrimaryIfNeededAndAvailable(Pawn pawn, GoldfishModule pawnMemory)
        {
            if(pawn.equipment.Primary == null)
            {
                if (pawnMemory.primary == GoldfishModule.NoWeaponString)
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

        internal static void doCQC(Pawn pawn, Pawn attacker)
        {
            //Log.Message(attacker.LabelShort + " attacking " + pawn.LabelShort + " CQC check");
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
                    

                    if (CQCTargetOnly.Value == true && attacker != pawn.mindState.enemyTarget)
                    {
                        return;
                    }

                    bool changed = tryCQCWeaponSwapToMelee(pawn);

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
                else
                {
                    return;
                    //Log.Message(__instance.LabelShort + " reports taking damage with no source");
                }
            }
        }

        internal static bool tryCQCWeaponSwapToMelee(Pawn pawn)
        {
            if (pawn.Dead)
                return false;

            if (pawn.equipment == null)
                return false;

            if (pawn.equipment.Primary == null && pawn.IsColonistPlayerControlled && (CQCFistSwitch == false))
            {
                return false;
            }
            if (pawn.equipment.Primary != null)
            {
                if (pawn.equipment.Primary.def.IsMeleeWeapon)
                    return false;
            }
            return tryMeleeWeaponSwap(pawn, MiscUtils.shouldDrop(DroppingModeEnum.Panic), true, pawn.IsColonistPlayerControlled);
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

            //Log.Message("looking for ranged weapon in inventory");
            if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
            {
                //Log.Message("found ranged weapon in inventory");
                best = GettersFilters.findBestRangedWeapon(pawn, skipDangerous/*, RangedSelectionMode*/);
            }

            if (best == null)
                return false;

            //Log.Message("sorted out best (" + best.LabelShort + ")");

            if (best is ThingWithComps)
            {
                //Log.Message("converted to ThingWithComps");
                ThingWithComps bestThing = (ThingWithComps)best;

                SetPrimary(pawn, bestThing, false, true, dropCurrent, false);

                return true;
            }
            return false;
        }

        internal static bool tryMeleeWeaponSwap(Pawn pawn, bool dropCurrent, bool considerUnarmed, bool skipDangerous)
        {
            if (pawn.Dead)
                return false;
            Thing best = null;

            bool unarmedIsBest;

            //Log.Message("looking for melee weapon in inventory");
            if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsMeleeWeapon))
            {
                //Log.Message("found melee weapon in inventory");
                best = GettersFilters.findBestMeleeWeapon(pawn, skipDangerous, out unarmedIsBest/*, MeleeSelectionMode*/);
            }
            else
            {
                unarmedIsBest = true;
            }

            if (best == null | unarmedIsBest)
            {
                if (considerUnarmed)
                {
                    SetPrimary(pawn, null, false, true, dropCurrent, false);
                }
            }
            else
            {
                if (best is ThingWithComps)
                {
                    SetPrimary(pawn, best, false, true, dropCurrent, false);
                    return true;
                }
            }
            
            return false;
        }
        
        internal static bool trySwapToMoreAccurateRangedWeapon(Pawn pawn, bool dropCurrent, float range, bool skipDangerous)
        {
            //Log.Message("attempting swap");
            if (pawn.Dead)
                return false;
            Thing betterWeapon = null;
            float betterDPS;

            //Log.Message("looking for ranged weapon in inventory");
            if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
            {
                //Log.Message("found ranged weapon in inventory");
                betterWeapon = GettersFilters.findBestRangedWeaponAtRanged(pawn, range, skipDangerous, out betterDPS);
            }
            else
            {
                return false;
            }
            if (betterWeapon == null)
                return false;

            float currentDPS = StatCalculator.RangedDPS(pawn.equipment.Primary, SpeedSelectionBiasRanged.Value, range);

            //Log.Message("current DPS is "+currentDPS+ " ("+pawn.equipment.Primary.LabelShort+")");
            //Log.Message("best sidearm DPS is " + betterDPS + " (" + betterWeapon.LabelShort + ")");

            if (betterDPS < currentDPS + ANTI_OSCILLATION_FACTOR)
                return false;

            //Log.Message("sorted out best (" + best.LabelShort + ")");

            if (betterWeapon is ThingWithComps)
            {
                //Log.Message("converted to ThingWithComps");

                SetPrimary(pawn, betterWeapon, false, true, dropCurrent, false);

                return true;
            }
            return false;
        }

        internal static void dropSidearm(Pawn pawn, Thing interactedWeapon)
        {
            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
            if (pawnMemory == null)
                return;
            pawnMemory.ForgetSidearm(interactedWeapon.def);
            Thing whoCares;
            pawn.inventory.innerContainer.TryDrop(interactedWeapon, pawn.Position, pawn.Map, ThingPlaceMode.Near, out whoCares, null);
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
