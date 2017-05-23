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
    [HarmonyPatch(typeof(GizmoGridDrawer), "DrawGizmoGrid")]
    static class GizmoGridDrawer_DrawGizmoGrid_Prefix
    {
        [HarmonyPrefix]
        private static bool DrawGizmoGrid(ref IEnumerable<Gizmo> gizmos)
        {
            List<Gizmo> newList = new List<Gizmo>();
            newList.AddRange(gizmos);

            Pawn selected = GettersFilters.extractSelectedPawn();
            if (selected == null)
            {
                //Log.Message("no pawn");
            }
            else if (!selected.IsColonistPlayerControlled)
            {
                //Log.Message("pawn uncontrolled");
            }
            else
            { 
                if (selected.inventory != null)
                {
                    List<Thing> rangedWeapons;
                    List<Thing> meleeWeapons;
                    GettersFilters.getWeaponLists(out rangedWeapons, out meleeWeapons, selected.inventory);

                    if (rangedWeapons.Count > 0 || meleeWeapons.Count > 0)
                    {
                        Gizmo_SidearmsList advanced = new Gizmo_SidearmsList(selected, rangedWeapons, meleeWeapons);
                        advanced.defaultLabel = "DrawSidearm_gizmoTitle".Translate();
                        //draft.hotKey = KeyBindingDefOf.CommandColonistDraft;
                        advanced.defaultDesc = "DrawSidearm_gizmoTooltip".Translate();
                        newList.Add(advanced);
                    }
                }
            }
            gizmos = newList;

            return true;
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>) })]
    static class FloatMenuMakerMap_AddHumanLikeOrders_Postfix
    {
        [HarmonyPostfix]
        private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
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
                        item3 = new FloatMenuOption("CannotEquip".Translate(new object[]
                        {
                                labelShort
                        }) + " (" + errStr + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                        opts.Add(item3);
                    }
                    else
                    {
                        string text2 = "Equip".Translate(new object[]
                        {
                            labelShort
                        }) + "AsSidearm".Translate();

                        if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                        {
                            text2 = text2 + " " + "EquipWarningBrawler".Translate();
                        }

                        item3 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, delegate
                        {
                            equipment.SetForbidden(false, true);
                            pawn.jobs.TryTakeOrderedJob(new Job(SidearmsJobDefOf.EquipSecondary, equipment), JobTag.Misc);
                            MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f);
                        }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, equipment, "ReservedBy");
                        opts.Add(item3);
                    }
                }
            }
        }
    }
}
