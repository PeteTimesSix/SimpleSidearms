﻿using HarmonyLib;
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
    public static class JobDriver_Equip_MakeNewToils_Patches
    {
        private static readonly CodeMatch[] toMatch = new CodeMatch[]
        {
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn))),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.equipment))),
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment)))
        };

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> CalculateMethods(Harmony instance)
        {
            var candidates = AccessTools.GetDeclaredMethods(typeof(JobDriver_Equip)).ToHashSet();
            candidates.AddRange(typeof(JobDriver_Equip).GetNestedTypes(AccessTools.all).SelectMany(t => AccessTools.GetDeclaredMethods(t)));

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
        public static IEnumerable<CodeInstruction> JobDriver_Equip_MakeNewToils_Patches_initAction_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn))),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JobDriver_Equip_MakeNewToils_Patches), nameof(MemoriseJustEquippedWeapon)))
            };

            codeMatcher.MatchEndForward(toMatch);
            codeMatcher.Advance(1);
            codeMatcher.Insert(toInsert);

            if (codeMatcher.IsInvalid)
            {
                Log.Warning("SS: failed to apply transpiler on JobDriver_Equip_MakeNewToils_Patches_initAction!");
                return instructions;
            }
            else
                return codeMatcher.InstructionEnumeration();
        }

        public static void MemoriseJustEquippedWeapon(Pawn pawn, ThingWithComps weapon)
        {
            if (!pawn.IsValidSidearmsCarrierRightNow())
                return;
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return;
            pawnMemory.InformOfAddedPrimary(weapon);
        }
    }
}
