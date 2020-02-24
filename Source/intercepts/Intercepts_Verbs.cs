using HarmonyLib;
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
            if (__instance.caster is Pawn)
            {
                Pawn pawn = (__instance.caster as Pawn);
                ThingStuffPair weapon = __instance.EquipmentSource.toThingStuffPair();
                bool anotherFound = WeaponAssingment.equipSpecificWeaponTypeFromInventory(pawn, weapon, false, false);
                if (!anotherFound)
                    WeaponAssingment.equipBestWeaponFromInventoryByPreference(pawn, DroppingModeEnum.UsedUp);
            }
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    static class Verb_MeleeAttack_TryCastShot_PostFix
    {

        [HarmonyPostfix]
        private static void TryCastShot(Verb_MeleeAttack __instance)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = (__instance.GetType()).GetField("currentTarget", bindFlags);
            object fieldValue = field.GetValue(__instance);
            if (fieldValue != null && fieldValue is LocalTargetInfo)
            {
                Thing targetThing = ((LocalTargetInfo)fieldValue).Thing;
                if (__instance.CasterPawn != null && targetThing != null && targetThing is Pawn && !(targetThing as Pawn).Dead && (targetThing as Pawn).RaceProps.Humanlike && (targetThing as Pawn).equipment != null)
                {
                    WeaponAssingment.doCQC(targetThing as Pawn, __instance.CasterPawn);
                }
            }
        }
    }
}
