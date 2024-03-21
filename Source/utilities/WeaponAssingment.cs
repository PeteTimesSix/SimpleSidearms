using HarmonyLib;
using PeteTimesSix.SimpleSidearms.Compat;
using PeteTimesSix.SimpleSidearms.Intercepts;
using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;
using static HarmonyLib.AccessTools;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms.Utilities
{
    public static class WeaponAssingment
    {
        public static bool EquipSpecificWeaponFromInventoryAsOffhand(Pawn pawn, ThingWithComps weapon, bool dropCurrent, bool intentionalDrop)
        {
            if (!pawn.IsValidSidearmsCarrier())
                return false;

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return false;

            if(!(Tacticowl.active && Tacticowl.dualWieldActive()))
            {
                Log.Error("SS: EquipSpecificWeaponFromInventoryAsOffhand called, but Tacticowl is not active!");
                return false;
            }

            if (Tacticowl.getOffHand(pawn, out ThingWithComps currentOffhand))
            {
                UnequipOffhand(pawn, currentOffhand, dropCurrent, intentionalDrop);
            }

            pawn.inventory.innerContainer.Remove(weapon);
            //pawn.equipment.MakeRoomFor(weapon);
            Tacticowl.setOffHand(pawn, weapon, removing: false);

            if (weapon.def.soundInteract != null && Settings.PlaySounds)
            {
                weapon.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
            }

            return true;
        }

        public static void UnequipOffhand(Pawn pawn, ThingWithComps offhand, bool dropCurrent, bool intentionalDrop)
        {
            if (!(Tacticowl.active && Tacticowl.dualWieldActive()))
            {
                Log.Error("SS: UnequipOffhand called, but Tacticowl is not active!");
                return;
            }

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);

            Tacticowl.setOffHand(pawn, offhand, removing: true);
            //drop
            if (dropCurrent)
            {
                if (!intentionalDrop)
                    DoFumbleMote(pawn);
                pawnMemory.InformOfDroppedSidearm(offhand, intentionalDrop);
                //Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = true;
                pawn.equipment.TryDropEquipment(offhand, out ThingWithComps droppedItem, pawn.Position, false);
                //Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = false;
            }
            //or put it in inventory
            else
            {
                bool addedToInventory = pawn.inventory.innerContainer.TryAddOrTransfer(offhand, true);
                if (!addedToInventory)
                    Log.Warning(String.Format("SS: Failed to place offhand equipment {0} into inventory on pawn {1} (colonist: {2}) (dropping: {3}, current drop mode: {4}). Aborting swap. Please report this!",
                       offhand != null ? offhand.LabelCap : "NULL",
                       pawn?.LabelCap,
                       pawn?.IsColonist,
                       dropCurrent,
                       Settings.FumbleMode
                    ));
            }
        }

        public static bool equipSpecificWeaponTypeFromInventory(Pawn pawn, ThingDefStuffDefPair weapon, bool dropCurrent, bool intentionalDrop)
        {
            ThingWithComps match = pawn.inventory.innerContainer
                .Where(t => { return t is ThingWithComps && t.toThingDefStuffDefPair() == weapon; })
                .Cast<ThingWithComps>()
                .Where(t => { return StatCalculator.canUseSidearmInstance(t, pawn, out _); })
                .OrderByDescending(t => t.MarketValue)
                .FirstOrDefault();
            if (match != null)
                return equipSpecificWeaponFromInventory(pawn, match, dropCurrent, intentionalDrop);
            else
                return false;
        }

        public static bool equipSpecificWeaponFromInventory(Pawn pawn, ThingWithComps weapon, bool dropCurrent, bool intentionalDrop)
        {
            return equipSpecificWeapon(pawn, weapon, dropCurrent, intentionalDrop);
        }

        public static ThingWithComps currentlyEquippingWeapon = null;
        public static bool equipSpecificWeapon(Pawn pawn, ThingWithComps weapon, bool dropCurrent, bool intentionalDrop)
        {
            if (!pawn.IsValidSidearmsCarrierRightNow())
                return false;

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return false;

            if (weapon == pawn.equipment.Primary) //attepmpting to equip already-equipped weapon
            {
                Log.Warning("SS: Attepmpted to equip already-equipped weapon!");
                return false;
            }

            currentlyEquippingWeapon = weapon;

            ThingWithComps storedOffhand = null;

            if (Tacticowl.active && Tacticowl.dualWieldActive())
            {
                if (weapon is null || Tacticowl.isTwoHanded(weapon.def)) //cannot keep an offhand weapon with no primary or a two-handed primary
                {
                    if (Tacticowl.getOffHand(pawn, out ThingWithComps currentOffhand))
                    {
                        UnequipOffhand(pawn, currentOffhand, dropCurrent, intentionalDrop);
                    }
                }
                else if(Tacticowl.isOffHand(weapon)) //equipping weapon already wielded as offhand, need to stop offhanding first
                {
                    pawn.equipment.Remove(weapon);
                    Tacticowl.setOffHand(pawn, weapon, removing: true);
                }
                else if(Tacticowl.getOffHand(pawn, out ThingWithComps currentOffHand))
                {
                    storedOffhand = currentOffHand;
                    //need to briefly remove offhander or bad things happen
                    pawn.equipment.Remove(currentOffHand);
                    Tacticowl.setOffHand(pawn, weapon, removing: true);
                }
            }

            if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canUseSidearmInstance(weapon, pawn, out string reason))
            {
                Log.Warning($"SS: Blocked equip of {weapon.Label} at equip-time because of: {reason}");
                return false;
            }

            var currentPrimary = pawn.equipment.Primary;

            if(currentPrimary != null)
            {
                //drop current on the ground
                if (dropCurrent)
                {
                    if (!intentionalDrop)
                    {
                        DoFumbleMote(pawn);
                    }
                    pawnMemory.InformOfDroppedSidearm(weapon, intentionalDrop);
                    //Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = true;
                    pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out ThingWithComps droppedItem, pawn.Position, false);
                    //Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = false;
                }
                //or put it in inventory
                else
                {
                    bool addedToInventory = pawn.inventory.innerContainer.TryAddOrTransfer(currentPrimary, true);
                    if (!addedToInventory)
                    {
                        Log.Warning(String.Format("SS: Failed to place primary equipment {0} (initially was {1}) into inventory when swapping to {2} on pawn {3} (colonist: {4}) (dropping: {5}, current drop mode: {6}). Aborting swap. Please report this!",
                           pawn.equipment.Primary != null ? pawn.equipment.Primary.LabelCap : "NULL",
                           currentPrimary != null ? currentPrimary.LabelCap : "NULL",
                           weapon != null ? weapon.LabelCap : "NULL",
                           pawn?.LabelCap,
                           pawn?.IsColonist,
                           dropCurrent,
                           Settings.FumbleMode
                        ));
                    }
                }
            }

            if (weapon == null)
            {
            }
            else
            {
                if (weapon.stackCount > 1)
                    weapon = weapon.SplitOff(1) as ThingWithComps; //if this cast doesnt work the world has gone mad

                if (weapon.holdingOwner != null)
                    weapon.holdingOwner.Remove(weapon);
                //Pawn_EquipmentTracker_AddEquipment.addEquipmentSourcedBySimpleSidearms = true;
                pawn.equipment.AddEquipment(weapon as ThingWithComps);
                //Pawn_EquipmentTracker_AddEquipment.addEquipmentSourcedBySimpleSidearms = false;

                if (weapon.def.soundInteract != null && Settings.PlaySounds)
                {
                    weapon.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                }
            }

            //put offhand back if it got briefly vanished
            if(storedOffhand != null)
            {
                Tacticowl.setOffHand(pawn, storedOffhand, removing: false);
            }

            //avoid hunting stackoverflowexception
            if (pawn.CurJobDef == JobDefOf.Hunt)
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);

            currentlyEquippingWeapon = null;

            return true;
        }

        public static void DoFumbleMote(Pawn pawn)
        {
            if (!Prefs.DevMode)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, Prefs.DevMode ? "Fumbled".Translate() : "Fumbled".Translate());
            }
            else
            {
                var bestSkill = Math.Max(pawn.skills.GetSkill(SkillDefOf.Shooting).Level, pawn.skills.GetSkill(SkillDefOf.Melee).Level);
                var chance = Settings.FumbleRecoveryChance.Evaluate(bestSkill);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, Prefs.DevMode ? "Fumbled_dev".Translate($"{((1f - chance) * 100).ToString("F0")}% chance") : "Fumbled".Translate());
            }
        }

        public static bool equipBestWeaponFromInventoryByStatModifiers(Pawn pawn, List<StatDef> stats)
        {
            //Log.Message("looking for a stat booster for stats " + String.Join(",", stats.Select(s => s.label))); ;

            if (!pawn.IsValidSidearmsCarrierRightNow() || stats.Count == 0 || pawn.Drafted)
                return false;

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return false;

            var candidates = pawn.getCarriedWeapons(includeTools: true).Where(t =>
            {
                _ = t.toThingDefStuffDefPair().getBestStatBoost(stats, out bool found); return found;
            });

            if (VFECore.active && VFECore.offHandShield(pawn) != null)
            {
                candidates = candidates.Where(t => VFECore.usableWithShields(t.def));
            }
            if (Tacticowl.active && Tacticowl.dualWieldActive() && Tacticowl.getOffHand(pawn, out _)) //currently has offhanded weapon, filter to only one-handed
            {
                candidates = candidates.Where(t => Tacticowl.canBeOffHand(t.def));
            }

            ThingWithComps bestBooster = candidates.OrderByDescending(t => t.toThingDefStuffDefPair().getBestStatBoost(stats, out _)).FirstOrDefault();

            if (bestBooster == default(ThingWithComps))
                return false;

            if (bestBooster == pawn.equipment.Primary)
                return true;

            bool success = equipSpecificWeaponFromInventory(pawn, bestBooster, false, false);
            return success;
        }

        public static void equipBestWeaponFromInventoryByPreference(Pawn pawn, DroppingModeEnum dropMode, PrimaryWeaponMode? modeOverride = null, Pawn target = null)
        {
            if (!pawn.IsValidSidearmsCarrierRightNow())
                return;
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return;

            PrimaryWeaponMode mode = modeOverride == null ? pawnMemory.primaryWeaponMode : modeOverride.Value;

            if ((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0)
            {
                if (pawn.equipment.Primary != null)
                {
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
            }

            if (pawn.Drafted && 
                (pawnMemory.ForcedUnarmedWhileDrafted || pawnMemory.ForcedUnarmed && pawnMemory.ForcedWeaponWhileDrafted == null))
            {
                if (pawn.equipment.Primary != null)
                {
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
                    
            }
            if (pawn.Drafted && pawnMemory.ForcedWeaponWhileDrafted != null)
            {
                if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.ForcedWeaponWhileDrafted.Value)
                {
                    var requiredWeapon = pawnMemory.ForcedWeaponWhileDrafted.Value;
                    /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                    {
                        //clear invalid
                        pawnMemory.ForcedWeaponWhileDrafted = null;
                        return;
                    }*/
                    bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
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
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            if (pawnMemory.ForcedWeapon != null)
            {
                if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.ForcedWeapon.Value)
                {
                    var requiredWeapon = pawnMemory.ForcedWeapon.Value;
                    /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                    {
                        //clear invalid
                        pawnMemory.ForcedWeapon = null;
                        return;
                    }*/
                    bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            
            if (mode == PrimaryWeaponMode.Ranged ||
                ((mode == PrimaryWeaponMode.BySkill) && (pawn.getSkillWeaponPreference() == PrimaryWeaponMode.Ranged)))
            {

                if (pawnMemory.DefaultRangedWeapon != null && pawn.hasWeaponType(pawnMemory.DefaultRangedWeapon.Value))
                {
                    if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.DefaultRangedWeapon.Value)
                    {
                        var requiredWeapon = pawnMemory.DefaultRangedWeapon.Value;
                        /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                        {
                            //clear invalid
                            pawnMemory.DefaultRangedWeapon = null;
                            return;
                        }*/
                        bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                        if (success)
                            return;
                    }
                    else
                        return;
                }

                else
                {
                    bool skipManualUse = true;
                    bool skipDangerous = pawn.IsColonistPlayerControlled && Settings.SkipDangerousWeapons;
                    bool skipEMP = true;
                    (ThingWithComps weapon, float dps, float averageSpeed) bestWeapon = GettersFilters.findBestRangedWeapon(pawn, null, skipManualUse, skipDangerous, skipEMP);
                    if (bestWeapon.weapon != null)
                    {
                        if (pawn.equipment.Primary != bestWeapon.weapon)
                        {
                            bool success = equipSpecificWeaponFromInventory(pawn, bestWeapon.weapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
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
                        bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                        if (success)
                            return;
                    }
                    else
                        return;

                }
                else 
                {
                    if (pawnMemory.PreferredMeleeWeapon != null && pawn.hasWeaponType(pawnMemory.PreferredMeleeWeapon.Value))
                    {
                        if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.PreferredMeleeWeapon.Value)
                        {
                            var requiredWeapon = pawnMemory.PreferredMeleeWeapon.Value;
                            /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                            {
                                //clear invalid
                                pawnMemory.PreferredMeleeWeapon = null;
                                return;
                            }*/
                            bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
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
                                bool success = equipSpecificWeaponFromInventory(pawn, result, MiscUtils.shouldDrop(pawn, dropMode, false), false);
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
            if (Settings.CQCAutoSwitch == true)
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
                    

                    if (Settings.CQCTargetOnly == true && attacker != pawn.mindState.lastAttackedTarget.Thing)
                    {
                        return;
                    }

                    if (!Settings.OptimalMelee && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsMeleeWeapon)
                        return;

                    bool changed = tryCQCWeaponSwapToMelee(pawn, attacker, DroppingModeEnum.InDistress);

                    //change targets if shooting something else, or has no set target (or nothing)
                    if (changed && (attacker != pawn.mindState.enemyTarget || pawn.mindState.enemyTarget == null))
                    {
                        if (pawn.CurJobDef == JobDefOf.AttackStatic)
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
            if (!Settings.OptimalMelee || target == null || (target.MentalStateDef == MentalStateDefOf.SocialFighting && pawn.MentalStateDef == MentalStateDefOf.SocialFighting))
                    return;

            tryCQCWeaponSwapToMelee(pawn, target, DroppingModeEnum.Combat);
        }

        public static bool tryCQCWeaponSwapToMelee(Pawn pawn, Pawn target, DroppingModeEnum dropMode)
        {
            if (!pawn.IsValidSidearmsCarrierRightNow())
                return false;

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);

            if (pawnMemory == null)
                return false;

            if (pawn.equipment.Primary != null)
            {
                if (pawn.equipment.Primary.def.destroyOnDrop)
                    return false;
            }

            if (pawnMemory.IsCurrentWeaponForced(false))
                return false;

            var current = pawn.equipment.Primary;
            equipBestWeaponFromInventoryByPreference(pawn, dropMode, PrimaryWeaponMode.Melee, target: target);
            return (current != pawn.equipment.Primary);
        }


        public static bool trySwapToMoreAccurateRangedWeapon(Pawn pawn, LocalTargetInfo target, bool dropCurrent, bool skipManualUse, bool skipDangerous = true, bool skipEMP = true)
        {
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);

            if (pawn == null || pawn.Dead || pawnMemory == null || pawn.equipment == null || pawn.inventory == null)
                return false;

            if (pawnMemory.IsCurrentWeaponForced(false))
                return false;

            (ThingWithComps weapon, float dps, float averageSpeed) bestWeapon = GettersFilters.findBestRangedWeapon(pawn, target, skipManualUse, skipDangerous, skipEMP, true);

            if (bestWeapon.weapon == null)
                return false;


            var targetDistance = target.Cell.DistanceTo(pawn.Position);
            float currentDPS = StatCalculator.RangedDPS(pawn.equipment.Primary, Settings.SpeedSelectionBiasRanged, bestWeapon.averageSpeed, targetDistance);
            
            if (bestWeapon.dps < currentDPS + MiscUtils.ANTI_OSCILLATION_FACTOR)
                return false;

            equipSpecificWeaponFromInventory(pawn, bestWeapon.weapon, dropCurrent, false);
            return true;
        }

        [Obsolete("use DropSidearm(Pawn pawn, ThingWithComps weapon, bool intentionalDrop, bool unmemorise) instead")]
        public static void dropSidearm(Pawn pawn, Thing weapon, bool intentionalDrop)
        {
            if (!(weapon is ThingWithComps weaponTyped))
                return;
            DropSidearm(pawn, weaponTyped, intentionalDrop, false);
        }

        public static void DropSidearm(Pawn pawn, ThingWithComps weapon, bool intentionalDrop, bool unmemorise)
        {
            if (weapon == null)
                return;
            if (pawn.IsQuestLodger() && intentionalDrop)
                return;

            if (!intentionalDrop)
                DoFumbleMote(pawn);

            if (pawn.equipment.Primary == weapon || (Tacticowl.active && Tacticowl.dualWieldActive() && Tacticowl.isOffHand(weapon)))
            {
                //Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = true;
                pawn.equipment.TryDropEquipment(weapon, out _, pawn.Position, false);
                //Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = false;
            }
            else
            {
                if (weapon.stackCount > 1)
                {
                    var toDrop = weapon.SplitOff(1);
                    GenDrop.TryDropSpawn(toDrop, pawn.Position, pawn.Map, ThingPlaceMode.Near, out _);
                }
                else
                {
                    pawn.inventory.innerContainer.TryDrop(weapon, pawn.Position, pawn.Map, ThingPlaceMode.Near, out _, null);
                }
            }

            if (unmemorise)
            {
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (pawnMemory == null)
                    return;
                pawnMemory.InformOfDroppedSidearm(weapon, intentionalDrop);
            }
        }
    }
}
