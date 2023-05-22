using HarmonyLib;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Compat
{
    [StaticConstructorOnStartup]
    public static class Tacticowl
    {
        public static bool active = false;

        public delegate bool GetOffHand(Pawn pawn, out ThingWithComps thing);
        public static GetOffHand getOffHand;
        public delegate void SetOffHand(Pawn pawn, ThingWithComps thing, bool removing);
        public static SetOffHand setOffHand;
        public delegate bool IsOffHand(Thing thing);
        public static IsOffHand isOffHand;
        public delegate bool IsTwoHanded(Def def);
        public static IsTwoHanded isTwoHanded;
        public delegate bool CanBeOffHand(Def def);
        public static CanBeOffHand canBeOffHand;

        static Tacticowl() 
        {
            if (ModLister.GetActiveModWithIdentifier("Owlchemist.Tacticowl") != null)
            {
                active = true;
                getOffHand = AccessTools.MethodDelegate<GetOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("GetOffHander"));
                setOffHand = AccessTools.MethodDelegate<SetOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("SetOffHander"));
                isOffHand = AccessTools.MethodDelegate<IsOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("IsOffHandedWeapon"));
                isTwoHanded = AccessTools.MethodDelegate<IsTwoHanded>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("IsTwoHanded"));
                canBeOffHand = AccessTools.MethodDelegate<CanBeOffHand>(AccessTools.TypeByName("Tacticowl.DualWieldExtensions").GetMethod("CanBeOffHand"));
            }
        }


        public static void Patch_Delayed_Tacticowl(Harmony harmony)
        {
            Type dualWieldExtensions = AccessTools.TypeByName("Tacticowl.DualWieldExtensions");
            harmony.Patch(AccessTools.Method(dualWieldExtensions, "SetOffHander"), postfix: new HarmonyMethod(typeof(Tacticowl), nameof(SetOffHander_Postfix)));


            /*Type type = AccessTools.TypeByName("Tacticowl.DualWield.JobDriver_EquipOffHand");

            DMM_Patch_BillStack_DoListing_Patches.GizmoListRect = AccessTools.StaticFieldRefAccess<Rect>(AccessTools.Field(type, "GizmoListRect"));
            harmony.Patch(AccessTools.Method(type, "Doink"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.Doink_Transpiler))));
            harmony.Patch(AccessTools.Method(type, "DoRow"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(DMM_Patch_BillStack_DoListing_Patches), nameof(DMM_Patch_BillStack_DoListing_Patches.DoRow_Transpiler))));*/
        }

        public static void SetOffHander_Postfix(Pawn pawn, ThingWithComps thing, bool removing)
        {
            Log.Message($"tacticowl postfix: {pawn} {thing} {removing}");
            if(!removing && thing != null)
            {
                CompSidearmMemory.GetMemoryCompForPawn(pawn).InformOfAddedSidearm(thing);
            }
        }
    }
}
