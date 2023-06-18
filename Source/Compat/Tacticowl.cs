using HarmonyLib;
using SimpleSidearms.rimworld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static HarmonyLib.AccessTools;

namespace PeteTimesSix.SimpleSidearms.Compat
{
    [StaticConstructorOnStartup]
    public static class Tacticowl
    {
        public static bool active = false;

        public static FieldRef<bool> dualWieldActive;
        public delegate bool GetOffHand(Pawn pawn, out ThingWithComps thing);
        public static GetOffHand getOffHand;
        public delegate void SetOffHand(Pawn pawn, ThingWithComps thing, bool removing);
        public static SetOffHand setOffHand;
        private static SetOffHand _setOffHandTrue;
        public delegate bool IsOffHand(Thing thing);
        public static IsOffHand isOffHand;
        public delegate bool IsTwoHanded(Def def);
        public static IsTwoHanded isTwoHanded;
        public delegate bool CanBeOffHand(Def def);
        public static CanBeOffHand canBeOffHand;
        public delegate void SetWeaponAsOffHand(ThingWithComps weapon, bool set);
        public static SetWeaponAsOffHand setWeaponAsOffHand;

        static Tacticowl() 
        {
            if (ModLister.GetActiveModWithIdentifier("Owlchemist.Tacticowl") != null)
            {
                active = true;

                dualWieldActive = AccessTools.StaticFieldRefAccess<bool>(AccessTools.TypeByName("Tacticowl.ModSettings_Tacticowl").GetField("dualWieldEnabled"));

                getOffHand = AccessTools.MethodDelegate<GetOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("GetOffHander"));
                setOffHand = AccessTools.MethodDelegate<SetOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("SetOffHander"));
                isOffHand = AccessTools.MethodDelegate<IsOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("IsOffHandedWeapon"));
                isTwoHanded = AccessTools.MethodDelegate<IsTwoHanded>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("IsTwoHanded"));
                canBeOffHand = AccessTools.MethodDelegate<CanBeOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("CanBeOffHand"));
                setWeaponAsOffHand = AccessTools.MethodDelegate<SetWeaponAsOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("SetWeaponAsOffHanded"));
            }
        }


        public static void Patch_Delayed_Tacticowl(Harmony harmony)
        {
            Type dualWieldExtensions = AccessTools.TypeByName("Tacticowl.DualWieldExtensions");
            //harmony.Patch(AccessTools.Method(dualWieldExtensions, "SetOffHander"), postfix: new HarmonyMethod(typeof(Tacticowl), nameof(SetOffHander_Postfix)));

            var jobDriver_EquipOffHand_initAction = AccessTools.FirstMethod(AccessTools.TypeByName("Tacticowl.DualWield.JobDriver_EquipOffHand"), (MethodInfo m) => m.Name == "<MakeNewToils>b__1_0");
            SimpleSidearms.Harmony.Patch(jobDriver_EquipOffHand_initAction, transpiler: new HarmonyMethod(AccessTools.Method(typeof(Tacticowl), nameof(Tacticowl.JobDriver_EquipOffHand_initAction_Transpiler))));
        }

        public static void JobDriver_EquipOffHand_initAction_Infix(Pawn pawn, ThingWithComps thing) 
        {
            CompSidearmMemory.GetMemoryCompForPawn(pawn)?.InformOfAddedSidearm(thing); //sometimes null during worldgen?
        }

        public static IEnumerable<CodeInstruction> JobDriver_EquipOffHand_initAction_Transpiler(IEnumerable<CodeInstruction> instructions) 
        {
            var codeMatcher = new CodeMatcher(instructions);

            CodeMatch[] toMatch = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Call, AccessTools.Method("Tacticowl.DualWieldExtensions:SetOffHander"))
            };

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn))),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Tacticowl), nameof(Tacticowl.JobDriver_EquipOffHand_initAction_Infix)))
            };

            codeMatcher.MatchEndForward(toMatch).Advance(1);
            codeMatcher.Insert(toInsert);
            codeMatcher.End();

            return codeMatcher.InstructionEnumeration();
        }
    }
}
