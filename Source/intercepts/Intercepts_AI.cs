using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using System.Reflection.Emit;
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
        private const int autoRetrieveDelay = 300;

        [HarmonyPostfix]
        private static void AutoUndraftTick(AutoUndrafter __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            int tick = Find.TickManager.TicksGame;
            if (tick % 100 == 0)
            {
                if (pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.Wait_Combat && pawn.stances != null && pawn.stances.curStance is Stance_Mobile)
                {
                    //pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                    
                    WeaponAssingment.equipBestWeaponFromInventoryByPreference(pawn, DroppingModeEnum.Combat);

                    BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    FieldInfo field = (__instance.GetType()).GetField("lastNonWaitingTick", bindFlags);
                    int lastNonWaitingTick = (int)field.GetValue(__instance);
                    if (tick - lastNonWaitingTick > autoRetrieveDelay)
                    {
                        Job retrieval = JobGiver_RetrieveWeapon.TryGiveJobStatic(pawn, true);
                        if (retrieval != null)
                            pawn.jobs.TryTakeOrderedJob(retrieval, JobTag.Misc);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "Drafted", MethodType.Setter)]
    static class Pawn_DraftController_Drafted_Setter_Postfix
    {
        [HarmonyPostfix()]
        private static void DraftedSetter(Pawn_DraftController __instance)
        {
            //Log.Message("undraft intercept: " + __instance.Drafted);
            if (!__instance.Drafted)
            {
                Pawn pawn = __instance.pawn;
                if (pawn != null && !pawn.Dead && pawn.IsColonist)
                {
                    GoldfishModule.GetGoldfishForPawn(pawn).InformOfUndraft();
                }
            }
        }
    }

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

            LocalTargetInfo target = __instance.focusTarg;

            WeaponAssingment.trySwapToMoreAccurateRangedWeapon(pawn, target, MiscUtils.shouldDrop(DroppingModeEnum.Combat), pawn.IsColonistPlayerControlled);
        }

        private static bool IsHunting(Pawn pawn)
        {
            if (pawn.CurJob == null)
            {
                return false; 
            }
            JobDriver_Hunt jobDriver_Hunt = pawn.jobs.curDriver as JobDriver_Hunt;
            JobDriver_PredatorHunt jobDriver_PredatorHunt = pawn.jobs.curDriver as JobDriver_PredatorHunt;
            return jobDriver_Hunt != null || jobDriver_PredatorHunt != null;
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
                pawnMemory.InformOfAddedPrimary(newEq);
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
            if (caster != null && target != null && target is Pawn && !caster.Dead/* && caster.def.race.Humanlike*/)
            {
                WeaponAssingment.chooseOptimalMeleeForAttack(caster, target as Pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_InventoryTracker))]
    [HarmonyPatch("FirstUnloadableThing", MethodType.Getter)]
    static class Pawn_InventoryTracker_FirstUnloadableThing
    {

        [HarmonyPostfix]
        public static void Postfix(Pawn_InventoryTracker __instance, ref ThingCount __result)
        {
            if (__result == default(ThingCount) ||
                !__result.Thing.def.IsWeapon)
            {
                return;
            }
            else 
            {
                Pawn pawn = __instance.pawn;
                GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
                List<ThingStuffPair> desiredSidearms = pawnMemory.RememberedWeapons.ListFullCopy();
                if (desiredSidearms.Contains(pawn.equipment.Primary.toThingStuffPair()))
                    desiredSidearms.Remove(pawn.equipment.Primary.toThingStuffPair());

                int inventoryOffset = 0;

                //TODO: this does not preserve best possible weapon, just whichever one is first in inventory. Maybe fix?
                while (inventoryOffset < __instance.innerContainer.Count)
                {
                    Thing candidate = __instance.innerContainer[inventoryOffset];
                    if (candidate.def.IsWeapon & desiredSidearms.Contains(candidate.toThingStuffPair()))
                    {
                        desiredSidearms.Remove(candidate.toThingStuffPair());
                    }
                    else
                    {
                        __result = new ThingCount(candidate, candidate.stackCount);
                        return;
                    }
                    inventoryOffset++;
                }
                __result = default(ThingCount);
                return;
            }
        }
    }
}
