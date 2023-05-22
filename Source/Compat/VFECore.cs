using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Compat
{
    [StaticConstructorOnStartup]
    public static class VFECore
    {
        public static bool active = false;

        public delegate ThingWithComps OffHandShield(Pawn pawn);
        public static OffHandShield offHandShield;

        static VFECore() 
        {
            if (ModLister.GetActiveModWithIdentifier("OskarPotocki.VanillaFactionsExpanded.Core") != null)
            {
                active = true;

                offHandShield = AccessTools.MethodDelegate<OffHandShield>(AccessTools.TypeByName("VFECore.ShieldUtility").GetMethod("OffHandShield"));
            }
        }

        public static void Patch_Delayed_VFECore(Harmony harmony)
        {
            //throw new NotImplementedException();
        }
    }
}
