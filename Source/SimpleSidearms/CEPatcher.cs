using Harmony;
using SimpleSidearms.intercepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace SimpleSidearms
{
    static class CEPatcher
    {
        public static bool isCEPresent(HarmonyInstance harmony)
        {
            Type verbShootType = AccessTools.TypeByName("CombatExtended.Verb_ShootCE");
            return verbShootType != null;
        }

        public static void patchCE(HarmonyInstance harmony)
        {
            Type meleeAtk = AccessTools.TypeByName("CombatExtended.Verb_MeleeAttackCE");

            MethodInfo original = meleeAtk.GetMethod("TryCastShot", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo postfix = typeof(Verb_MeleeAttack_TryCastShot_PostFix).GetMethod("TryCastShot", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(original, null, new HarmonyMethod(postfix));

            Type oneUse = AccessTools.TypeByName("CombatExtended.Verb_ShootCEOneUse");

            original = oneUse.GetMethod("SelfConsume", BindingFlags.NonPublic | BindingFlags.Instance);
            postfix = typeof(Verb_ShootOneUse_SelfConsume_Postfix).GetMethod("SelfConsume", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(original, null, new HarmonyMethod(postfix));

            //harmony.Patch()
        }
    }
}
