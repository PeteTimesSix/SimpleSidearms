using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static SimpleSidearms.Globals;
using SimpleSidearms.utilities;
using SimpleSidearms.rimworld;

namespace SimpleSidearms.intercepts
{

    [HarmonyPatch(typeof(Stance_Warmup), "StanceTick")]
    static class Stance_Warmup_StanceTick_Postfix
    {
        [HarmonyPostfix]
        private static void StanceTick(Stance_Warmup __instance)
        {
            if (SimpleSidearms.RangedCombatAutoSwitch == false)
                return;
            Pawn pawn = __instance.stanceTracker.pawn;
            if (SwapControlsHandler.GetHandlerForPawn(pawn).currentWeaponLocked)
                return;
            if (IsHunting(pawn))
                return;
            if (!(__instance.verb is Verb_Shoot))
                return;
            float statValue = pawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
            int ticks = (__instance.verb.verbProps.warmupTime * statValue).SecondsToTicks();
            
            if (__instance.ticksLeft / (float)ticks < 1f - SimpleSidearms.RangedCombatAutoSwitchMaxWarmup.Value)
            {
                return;
            }

            LocalTargetInfo targ = __instance.focusTarg;

            if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
            {
                CellRect cellRect = (!targ.HasThing) ? CellRect.SingleCell(targ.Cell) : targ.Thing.OccupiedRect();
                float range = cellRect.ClosestDistSquaredTo(pawn.Position);
                WeaponAssingment.trySwapToMoreAccurateRangedWeapon(pawn, MiscUtils.shouldDrop(DroppingModeEnum.Range), range, pawn.IsColonistPlayerControlled);
            }
        }

        private static bool IsHunting(Pawn pawn)
        {
            if (pawn.CurJob == null)
            {
                return false;
            }
            JobDriver_Hunt jobDriver_Hunt = pawn.jobs.curDriver as JobDriver_Hunt;
            JobDriver_PredatorHunt jobDriver_PredatorHunt = pawn.jobs.curDriver as JobDriver_PredatorHunt;
            return jobDriver_Hunt != null | jobDriver_PredatorHunt != null;
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "AddEquipment")]
    static class Pawn_EquipmentTracker_AddEquipment_Postfix
    {
        //EW EW EW GLOBAL FLAG EW EW
        public static bool sourcedBySimpleSidearms = false;

        [HarmonyPostfix]
        private static void AddEquipment(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
        {
            if (!sourcedBySimpleSidearms)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawn == null)
                    return;

                GoldfishModule.GetGoldfishForPawn(pawn).PickupPrimary(newEq.def, true);
            }
        }
    }
}
