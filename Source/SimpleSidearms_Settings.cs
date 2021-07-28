using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

using static PeteTimesSix.SimpleSidearms.Utilities.Enums;
using static PeteTimesSix.SimpleSidearms.UI.SidearmsSpecificExtensions;
using static PeteTimesSix.SimpleSidearms.UI.ListingExtensions;
using PeteTimesSix.SimpleSidearms.Utilities;
using SimpleSidearms.rimworld;
using System.Linq;
using RimWorld;

namespace PeteTimesSix.SimpleSidearms
{
    public static class InferredValues
    {
        public static float maxWeightMelee;
        public static float maxWeightRanged;
        public static float maxWeightTotal;
        public static float maxCapacity = 35f;

        public static void Init()
        {
            GettersFilters.getHeaviestWeapons(out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);
        }
    }

    public class SimpleSidearms_Settings : ModSettings
    {
        public OptionsTab ActiveTab = OptionsTab.Presets;
        public SettingsPreset ActivePreset = SettingsPreset.NoneApplied;

        public bool ToolAutoSwitch;
        public bool OptimalMelee;
        public bool CQCAutoSwitch;
        public bool CQCTargetOnly;
        public bool RangedCombatAutoSwitch;

        public float RangedCombatAutoSwitchMaxWarmup;

        public float SpeedSelectionBiasMelee;
        public float SpeedSelectionBiasRanged;

        public bool SeparateModes;
        public LimitModeSingleSidearm LimitModeSingle;
        public LimitModeAmountOfSidearms LimitModeAmount;

        public LimitModeSingleSidearm LimitModeSingleMelee;
        public LimitModeAmountOfSidearms LimitModeAmountMelee;
        public LimitModeSingleSidearm LimitModeSingleRanged;
        public LimitModeAmountOfSidearms LimitModeAmountRanged;
        public LimitModeAmountOfSidearms LimitModeAmountTotal;

        #region LimitModeSingle
        public float LimitModeSingle_AbsoluteMass;
        public float LimitModeSingle_RelativeMass;
        public HashSet<ThingDef> LimitModeSingle_Selection;

        private HashSet<ThingDef> LimitModeSingle_Match_Cache;
        #endregion
        #region LimitModeAmount
        public float LimitModeAmount_AbsoluteMass;
        public float LimitModeAmount_RelativeMass;
        public int LimitModeAmount_Slots;
        #endregion

        #region LimitModeSingleMelee
        public float LimitModeSingleMelee_AbsoluteMass;
        public float LimitModeSingleMelee_RelativeMass;
        public HashSet<ThingDef> LimitModeSingleMelee_Selection;

        private HashSet<ThingDef> LimitModeSingleMelee_Match_Cache;
        #endregion
        #region LimitModeAmountMelee
        public float LimitModeAmountMelee_AbsoluteMass;
        public float LimitModeAmountMelee_RelativeMass;
        public int LimitModeAmountMelee_Slots;
        #endregion
        #region LimitModeSingleRanged
        public float LimitModeSingleRanged_AbsoluteMass;
        public float LimitModeSingleRanged_RelativeMass;
        public HashSet<ThingDef> LimitModeSingleRanged_Selection;

        private HashSet<ThingDef> LimitModeSingleRanged_Match_Cache;
        #endregion
        #region LimitModeAmountRanged
        public float LimitModeAmountRanged_AbsoluteMass;
        public float LimitModeAmountRanged_RelativeMass;
        public int LimitModeAmountRanged_Slots;
        #endregion
        #region LimitModeAmountTotal
        public float LimitModeAmountTotal_AbsoluteMass;
        public float LimitModeAmountTotal_RelativeMass;
        public int LimitModeAmountTotal_Slots;
        #endregion

        public float SidearmSpawnChance;
        public float SidearmSpawnChanceDropoff;
        public float SidearmBudgetMultiplier;
        public float SidearmBudgetDropoff;

        public PrimaryWeaponMode ColonistDefaultWeaponMode;
        public PrimaryWeaponMode NPCDefaultWeaponMode;

        public DroppingModeOptionsEnum DropMode;
        public bool ReEquipOutOfCombat;
        public bool ReEquipBest;
        public bool ReEquipInCombat;

        public void StartupChecks()
        {
            if(ActivePreset == SettingsPreset.NoneApplied) 
            {
                
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ToolAutoSwitch, "ToolAutoSwitch");
            Scribe_Values.Look(ref OptimalMelee, "OptimalMelee");
            Scribe_Values.Look(ref CQCAutoSwitch, "CQCAutoSwitch");
            Scribe_Values.Look(ref CQCTargetOnly, "CQCTargetOnly");
            Scribe_Values.Look(ref RangedCombatAutoSwitch, "RangedCombatAutoSwitch");
            Scribe_Values.Look(ref RangedCombatAutoSwitchMaxWarmup, "RangedCombatAutoSwitchMaxWarmup");
            Scribe_Values.Look(ref SpeedSelectionBiasMelee, "SpeedSelectionBiasMelee");
            Scribe_Values.Look(ref SpeedSelectionBiasRanged, "SpeedSelectionBiasRanged");
            Scribe_Values.Look(ref SeparateModes, "SeparateModes");

            Scribe_Values.Look(ref LimitModeSingle, "LimitModeSingle");
            Scribe_Values.Look(ref LimitModeAmount, "LimitModeAmount");

            Scribe_Values.Look(ref LimitModeSingleMelee, "LimitModeSingleMelee");
            Scribe_Values.Look(ref LimitModeAmountMelee, "LimitModeAmountMelee");
            Scribe_Values.Look(ref LimitModeSingleRanged, "LimitModeSingleRanged");
            Scribe_Values.Look(ref LimitModeAmountRanged, "LimitModeAmountRanged");
            Scribe_Values.Look(ref LimitModeAmountTotal, "LimitModeAmountTotal");

            Scribe_Values.Look(ref LimitModeSingle_AbsoluteMass, "LimitModeSingle_AbsoluteMass");
            Scribe_Values.Look(ref LimitModeSingle_RelativeMass, "LimitModeSingle_RelativeMass");
            Scribe_Collections.Look(ref LimitModeSingle_Selection, "LimitModeSingle_Selection");

            Scribe_Values.Look(ref LimitModeAmount_AbsoluteMass, "LimitModeAmount_AbsoluteMass");
            Scribe_Values.Look(ref LimitModeAmount_RelativeMass, "LimitModeAmount_RelativeMass");
            Scribe_Values.Look(ref LimitModeAmount_Slots, "LimitModeAmount_Slots");

            Scribe_Values.Look(ref LimitModeSingleMelee_AbsoluteMass, "LimitModeSingleMelee_AbsoluteMass");
            Scribe_Values.Look(ref LimitModeSingleMelee_RelativeMass, "LimitModeSingleMelee_RelativeMass");
            Scribe_Collections.Look(ref LimitModeSingleMelee_Selection, "LimitModeSingleMelee_Selection");

            Scribe_Values.Look(ref LimitModeAmountMelee_AbsoluteMass, "LimitModeAmountMelee_AbsoluteMass");
            Scribe_Values.Look(ref LimitModeAmountMelee_RelativeMass, "LimitModeAmountMelee_RelativeMass");
            Scribe_Values.Look(ref LimitModeAmountMelee_Slots, "LimitModeAmountMelee_Slots");

            Scribe_Values.Look(ref LimitModeSingleRanged_AbsoluteMass, "LimitModeSingleRanged_AbsoluteMass");
            Scribe_Values.Look(ref LimitModeSingleRanged_RelativeMass, "LimitModeSingleRanged_RelativeMass");
            Scribe_Collections.Look(ref LimitModeSingleRanged_Selection, "LimitModeSingleRanged_Selection");

            Scribe_Values.Look(ref LimitModeAmountRanged_AbsoluteMass, "LimitModeAmountRanged_AbsoluteMass");
            Scribe_Values.Look(ref LimitModeAmountRanged_RelativeMass, "LimitModeAmountRanged_RelativeMass");
            Scribe_Values.Look(ref LimitModeAmountRanged_Slots, "LimitModeAmountRanged_Slots");

            Scribe_Values.Look(ref LimitModeAmountTotal_AbsoluteMass, "LimitModeAmountTotal_AbsoluteMass");
            Scribe_Values.Look(ref LimitModeAmountTotal_RelativeMass, "LimitModeAmountTotal_RelativeMass");
            Scribe_Values.Look(ref LimitModeAmountTotal_Slots, "LimitModeAmountTotal_Slots");

            Scribe_Values.Look(ref SidearmSpawnChance, "SidearmSpawnChance");
            Scribe_Values.Look(ref SidearmSpawnChanceDropoff, "SidearmSpawnChanceDropoff");
            Scribe_Values.Look(ref SidearmBudgetMultiplier, "SidearmBudgetMultiplier");
            Scribe_Values.Look(ref SidearmBudgetDropoff, "SidearmBudgetDropoff");

            Scribe_Values.Look(ref ColonistDefaultWeaponMode, "ColonistDefaultWeaponMode");
            Scribe_Values.Look(ref NPCDefaultWeaponMode, "NPCDefaultWeaponMode");

            Scribe_Values.Look(ref DropMode, "DropMode");
            Scribe_Values.Look(ref ReEquipOutOfCombat, "ReEquipOutOfCombat");
            Scribe_Values.Look(ref ReEquipBest, "ReEquipBest");
            Scribe_Values.Look(ref ReEquipInCombat, "ReEquipInCombat");
        }

        internal void DoSettingsWindowContents(Rect outerRect)
        {
            Color colorSave = GUI.color;
            TextAnchor anchorSave = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(outerRect);
            float maxWidth = listingStandard.ColumnWidth;

            //public enum OptionsTab { Presets, Automation, Allowances, Spawning, Misc }
            listingStandard.EnumSelector("ActiveTab_title".Translate(), ref ActiveTab, "ActiveTab_option_", valueTooltipPostfix: null, tooltip: "ActiveTab_desc".Translate());
            listingStandard.GapLine();


            switch (ActiveTab) 
            {
                case OptionsTab.Presets:
                    
                    break;
                case OptionsTab.Automation:
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.CheckboxLabeled("ToolAutoSwitch_title".Translate(), ref ToolAutoSwitch, "ToolAutoSwitch_desc".Translate());
                        subsection.CheckboxLabeled("OptimalMelee_title".Translate(), ref OptimalMelee, "OptimalMelee_desc".Translate());
                        subsection.CheckboxLabeled("CQCAutoSwitch_title".Translate(), ref CQCAutoSwitch, "CQCAutoSwitch_desc".Translate());
                        subsection.CheckboxLabeled("CQCTargetOnly_title".Translate(), ref CQCTargetOnly, "CQCTargetOnly_desc".Translate());

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.CheckboxLabeled("RangedCombatAutoSwitch_title".Translate(), ref RangedCombatAutoSwitch, "RangedCombatAutoSwitch_desc".Translate());
                        if (RangedCombatAutoSwitch)
                        {
                            subsection.SliderLabeled("RangedCombatAutoSwitchMaxWarmup_title".Translate(), ref RangedCombatAutoSwitchMaxWarmup, 0, 1, displayMult: 100, valueSuffix: "%");
                        }

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.SliderSpeedBias("SpeedSelectionBiasMelee_title".Translate(), ref SpeedSelectionBiasMelee, 0.5f, 1f, 2f, true, displayMult: 100, valueSuffix: "%");

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.SliderSpeedBias("SpeedSelectionBiasRanged_title".Translate(), ref SpeedSelectionBiasRanged, 0.5f, 1f, 2f, false, displayMult: 100, valueSuffix: "%");

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    break;
                case OptionsTab.Allowances:
                    {
                        var valBefore = SeparateModes;
                        listingStandard.CheckboxLabeled("SeparateModes_title".Translate(), ref SeparateModes, "SeparateModes_desc".Translate());
                        if (valBefore != SeparateModes)
                        {
                            LimitModeSingle_Match_Cache = null;
                            LimitModeSingleMelee_Match_Cache = null;
                            LimitModeSingleRanged_Match_Cache = null;
                        }
                    }
                    if (!SeparateModes)
                    {
                        Limits(listingStandard, WeaponListKind.Both);
                    }
                    else
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        Limits(subsection, WeaponListKind.Melee);

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        Limits(subsection, WeaponListKind.Ranged);

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                        listingStandard.EnumSelector("LimitModeAmountTotal_title".Translate(), ref LimitModeAmountTotal, "LimitModeAmount_option_", valueTooltipPostfix: null, tooltip: "LimitModeAmountTotal_desc".Translate());
                        switch (LimitModeAmountTotal)
                        {
                            case LimitModeAmountOfSidearms.AbsoluteWeight:
                                listingStandard.SliderLabeled("MaximumMassAmountAbsolute_title".Translate(), ref LimitModeAmountTotal_AbsoluteMass, 0, InferredValues.maxCapacity, valueSuffix: " kg");
                                break;
                            case LimitModeAmountOfSidearms.RelativeWeight:
                                listingStandard.SliderLabeled("MaximumMassAmountRelative_title".Translate(), ref LimitModeAmountTotal_RelativeMass, 0, 1, displayMult: 100, valueSuffix: "%");
                                break;
                            case LimitModeAmountOfSidearms.Slots:
                                listingStandard.Spinner("MaximumSlots_title".Translate(), ref LimitModeAmountTotal_Slots, min: 1, tooltip: "MaximumSlots_desc".Translate());
                                break;
                            case LimitModeAmountOfSidearms.None:
                                break;
                        }
                    }
                    Color save = GUI.color;
                    GUI.color = Color.gray;
                    listingStandard.Label("LimitCarryInfo_title".Translate());
                    GUI.color = save;
                    
                    break;
                case OptionsTab.Spawning:
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.SliderLabeled("SidearmSpawnChance_title".Translate(), ref SidearmSpawnChance, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmSpawnChance_desc".Translate());
                        subsection.SliderLabeled("SidearmSpawnChanceDropoff_title".Translate(), ref SidearmSpawnChanceDropoff, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmSpawnChanceDropoff_desc".Translate());

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.SliderLabeled("SidearmBudgetMultiplier_title".Translate(), ref SidearmBudgetMultiplier, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmBudgetMultiplier_desc".Translate());
                        subsection.SliderLabeled("SidearmBudgetDropoff_title".Translate(), ref SidearmBudgetDropoff, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmBudgetDropoff_desc".Translate());

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    break;
                case OptionsTab.Misc:
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.EnumSelector("ColonistDefaultWeaponMode_title".Translate(), ref ColonistDefaultWeaponMode, "PrimaryWeaponMode_option_", valueTooltipPostfix: null, tooltip: "ColonistDefaultWeaponMode_desc".Translate());
                        subsection.EnumSelector("NPCDefaultWeaponMode_title".Translate(), ref NPCDefaultWeaponMode, "PrimaryWeaponMode_option_", valueTooltipPostfix: null, tooltip: "NPCDefaultWeaponMode_desc".Translate());

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.EnumSelector("DropMode_title".Translate(), ref DropMode, "DropMode_option_", valueTooltipPostfix: null, tooltip: "DropMode_desc".Translate());
                        subsection.CheckboxLabeled("ReEquipOutOfCombat_title".Translate(), ref ReEquipOutOfCombat, "ReEquipOutOfCombat_desc".Translate());
                        if (ReEquipOutOfCombat)
                        {
                            subsection.CheckboxLabeled("ReEquipBest_title".Translate(), ref ReEquipBest, "ReEquipBest_desc".Translate());
                            subsection.CheckboxLabeled("ReEquipInCombat_title".Translate(), ref ReEquipInCombat, "ReEquipInCombat_desc".Translate());
                        }

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    break;
            }

            listingStandard.End();


            GUI.color = colorSave;
            Text.Anchor = anchorSave;
        }

        private void Limits(Listing_Standard listingStandard, WeaponListKind listType)
        {

            ref var limitModeSingle = ref LimitModeSingle;
            ref var limitModeSingle_Match_Cache = ref LimitModeSingle_Match_Cache;
            ref var limitModeSingle_AbsoluteMass = ref LimitModeSingle_AbsoluteMass;
            ref var limitModeSingle_RelativeMass = ref LimitModeSingle_RelativeMass;
            ref var limitModeAmount = ref LimitModeAmount;
            ref var limitModeAmount_AbsoluteMass = ref LimitModeAmount_AbsoluteMass;
            ref var limitModeAmount_RelativeMass = ref LimitModeAmount_RelativeMass;
            ref var limitModeAmount_Slots = ref LimitModeAmount_Slots;

            var limitModeSingleLabel = "LimitModeSingle_title";
            var limitModeSingleTooltip = "LimitModeSingle_desc";
            var limitModeAmountLabel = "LimitModeAmount_title";
            var limitModeAmountTooltip = "LimitModeAmount_desc";

            switch (listType)
            {
                case WeaponListKind.Both:
                    break;
                case WeaponListKind.Melee:
                    limitModeSingle = ref LimitModeSingleMelee;
                    limitModeSingle_Match_Cache = ref LimitModeSingleMelee_Match_Cache;
                    limitModeSingle_AbsoluteMass = ref LimitModeSingleMelee_AbsoluteMass;
                    limitModeSingle_RelativeMass = ref LimitModeSingleMelee_RelativeMass;
                    limitModeAmount = ref LimitModeAmountMelee;
                    limitModeAmount_AbsoluteMass = ref LimitModeAmountMelee_AbsoluteMass;
                    limitModeAmount_RelativeMass = ref LimitModeAmountMelee_RelativeMass;
                    limitModeAmount_Slots = ref LimitModeAmountMelee_Slots;

                    limitModeSingleLabel = "LimitModeSingleMelee_title";
                    limitModeSingleTooltip = "LimitModeSingleMelee_desc";
                    limitModeAmountLabel = "LimitModeAmountMelee_title";
                    limitModeAmountTooltip = "LimitModeAmountMelee_desc";
                    break;
                case WeaponListKind.Ranged:
                    limitModeSingle = ref LimitModeSingleRanged;
                    limitModeSingle_Match_Cache = ref LimitModeSingleRanged_Match_Cache;
                    limitModeSingle_AbsoluteMass = ref LimitModeSingleRanged_AbsoluteMass;
                    limitModeSingle_RelativeMass = ref LimitModeSingleRanged_RelativeMass;
                    limitModeAmount = ref LimitModeAmountRanged;
                    limitModeAmount_AbsoluteMass = ref LimitModeAmountRanged_AbsoluteMass;
                    limitModeAmount_RelativeMass = ref LimitModeAmountRanged_RelativeMass;
                    limitModeAmount_Slots = ref LimitModeAmountRanged_Slots;

                    limitModeSingleLabel = "LimitModeSingleRanged_title";
                    limitModeSingleTooltip = "LimitModeSingleRanged_desc";
                    limitModeAmountLabel = "LimitModeAmountRanged_title";
                    limitModeAmountTooltip = "LimitModeAmountRanged_desc";
                    break;
                default:
                    throw new ArgumentException();
            }

            {
                var valBefore = limitModeSingle;
                listingStandard.EnumSelector(limitModeSingleLabel.Translate(), ref limitModeSingle, "LimitModeSingle_option_", valueTooltipPostfix: null, tooltip: limitModeSingleTooltip.Translate());
                if (valBefore != limitModeSingle)
                    limitModeSingle_Match_Cache = null;
            }
            switch (limitModeSingle)
            {
                case LimitModeSingleSidearm.AbsoluteWeight:
                    {
                        var valBefore = limitModeSingle_AbsoluteMass;
                        listingStandard.SliderLabeled("MaximumMassSingleAbsolute_title".Translate(), ref limitModeSingle_AbsoluteMass, 0, InferredValues.maxWeightTotal, valueSuffix: " kg");
                        if (valBefore != limitModeSingle_AbsoluteMass || limitModeSingle_Match_Cache == null)
                            RebuildCache(ref limitModeSingle_Match_Cache, listType);
                        listingStandard.WeaponList(ref limitModeSingle_Match_Cache, listType);
                    }
                    break;
                case LimitModeSingleSidearm.RelativeWeight:
                    {
                        var valBefore = limitModeSingle_RelativeMass;
                        listingStandard.SliderLabeled("MaximumMassSingleRelative_title".Translate(), ref limitModeSingle_RelativeMass, 0, 1, displayMult: 100, valueSuffix: "%");
                        if (valBefore != limitModeSingle_RelativeMass || limitModeSingle_Match_Cache == null)
                            RebuildCache(ref limitModeSingle_Match_Cache, listType);
                        Color save = GUI.color;
                        GUI.color = Color.gray;
                        listingStandard.Label("MaximumMassAmountRelative_hint".Translate());
                        GUI.color = save;
                        listingStandard.WeaponList(ref limitModeSingle_Match_Cache, listType);
                    }
                    break;
                case LimitModeSingleSidearm.Selection:
                    //sublisting.WeaponSelector(ref LimitModeSingle_Selection, WeaponListKind.Both);
                    break;
                case LimitModeSingleSidearm.None:
                    break;
            }
            listingStandard.EnumSelector(limitModeAmountLabel.Translate(), ref limitModeAmount, "LimitModeAmount_option_", valueTooltipPostfix: null, tooltip: limitModeAmountTooltip.Translate());
            switch (limitModeAmount)
            {
                case LimitModeAmountOfSidearms.AbsoluteWeight:
                    listingStandard.SliderLabeled("MaximumMassAmountAbsolute_title".Translate(), ref limitModeAmount_AbsoluteMass, 0, InferredValues.maxCapacity, valueSuffix: " kg");
                    break;
                case LimitModeAmountOfSidearms.RelativeWeight:
                    listingStandard.SliderLabeled("MaximumMassAmountRelative_title".Translate(), ref limitModeAmount_RelativeMass, 0, 1, displayMult: 100, valueSuffix: "%");
                    break;
                case LimitModeAmountOfSidearms.Slots:
                    listingStandard.Spinner("MaximumSlots_title".Translate(), ref limitModeAmount_Slots, min: 1, tooltip: "MaximumSlots_desc".Translate());
                    break;
                case LimitModeAmountOfSidearms.None:
                    break;
            }
        }

        private void RebuildCache(ref HashSet<ThingDef> cache, WeaponListKind listType)
        {
            IEnumerable<ThingDefStuffDefPair> validSidearms = GettersFilters.getValidWeapons();
            List<ThingDef> matchingSidearms = GettersFilters.filterForWeaponKind(validSidearms, MiscUtils.LimitTypeToListType(listType)).Select(w => w.thing).ToList();

            LimitModeSingleSidearm limitMode;
            float limitModeSingle_AbsoluteMass;
            float limitModeSingle_RelativeMass;
            switch (listType) 
            {
                case WeaponListKind.Both:
                    limitMode = LimitModeSingle;
                    limitModeSingle_AbsoluteMass = LimitModeSingle_AbsoluteMass;
                    limitModeSingle_RelativeMass = LimitModeSingle_RelativeMass;
                    break;
                case WeaponListKind.Melee:
                    limitMode = LimitModeSingleMelee;
                    limitModeSingle_AbsoluteMass = LimitModeSingleMelee_AbsoluteMass;
                    limitModeSingle_RelativeMass = LimitModeSingleMelee_RelativeMass;
                    break;
                case WeaponListKind.Ranged:
                    limitMode = LimitModeSingleRanged;
                    limitModeSingle_AbsoluteMass = LimitModeSingleRanged_AbsoluteMass;
                    limitModeSingle_RelativeMass = LimitModeSingleRanged_RelativeMass;
                    break;
                default:
                    throw new ArgumentException();
            }

            switch (limitMode)
            {
                case LimitModeSingleSidearm.AbsoluteWeight:
                    matchingSidearms = matchingSidearms.Where(w => w.GetStatValueAbstract(StatDefOf.Mass) <= limitModeSingle_AbsoluteMass).OrderBy(t => t.GetStatValueAbstract(StatDefOf.Mass)).ToList();
                    break;
                case LimitModeSingleSidearm.RelativeWeight:
                    matchingSidearms = matchingSidearms.Where(w => w.GetStatValueAbstract(StatDefOf.Mass) <= limitModeSingle_RelativeMass * InferredValues.maxCapacity).OrderBy(t => t.GetStatValueAbstract(StatDefOf.Mass)).ToList();
                    break;
                case LimitModeSingleSidearm.Selection:
                    matchingSidearms = LimitModeSingle_Selection.ToList();
                    break;
                case LimitModeSingleSidearm.None:
                    break;
            }

            cache = matchingSidearms.ToHashSet();
        }
    }
}