using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Compat
{
    public static class OptionalPatches
    {
        public static void Patch(Harmony harmony)
        {
            //Log.Warning("Doing optional patches...");
        }

        public static void PatchDelayed(Harmony harmony)
        {
            //if (ModLister.GetActiveModWithIdentifier("Owlchemist.Tacticowl") != null)
            {
                try
                {

                }
                catch (Exception e) 
                {
                    Log.Error("SS: Error during patching VFE: " + e);
                }
                //Log.Warning("Doing VFECore patches...");
                //Patch_Delayed_VFECore(harmony);
            }
            if (Tacticowl.active)
            {
                //Log.Warning("Doing Tacticowl patches...");
                try
                {
                    Tacticowl.Patch_Delayed_Tacticowl(harmony);
                }
                catch (Exception e) 
                {
                    Log.Error("SS: Error during patching Tacticowl: " + e);
                }
            }
        }
    }
}
