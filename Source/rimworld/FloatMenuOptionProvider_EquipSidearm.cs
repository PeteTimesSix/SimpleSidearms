using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using SimpleSidearms.rimworld;
using PeteTimesSix.SimpleSidearms.Utilities;

namespace PeteTimesSix.SimpleSidearms
{
    public class FloatMenuOptionProvider_EquipSidearm : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            return context.FirstSelectedPawn.equipment != null;
        }

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            var pawn = context.FirstSelectedPawn;
            if (!clickedThing.HasComp<CompEquippable>())
            {
                return null;
            }

            var eqLab = clickedThing.LabelShort;
            bool toolUse = ((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0) || clickedThing.toThingDefStuffDefPair().isToolNotWeapon();
            string textPostfix = toolUse ? "AsTool".Translate() : "AsSidearm".Translate();

            /*if (clickedThing.def.IsWeapon && context.FirstSelectedPawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return new FloatMenuOption("CannotEquip".Translate(eqLab) + ": " + "IsIncapableOfViolenceLower".Translate(context.FirstSelectedPawn.LabelShort, context.FirstSelectedPawn), null);
            }*/
            if (clickedThing.def.IsRangedWeapon && context.FirstSelectedPawn.WorkTagIsDisabled(WorkTags.Shooting))
            {
                return null;
            }
            if (!context.FirstSelectedPawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return null;
            }
            if (!context.FirstSelectedPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }
            if (clickedThing.IsBurning())
            {
                return null;
            }
            if (context.FirstSelectedPawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(clickedThing, context.FirstSelectedPawn))
            {
                return null;
            }
            if (!EquipmentUtility.CanEquip(clickedThing, context.FirstSelectedPawn, out var cantReason, checkBonded: false))
            {
                return null;
            }

            if (!StatCalculator.CanPickupSidearmInstance(clickedThing as ThingWithComps, pawn, out string errStr))
            {
                string orderText = "CannotEquip".Translate(eqLab) + textPostfix;

                var order = new FloatMenuOption(orderText + ": " + errStr, null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return order;
            }
            else
            {
                string orderText = "Equip".Translate(clickedThing.LabelShort) + textPostfix;

                if (clickedThing.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                {
                    orderText = orderText + " " + "EquipWarningBrawler".Translate();
                }
                if (EquipmentUtility.AlreadyBondedToWeapon(clickedThing, context.FirstSelectedPawn))
                {
                    orderText += " " + "BladelinkAlreadyBonded".Translate();
                    TaggedString dialogText = "BladelinkAlreadyBondedDialog".Translate(context.FirstSelectedPawn.Named("PAWN"), clickedThing.Named("WEAPON"), context.FirstSelectedPawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
                    return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(orderText, () =>
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox(dialogText));
                    }, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
                }
                return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(orderText, () =>
                {
                    string personaWeaponConfirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText(clickedThing, context.FirstSelectedPawn);
                    if (!personaWeaponConfirmationText.NullOrEmpty())
                    {
                        Find.WindowStack.Add(new Dialog_MessageBox(personaWeaponConfirmationText, "Yes".Translate(), delegate
                        {
                            Equip();
                        }, "No".Translate()));
                    }
                    else
                    {
                        Equip();
                    }
                }, MenuOptionPriority.High), context.FirstSelectedPawn, clickedThing);
            }

            void Equip()
            {
                clickedThing.SetForbidden(false, true);
                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, clickedThing), JobTag.Misc);
                FleckMaker.Static(clickedThing.DrawPos, clickedThing.MapHeld, FleckDefOf.FeedbackEquip);
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
            }
        }
    }
}

/*
 [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>) })]
    public static class FloatMenuMakerMap_AddHumanLikeOrders_Postfix
    {
        [HarmonyPostfix]
        public static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            try
            {
                if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    return;
                if (pawn.IsQuestLodger())
                    return;

                IntVec3 c = IntVec3.FromVector3(clickPos);

                if (!pawn.CanReach(new LocalTargetInfo(c), PathEndMode.ClosestTouch, Danger.Deadly))
                    return;

                if (pawn.equipment != null)
                {
                    foreach (var thing in c.GetThingList(pawn.Map))
                    {
                        var thingWithComps = thing as ThingWithComps;
                        if (thingWithComps == null)
                            continue;
                        if (!thingWithComps.def.IsWeapon)
                            continue;
                        if (thingWithComps.IsBurning())
                            continue;

                        bool toolUse = ((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0) || thingWithComps.toThingDefStuffDefPair().isToolNotWeapon();
                        string textPostfix = toolUse ? "AsTool".Translate() : "AsSidearm".Translate();
                        if (!StatCalculator.CanPickupSidearmInstance(thingWithComps, pawn, out string errStr))
                        {
                            string orderText = "CannotEquip".Translate(thingWithComps.LabelShort) + textPostfix;

                            var order = new FloatMenuOption(orderText + ": " + errStr, null, MenuOptionPriority.Default, null, null, 0f, null, null);
                            opts.Add(order);
                        }
                        else
                        {
                            string orderText = "Equip".Translate(thingWithComps.LabelShort) + textPostfix;

                            if (thingWithComps.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                            {
                                orderText = orderText + " " + "EquipWarningBrawler".Translate();
                            }

                            var order = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(orderText, delegate
                            {
                                thingWithComps.SetForbidden(false, true);
                                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(SidearmsDefOf.EquipSecondary, thingWithComps), JobTag.Misc);
                                //MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, ThingDefOf.Mote_FeedbackEquip, 1f); //why is this gone?

                                PlayerKnowledgeDatabase.KnowledgeDemonstrated(SidearmsDefOf.Concept_SimpleSidearmsBasic, KnowledgeAmount.SmallInteraction);
                            }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, thingWithComps, "ReservedBy");
                            opts.Add(order);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception during SimpleSidearms floatmenumaker intercept. Cancelling intercept. Exception: " + e.ToString());
            }
        }
    }
 */