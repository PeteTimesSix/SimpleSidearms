using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using SimpleSidearms.utilities;
using SimpleSidearms.rimworld;

namespace SimpleSidearms.intercepts
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "InterfaceDrop")]
    static class ITab_Pawn_Gear_InterfaceDrop_Prefix
    {
        [HarmonyPrefix]
        private static void InterfaceDrop(ITab_Pawn_Gear __instance, Thing t)
        {
            if (t.def.IsMeleeWeapon || t.def.IsRangedWeapon)
            {
                ThingWithComps thingWithComps = t as ThingWithComps;
                ThingOwner thingOwner = thingWithComps.holdingOwner;
                IThingHolder actualOwner = thingOwner.Owner;
                if (actualOwner is Pawn_InventoryTracker)
                {
                    GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn((actualOwner as Pawn_InventoryTracker).pawn);
                    if (pawnMemory == null)
                        return;
                    pawnMemory.DropSidearm(thingWithComps.def, true);
                }
                else if(actualOwner is Pawn_EquipmentTracker)
                {
                    GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn((actualOwner as Pawn_EquipmentTracker).ParentHolder as Pawn);
                    if (pawnMemory == null)
                        return;
                    pawnMemory.DropPrimary(true);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Pawn_GetGizmos_Postfix
    {
        [HarmonyPostfix]
        private static void GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.IsColonistPlayerControlled)
            {
                if (__instance.inventory != null)
                {
                    List<Thing> rangedWeapons;
                    List<Thing> meleeWeapons;
                    GettersFilters.getWeaponLists(out rangedWeapons, out meleeWeapons, __instance.inventory);

                    GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(__instance);
                    //if (pawnMemory == null)
                    //    return;

                    if (rangedWeapons.Count > 0 || meleeWeapons.Count > 0 || (pawnMemory != null && pawnMemory.weapons.Count > 0))
                    {
                        List<ThingDef> rangedWeaponMemories = new List<ThingDef>();
                        List<ThingDef> meleeWeaponMemories = new List<ThingDef>();
                        
                        if(pawnMemory != null)
                        {
                            foreach (string weapon in pawnMemory.weapons)
                            {
                                ThingDef wepDef = DefDatabase<ThingDef>.GetNamedSilentFail(weapon);

                                if (wepDef == null)
                                    continue;

                                if (wepDef.IsMeleeWeapon)
                                    meleeWeaponMemories.Add(wepDef);
                                else if (wepDef.IsRangedWeapon)
                                    rangedWeaponMemories.Add(wepDef);
                            }
                        }
                       
                        Gizmo_SidearmsList advanced = new Gizmo_SidearmsList(__instance, rangedWeapons, meleeWeapons, rangedWeaponMemories, meleeWeaponMemories);
                        advanced.defaultLabel = "DrawSidearm_gizmoTitle".Translate();
                        //draft.hotKey = KeyBindingDefOf.CommandColonistDraft;
                        advanced.defaultDesc = "DrawSidearm_gizmoTooltip".Translate();

                        List<Gizmo> results = new List<Gizmo>();
                        foreach (Gizmo gizmo in __result)
                        {
                            results.Add(gizmo);
                        }
                        results.Add(advanced);
                        __result = results;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>) })]
    static class FloatMenuMakerMap_AddHumanLikeOrders_Postfix
    {
        [HarmonyPostfix]
        private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (SimpleSidearms.CEOverride)
                return;

            IntVec3 c = IntVec3.FromVector3(clickPos);
            if (pawn.equipment != null)
            {
                ThingWithComps equipment = null;
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i].TryGetComp<CompEquippable>() != null)
                    {
                        equipment = (ThingWithComps)thingList[i];
                        break;
                    }
                }
                if (equipment != null)
                {
                    string labelShort = equipment.LabelShort;
                    string errStr;
                    FloatMenuOption item3;
                    if (equipment.def.IsWeapon && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                    {
                    }
                    else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                    }
                    else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    {
                    }
                    else if (!equipment.def.IsWeapon)
                    {
                    }
                    else if (!StatCalculator.canCarrySidearm(equipment.def, pawn, out errStr))
                    {
                        "CannotEquip".Translate();
                        item3 = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + errStr + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                        opts.Add(item3);
                    }
                    else
                    {
                        string text2 = "Equip".Translate(labelShort) + "AsSidearm".Translate();

                        if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                        {
                            text2 = text2 + " " + "EquipWarningBrawler".Translate();
                        }

                        item3 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, delegate
                        {
                            equipment.SetForbidden(false, true);
                            pawn.jobs.TryTakeOrderedJob(new Job(SidearmsDefOf.EquipSecondary, equipment), JobTag.Misc);
                            MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f);
                            
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                        }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, equipment, "ReservedBy");
                        opts.Add(item3);
                    }
                }
            }
        }
    }
}
