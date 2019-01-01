using Harmony;
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
        [HarmonyPostfix]
        private static void AutoUndraftTick(AutoUndrafter __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (Find.TickManager.TicksGame % 100 == 0)
            {
                if (pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.Wait_Combat && pawn.stances != null && pawn.stances.curStance is Stance_Mobile)
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
            if (caster != null && target != null && target is Pawn && !caster.Dead/* && caster.def.race.Humanlike*/)
            {
                WeaponAssingment.chooseOptimalMeleeForAttack(caster, target as Pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_InventoryTracker))]
    [HarmonyPatch("FirstUnloadableThing", MethodType.Getter)]
    static class Pawn_InventoryTracker_FirstUnloadableThing_Transpiler
    {
        public static bool IsBestSidearm(Pawn pawn, Thing weapon)
        {
            if (!weapon.def.IsWeapon) return false;
            ThingDef weaponDef = weapon.def;

            //Go ahead and drop if same type is already equipped
            if (pawn.equipment?.Primary?.def == weaponDef)
                return false;

            GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
            if (pawnMemory != null)
            {
                foreach (string wepName in pawnMemory.weapons)
                {
                    if (weaponDef.defName == wepName)
                    {
                        List<Thing> matchingWeapons = pawn.inventory?.innerContainer.Where(t => t.def == weaponDef).ToList() ?? new List<Thing> ();

                        Thing bestWeapon = matchingWeapons.MaxBy(t => t.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, false));
                        //Ranged damage scales with melee damage so this works for all weapons
                        
                        return bestWeapon == weapon;
                    }
                }
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase mb)
        {
            //get_FirstUnloadableThing should ignore sidearms.
            //It already loops through the inventory and checks for drug usage.
            //Need to add a check for sidearms and skip it.

            //First, we need to add a continue statement to the inventory's for loop
            //ldloc_3 is the method's local variable indexed by 3, in this case the inventory for loop index
            //the last reference to it is the loop test, second to last is the increment ( where it should jump to continue the loop )

            //So : reverse the instructions and label the second to last reference to ld_loc3

            Label forContinue = il.DefineLabel();
            
            bool foundIndexOnce = false;

            foreach (CodeInstruction i in instructions.Reverse())
            {
                if (i.opcode == OpCodes.Ldloc_3)
                {
                    if (foundIndexOnce)
                    {
                        i.labels.Add(forContinue);
                        break;
                    }
                    else
                        foundIndexOnce = true;
                }
            }

            //Now, when IsDrug is checked, we also need to sheck for sidearms and continue
            //

            MethodInfo isDrugInfo = AccessTools.Property(typeof(ThingDef), "IsDrug").GetGetMethod();
            FieldInfo thisPawnInfo = AccessTools.Field(typeof(Pawn_InventoryTracker), "pawn");
            FieldInfo innerContainerInfo = AccessTools.Field(typeof(Pawn_InventoryTracker), "innerContainer");
            MethodInfo getItemInfo = AccessTools.Property(typeof(ThingOwner<Thing>), "Item").GetGetMethod();

            MethodInfo isBestSidearmInfo = AccessTools.Method(typeof(Pawn_InventoryTracker_FirstUnloadableThing_Transpiler), "IsBestSidearm");

            bool afterIsDrug = false;
            foreach (CodeInstruction i in instructions)
            {
                yield return i;
                if (afterIsDrug)
                {
                    //Call IsBestSidearms(Pawn, Thing)
                    //Call IsBestSidearms(this.pawn, this.innercontainer[index])

                    //IL_001a: ldarg.0      // this
                    //IL_001b: ldfld        class Verse.Pawn Verse.Pawn_InventoryTracker::pawn
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, thisPawnInfo);

                    //IL_00b1: ldarg.0      // this
                    //IL_00b2: ldfld        class Verse.ThingOwner`1<class Verse.Thing> Verse.Pawn_InventoryTracker::innerContainer
                    //IL_00b7: ldloc.3      // index1
                    //IL_00b8: callvirt instance !0/*class Verse.Thing*/ class Verse.ThingOwner`1<class Verse.Thing>::get_Item(int32)
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, innerContainerInfo);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Callvirt, getItemInfo);

                    //Call IsBestSidearms(Pawn, Thing)
                    yield return new CodeInstruction(OpCodes.Call, isBestSidearmInfo);

                    //if (IsBestSidearms(Pawn, THing)  
                    //  continue;
                    yield return new CodeInstruction(OpCodes.Brtrue, forContinue);
                    
                    afterIsDrug = false;
                }
                if (i.opcode == OpCodes.Callvirt && i.operand == isDrugInfo)
                {
                    afterIsDrug = true;
                }
            }
        }
    }
}
