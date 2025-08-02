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
        public delegate bool UsableWithShields(ThingDef def);
        public static UsableWithShields usableWithShields;

        static VFECore() 
        {
            if (ModLister.GetActiveModWithIdentifier("OskarPotocki.VanillaFactionsExpanded.Core", true) != null)
            {
                active = true;

                try
                {
                    offHandShield = AccessTools.MethodDelegate<OffHandShield>(AccessTools.TypeByName("VEF.Apparels.ShieldUtility").GetMethod("OffHandShield"));
                    usableWithShields = AccessTools.MethodDelegate<UsableWithShields>(AccessTools.TypeByName("VEF.Apparels.ShieldUtility").GetMethod("UsableWithShields"));
                }
                catch (Exception ex)
                {
                    Log.Warning("SS: Failed to initialize compat. with VEF. Compat will be disabled. Exception: "+ex.ToString());

                    active = false;

                    offHandShield = null;
                    usableWithShields = null;
                }
            }
        }

        public static void Patch_Delayed_VFECore(Harmony harmony)
        {
        }
    }
}
