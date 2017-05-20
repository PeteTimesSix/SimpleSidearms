﻿using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using SimpleSidearms.utilities;
using static SimpleSidearms.Globals;

namespace SimpleSidearms.intercepts
{

    [HarmonyPatch(typeof(Verb_ShootOneUse), "SelfConsume")]
    static class Verb_ShootOneUse_SelfConsume_Postfix
    {
        [HarmonyPostfix]
        private static void SelfConsume(Verb_ShootOneUse __instance)
        {
            if (SimpleSidearms.SingleshotAutoSwitch == true)
            {
                if (__instance.caster is Pawn)
                {
                    Pawn pawn = (__instance.caster as Pawn);
                    if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsRangedWeapon))
                    {
                        WeaponAssingment.tryRangedWeaponSwap(pawn, MiscUtils.shouldDrop(DroppingModeEnum.UsedUp), pawn.IsColonistPlayerControlled);
                    }

                    else if (pawn.inventory.innerContainer.Any((Thing x) => x.def.IsMeleeWeapon))
                    {
                        WeaponAssingment.tryMeleeWeaponSwap(pawn, MiscUtils.shouldDrop(DroppingModeEnum.UsedUp), false, pawn.IsColonistPlayerControlled);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    static class Verb_MeleeAttack_TryCastShot_PostFix
    {
        [HarmonyPostfix]
        private static void TryCastShot(Verb_MeleeAttack __instance)
        {
            //Log.Message("tryCastShot intercepted");
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = (__instance.GetType()).GetField("currentTarget", bindFlags);
            object fieldValue = field.GetValue(__instance);
            if (fieldValue != null && fieldValue is LocalTargetInfo)
            {
                Thing targetThing = ((LocalTargetInfo)fieldValue).Thing;
                if (__instance.CasterPawn != null && targetThing != null && targetThing is Pawn && !(targetThing as Pawn).Dead && (targetThing as Pawn).equipment != null)
                {
                    WeaponAssingment.doCQC(targetThing as Pawn, __instance.CasterPawn);
                }
            }
        }
    }
}
