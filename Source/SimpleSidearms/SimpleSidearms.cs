using Harmony;
using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static SimpleSidearms.Globals;

namespace SimpleSidearms
{
    public class SimpleSidearms : HugsLib.ModBase
    {
        public override string ModIdentifier { get { return "SimpleSidearms"; } }

        internal static SettingHandle<bool> CQCAutoSwitch;
        internal static SettingHandle<bool> CQCFistSwitch;
        internal static SettingHandle<bool> CQCTargetOnly;
        internal static SettingHandle<bool> RangedCombatAutoSwitch;
        internal static SettingHandle<float> RangedCombatAutoSwitchMaxWarmup;
        internal static SettingHandle<bool> SingleshotAutoSwitch;
        internal static SettingHandle<float> SpeedSelectionBiasMelee;
        internal static SettingHandle<float> SpeedSelectionBiasRanged;
        internal static SettingHandle<DroppingModeOptionsEnum> DropMode;

        //internal static SettingHandle<SelectionMode> MeleeSelectionMode;
        //internal static SettingHandle<SelectionMode> RangedSelectionMode;

        internal static SettingHandle<float> MaxSingleMeleeMass;
        internal static SettingHandle<float> MaxTotalMeleeMass;
        internal static SettingHandle<float> MaxSingleRangedMass;
        internal static SettingHandle<float> MaxTotalRangedMass;
        internal static SettingHandle<float> MaxTotalMass;

        internal static SettingHandle<float> SidearmSpawnChance;

        public override void DefsLoaded()
        {
            CQCAutoSwitch = Settings.GetHandle<bool>("CQCAutoSwitch", "CQCAutoSwitch_title".Translate(), "CQCAutoSwitch_desc".Translate(), true);
            CQCFistSwitch = Settings.GetHandle<bool>("CQCFistSwitch", "CQCFistSwitch_title".Translate(), "CQCFistSwitch_desc".Translate(), false);
            CQCTargetOnly = Settings.GetHandle<bool>("CQCTargetOnly", "CQCTargetOnly_title".Translate(), "CQCTargetOnly_desc".Translate(), false);
            RangedCombatAutoSwitch = Settings.GetHandle<bool>("RangedCombatAutoSwitch", "RangedCombatAutoSwitch_title".Translate(), "RangedCombatAutoSwitch_desc".Translate(), true);
            RangedCombatAutoSwitchMaxWarmup = Settings.GetHandle<float>("RangedCombatAutoSwitchMaxWarmup", "RangedCombatAutoSwitchMaxWarmup_title".Translate(), "RangedCombatAutoSwitchMaxWarmup_desc".Translate(), 0.5f);
            SingleshotAutoSwitch = Settings.GetHandle<bool>("SingleshotAutoSwitch", "SingleshotAutoSwitch_title".Translate(), "SingleshotAutoSwitch_desc".Translate(), true);
            SpeedSelectionBiasMelee = Settings.GetHandle<float>("SpeedSelectionBiasMelee", "SpeedSelectionBiasMelee_title".Translate(), "SpeedSelectionBiasMelee_desc".Translate(), 1.15f);
            SpeedSelectionBiasRanged = Settings.GetHandle<float>("SpeedSelectionBiasRanged", "SpeedSelectionBiasRanged_title".Translate(), "SpeedSelectionBiasRanged_desc".Translate(), 1.35f);
            DropMode = Settings.GetHandle<DroppingModeOptionsEnum>("CQCDrop", "CQCDrop_title".Translate(), "CQCDrop_desc".Translate(), DroppingModeOptionsEnum.Panic, null, "CQCDrop_option_");

            //MeleeSelectionMode = Settings.GetHandle<SelectionMode>("MeleeSelectionMode", "Melee selection preference", "Which kind of weapon will your pawns use first when holding multiple melee sidearms.", SelectionMode.Strongest);
            //RangedSelectionMode = Settings.GetHandle<SelectionMode>("RangedSelectionMode", "Ranged selection preference", "Which kind of weapon will your pawns use first when holding multiple ranged sidearms.", SelectionMode.Strongest);

            MaxSingleMeleeMass = Settings.GetHandle<float>("MaxSingleMeleeMass", "MaxSingleMeleeMass_title".Translate(), "MaxSingleMeleeMass_desc".Translate(), 0.15f);
            MaxTotalMeleeMass = Settings.GetHandle<float>("MaxTotalMeleeMass", "MaxTotalMeleeMass_title".Translate(), "MaxTotalMeleeMass_desc".Translate(), 0.20f);
            MaxSingleRangedMass = Settings.GetHandle<float>("MaxSingleRangedMass", "MaxSingleRangedMass_title".Translate(), "MaxSingleRangedMass_desc".Translate(), 0.15f);
            MaxTotalRangedMass = Settings.GetHandle<float>("MaxTotalRangedMass", "MaxTotalRangedMass_title".Translate(), "MaxTotalRangedMass_desc".Translate(), 0.20f);
            MaxTotalMass = Settings.GetHandle<float>("MaxTotalMass", "MaxTotalMass_title".Translate(), "MaxTotalMass_desc".Translate(), 0.30f);

            SidearmSpawnChance = Settings.GetHandle<float>("SidearmSpawnChance", "SidearmSpawnChance_title".Translate(), "SidearmSpawnChance_desc".Translate(), 0.65f);

        }

        public override void Initialize()
        {
            base.Initialize();
            //switchToMeleeSidearm = new Gizmo();
        }

        //attach to this instead of PawnWeaponGenerator since it wipes the inventory
        /*    [HarmonyPatch(typeof(PawnInventoryGenerator), "GenerateInventoryFor")]
            static class PawnInventoryGenerator_GenerateInventoryFor_Postfix
            {
                [HarmonyPostfix]
                private static void GenerateInventoryFor(Pawn p, PawnGenerationRequest request)
                {
                    Log.Message("GenerateInventoryFor postfix");
                    Pawn pawn = p;

                    if (pawn.equipment == null)
                        return;
                    if (pawn.equipment.Primary == null)
                        return;
                    if (pawn.equipment.Primary.def.IsMeleeWeapon)
                        return;

                    float rand = Rand.ValueSeeded(pawn.thingIDNumber ^ 1544445223);
                    Log.Message("randomed " + rand);
                    if (rand > 0f)
                    {
                        //AAAAH KILL IT WITH FIRE
                        //but seriously, since I dont think you can access the list of items to generate from 
                        //without reflection... 
                        ThingWithComps oldWep = pawn.equipment.Primary;
                        float saveMin = pawn.kindDef.weaponMoney.min;
                        float saveMax = pawn.kindDef.weaponMoney.max;
                        pawn.kindDef.weaponMoney.min = saveMin / 3;
                        pawn.kindDef.weaponMoney.max = saveMax / 3;
                        PawnWeaponGenerator.TryGenerateWeaponFor(pawn);
                        pawn.kindDef.weaponMoney.min = saveMin;
                        pawn.kindDef.weaponMoney.max = saveMax;
                        ThingWithComps newWep = pawn.equipment.Primary;
                        pawn.equipment.Remove(newWep);
                        pawn.equipment.AddEquipment(oldWep);
                        pawn.inventory.innerContainer.TryAdd(newWep);

                        Log.Message("old wep " + oldWep.LabelShort);
                        Log.Message("new wep " + newWep.LabelShort);
                    }                   
                }
            }*/

        /*[HarmonyPatch(typeof(Stance_Cooldown), "StanceTick")]
        static class Stance_Stance_Cooldown_Postfix
        {
            [HarmonyPostfix]
            private static void StanceTick(Stance_Warmup __instance)
            {
                Pawn pawn = __instance.stanceTracker.pawn;
                Log.Message("cooldown stance postfix + ("+pawn.LabelShort+")");
                if (!(__instance.verb is Verb_Shoot))
                    return;
                //if (__instance.Primary == null || __instance.Primary.def.IsMeleeWeapon)
                //    return;

                LocalTargetInfo targ = __instance.focusTarg;

                if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
                {
                    CellRect cellRect = (!targ.HasThing) ? CellRect.SingleCell(targ.Cell) : targ.Thing.OccupiedRect();
                    float range = cellRect.ClosestDistSquaredTo(pawn.Position);
                    UtilFuncs.trySwapToMoreAccurateRangedWeapon(pawn, UtilFuncs.shouldDrop(DroppingModeEnum.Range), range);
                }
            }
        }*/

        /*
        [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryStartAttack")]
        static class Pawn_EquipmentTracker_TryStartAttack_Prefix
        {
            [HarmonyPrefix]
            private static void TryStartAttack(Pawn_EquipmentTracker __instance, LocalTargetInfo targ)
            {
                Log.Message("start attack prefix");
                if (__instance.Primary == null || __instance.Primary.def.IsMeleeWeapon)
                    return;

                Pawn pawn = ((Pawn)__instance.ParentHolder);

                if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
                {
                    CellRect cellRect = (!targ.HasThing) ? CellRect.SingleCell(targ.Cell) : targ.Thing.OccupiedRect();
                    float range = cellRect.ClosestDistSquaredTo(pawn.Position);
                    UtilFuncs.trySwapToMoreAccurateRangedWeapon(pawn, UtilFuncs.shouldDrop(DroppingModeEnum.Range), range);
                }
            }
        }*/


        /*
        [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
        static class Pawn_PreApplyDamage_PostFix
        {
            [HarmonyPostfix]
            private static void PreApplyDamage(Pawn __instance, DamageInfo dinfo)
            {
                UtilFuncs.doCQC(__instance, dinfo.Instigator as Pawn);
            }
        }
        */

    }
}
