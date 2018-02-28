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
using Verse.AI;

namespace SimpleSidearms.intercepts
{

    [HarmonyPatch(typeof(AutoUndrafter), "AutoUndraftTick")]
    static class AutoUndrafter_AutoUndraftTick_Postfix
    {
        [HarmonyPostfix]
        private static void AutoUndraftTick(AutoUndrafter __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (Find.TickManager.TicksGame % 100 == 0)
            {
                if (pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.WaitCombat && pawn.stances != null && pawn.stances.curStance is Stance_Mobile)
                {
                    //pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                    
                    WeaponAssingment.reequipPrimaryIfNeededAndAvailable(pawn, GoldfishModule.GetGoldfishForPawn(pawn));
                }
            }
        }
    }

    //Commented out because it just doesnt work properly.
    //Maybe someday I can think of a better way of doing it.


    /*[HarmonyPatch(typeof(JobGiver_Orders), "TryGiveJob")]
    static class JobGiver_Orders_TryGiveJob_Postfix
    {
        [HarmonyPostfix]
        private static void TryGiveJob(JobGiver_Orders __instance, Pawn pawn, ref Job __result)
        {
            if (__result == null)
                return;
            if (__result.def != JobDefOf.WaitCombat)
                return;
            if (pawn.Downed)
                return;
            if (!(pawn.stances.curStance is Stance_Mobile))
                return;          


            Job retrieval = JobGiver_RetrieveWeapon.TryGiveJobStatic(pawn, true);
            if (retrieval != null)
            {
                __result = retrieval;
                return;
            }
        }
    }*/


    [HarmonyPatch(typeof(Stance_Warmup), "StanceTick")]
    static class Stance_Warmup_StanceTick_Postfix
    {
        private static Type ceRangedVerb;
        private static Type CERangedVerb {
            get {
                if(ceRangedVerb == null)
                    ceRangedVerb = AccessTools.TypeByName("CombatExtended.Verb_ShootCE");
                return ceRangedVerb;
            }
        }

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
            if (!SimpleSidearms.CEOverride && !(__instance.verb is Verb_Shoot))
                return;
            if (SimpleSidearms.CEOverride && !(CERangedVerb.IsAssignableFrom(__instance.verb.GetType())))
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
            else
            {
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
                GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
                if (pawnMemory == null)
                    return;
                pawnMemory.PickupPrimary(newEq.def, true);
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_AttackMelee), "TryMakePreToilReservations")]
    static class JobDriver_AttackMelee_TryMakePreToilReservations
    {
        private static void Postfix(JobDriver_AttackMelee __instance)
        {
            Pawn caster = ((__instance.GetType()).GetField("pawn").GetValue(__instance) as Pawn);
            Job job = ((__instance.GetType()).GetField("job").GetValue(__instance) as Job);
            Thing target = job?.targetA.Thing;
            if (caster != null && target != null && target is Pawn && !caster.Dead)
            {
                WeaponAssingment.chooseOptimalMeleeForAttack(caster, target as Pawn);
            }
        }
    }
}
