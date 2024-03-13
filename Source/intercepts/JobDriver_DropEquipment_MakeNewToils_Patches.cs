using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using SimpleSidearms.rimworld;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{
    [HarmonyPatch]
    public static class JobDriver_DropEquipment_MakeNewToils_Patches
    {
        private static readonly CodeMatch[] toMatch = new CodeMatch[]
        {
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn))),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.equipment))),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(JobDriver_DropEquipment), "TargetEquipment"))
        };

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> CalculateMethods(Harmony instance)
        {
            var candidates = AccessTools.GetDeclaredMethods(typeof(JobDriver_DropEquipment)).ToHashSet();
            candidates.AddRange(typeof(JobDriver_DropEquipment).GetNestedTypes(AccessTools.all).SelectMany(t => AccessTools.GetDeclaredMethods(t)));

            foreach (var method in candidates)
            {
                var instructions = PatchProcessor.GetCurrentInstructions(method);
                var matched = new CodeMatcher(instructions).MatchStartForward(toMatch).IsValid;
                if (matched)
                    yield return method;
            }
            yield break;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> JobDriver_DropEquipment_MakeNewToils_Patches_initAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JobDriver_DropEquipment_MakeNewToils_Patches), nameof(UnmemoriseJustDroppedWeapon)))
            };

            codeMatcher.MatchEndForward(toMatch);
            codeMatcher.Advance(1);
            codeMatcher.Insert(toInsert);

            if (codeMatcher.IsInvalid)
            {
                Log.Warning("SS: failed to apply transpiler on JobDriver_DropEquipment_MakeNewToils_Patches!");
                return instructions;
            }
            else
                return codeMatcher.InstructionEnumeration();
        }

        public static void UnmemoriseJustDroppedWeapon(ThingWithComps weapon, Pawn pawn)
        {
            if (weapon == null)
                return;
            if (!pawn.IsValidSidearmsCarrierRightNow())
                return;
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return;
            pawnMemory.InformOfDroppedSidearm(weapon, true);
        }
    }
}
