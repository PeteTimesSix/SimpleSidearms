using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

using static PeteTimesSix.SimpleSidearms.Utilities.Enums;
using static PeteTimesSix.SimpleSidearms.UI.SidearmsSpecificExtensions;
using static PeteTimesSix.SimpleSidearms.UI.ListingExtensions;
using PeteTimesSix.SimpleSidearms.Utilities;

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
        public float LimitModeSingle_RelativeMass;
        public float LimitModeSingle_AbsoluteMass;
        public HashSet<ThingDef> LimitModeSingle_Selection;

        private HashSet<ThingDef> LimitModeSingle_Match_Cache;
        #endregion
        #region LimitModeAmount
        public float LimitModeAmount_RelativeMass;
        public float LimitModeAmount_AbsoluteMass;
        public int LimitModeAmount_Slots;
        #endregion

        #region LimitModeSingleMelee
        public float LimitModeSingleMelee_RelativeMass;
        public float LimitModeSingleMelee_AbsoluteMass;
        public HashSet<ThingDef> LimitModeSingleMelee_Selection;

        private HashSet<ThingDef> LimitModeSingleMelee_Match_Cache;
        #endregion
        #region LimitModeAmountMelee
        public float LimitModeAmountMelee_RelativeMass;
        public float LimitModeAmountMelee_AbsoluteMass;
        public int LimitModeAmountMelee_Slots;
        #endregion
        #region LimitModeSingleRanged
        public float LimitModeSingleRanged_RelativeMass;
        public float LimitModeSingleRanged_AbsoluteMass;
        public HashSet<ThingDef> LimitModeSingleRanged_Selection;

        private HashSet<ThingDef> LimitModeSingleRanged_Match_Cache;
        #endregion
        #region LimitModeAmountRanged
        public float LimitModeAmountRanged_RelativeMass;
        public float LimitModeAmountRanged_AbsoluteMass;
        public int LimitModeAmountRanged_Slots;
        #endregion
        #region LimitModeAmountTotal
        public float LimitModeAmountTotal_RelativeMass;
        public float LimitModeAmountTotal_AbsoluteMass;
        public int LimitModeAmountTotal_Slots;
        #endregion

        public float SidearmSpawnChance;
        public float SidearmSpawnChanceDropoff;
        public float SidearmBudgetMultiplier;
        public float SidearmBudgetDropoff;

        public PrimaryWeaponMode ColonistDefaultWeaponMode;
        public PrimaryWeaponMode NPCDefaultWeaponMode;

        public DroppingModeOptionsEnum DropMode;
        public bool ReEquipBest;

        public void StartupChecks()
        {
            if(ActivePreset == SettingsPreset.NoneApplied) 
            {
                
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref LimitModeSingle_Selection, "LimitModeSingle_Selection", LookMode.Def);
        }

        internal void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            float maxWidth = listingStandard.ColumnWidth;

            {
                var togglesSection = listingStandard.BeginHiddenSection(out float togglesSectionHeight);
                togglesSection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                togglesSection.CheckboxLabeled("ToolAutoSwitch_title".Translate(), ref ToolAutoSwitch, "ToolAutoSwitch_desc".Translate());
                togglesSection.CheckboxLabeled("OptimalMelee_title".Translate(), ref OptimalMelee, "OptimalMelee_desc".Translate());
                togglesSection.CheckboxLabeled("CQCAutoSwitch_title".Translate(), ref CQCAutoSwitch, "CQCAutoSwitch_desc".Translate());
                togglesSection.CheckboxLabeled("CQCTargetOnly_title".Translate(), ref CQCTargetOnly, "CQCTargetOnly_desc".Translate());
                togglesSection.NewHiddenColumn(ref togglesSectionHeight);
                togglesSection.CheckboxLabeled("RangedCombatAutoSwitch_title".Translate(), ref RangedCombatAutoSwitch, "RangedCombatAutoSwitch_desc".Translate());
                if (RangedCombatAutoSwitch)
                {
                    togglesSection.SliderLabeled("RangedCombatAutoSwitchMaxWarmup_title".Translate(), ref RangedCombatAutoSwitchMaxWarmup, 0, 1, displayMult: 100, valueSuffix: "%");
                }
                listingStandard.EndHiddenSection(togglesSection, togglesSectionHeight);
            }

            {
                var speedBiasSection = listingStandard.BeginHiddenSection(out float speedBiasSectionHeight);
                speedBiasSection.ColumnWidth = (maxWidth - ColumnGap) / 2;
                speedBiasSection.SliderSpeedBias("SpeedSelectionBiasMelee_title".Translate(), ref SpeedSelectionBiasMelee, 0.5f, 1f, 2f, true, displayMult: 100, valueSuffix: "%");
                speedBiasSection.NewHiddenColumn(ref speedBiasSectionHeight);
                speedBiasSection.SliderSpeedBias("SpeedSelectionBiasRanged_title".Translate(), ref SpeedSelectionBiasRanged, 0.5f, 1f, 2f, false, displayMult: 100, valueSuffix: "%");
                listingStandard.EndHiddenSection(speedBiasSection, speedBiasSectionHeight);
            }

            listingStandard.GapLine();

            listingStandard.CheckboxLabeled("SeparateModes_title".Translate(), ref SeparateModes, "SeparateModes_desc".Translate());
            if (!SeparateModes)
            {
                listingStandard.EnumSelector("LimitModeSingle_title".Translate(), ref LimitModeSingle, "LimitModeSingle_option_", valueTooltipPostfix: null, tooltip: "LimitModeSingle_desc".Translate());
                switch (LimitModeSingle)
                {
                    case LimitModeSingleSidearm.AbsoluteWeight:
                        listingStandard.SliderLabeled("MaximumMassSingleAbsolute_title".Translate(), ref LimitModeSingle_AbsoluteMass, 0, InferredValues.maxWeightTotal, valueSuffix: " kg");
                        //sublisting.WeaponList(ref LimitModeSingle_Match_Cache, WeaponListKind.Both);
                        break;
                    case LimitModeSingleSidearm.RelativeWeight:
                        listingStandard.SliderLabeled("MaximumMassSingleRelative_title".Translate(), ref LimitModeSingle_RelativeMass, 0, 1, valueSuffix: "%");
                        //sublisting.WeaponList(ref LimitModeSingle_Match_Cache, WeaponListKind.Both);
                        break;
                    case LimitModeSingleSidearm.Selection:
                        //sublisting.WeaponSelector(ref LimitModeSingle_Selection, WeaponListKind.Both);
                        break;
                    case LimitModeSingleSidearm.None:
                        break;
                }
                listingStandard.EnumSelector("LimitModeAmount_title".Translate(), ref LimitModeAmount, "LimitModeAmount_option_", valueTooltipPostfix: null, tooltip: "LimitModeAmount_desc".Translate());
                switch (LimitModeAmount)
                {
                    case LimitModeAmountOfSidearms.AbsoluteWeight:
                        listingStandard.SliderLabeled("MaximumMassAmountAbsolute_title".Translate(), ref LimitModeAmount_AbsoluteMass, 0, InferredValues.maxCapacity, valueSuffix: " kg");
                        break;
                    case LimitModeAmountOfSidearms.RelativeWeight:
                        listingStandard.SliderLabeled("MaximumMassAmountRelative_title".Translate(), ref LimitModeAmount_RelativeMass, 0, 1, valueSuffix: "%");
                        break;
                    case LimitModeAmountOfSidearms.Slots:
                        listingStandard.Spinner("MaximumSlots_title".Translate(), ref LimitModeAmount_Slots, min: 1, tooltip: "MaximumSlots_desc".Translate());
                        break;
                    case LimitModeAmountOfSidearms.None:
                        break;
                }
            }
            else 
            {

                var limitModesSection = listingStandard.BeginHiddenSection(out float limitModesSectionHeight);
                limitModesSection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                var sublisting = limitModesSection;
                {//melee 
                    sublisting.EnumSelector("LimitModeSingleMelee_title".Translate(), ref LimitModeSingleMelee, "LimitModeSingle_option_", valueTooltipPostfix: null, tooltip: "LimitModeSingleMelee_desc".Translate());
                    switch (LimitModeSingleMelee)
                    {
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            sublisting.SliderLabeled("MaximumMassSingleAbsolute_title".Translate(), ref LimitModeSingleMelee_AbsoluteMass, 0, InferredValues.maxWeightTotal, valueSuffix: " kg");
                            //sublisting.WeaponList(ref LimitModeSingleMelee_Match_Cache, WeaponListKind.Melee);
                            break;
                        case LimitModeSingleSidearm.RelativeWeight:
                            sublisting.SliderLabeled("MaximumMassSingleRelative_title".Translate(), ref LimitModeSingleMelee_RelativeMass, 0, 1, valueSuffix: "%");
                            //sublisting.WeaponList(ref LimitModeSingleMelee_Match_Cache, WeaponListKind.Melee);
                            break;
                        case LimitModeSingleSidearm.Selection:
                            //sublisting.WeaponSelector(ref LimitModeSingleMelee_Selection, WeaponListKind.Melee);
                            break;
                        case LimitModeSingleSidearm.None:
                            break;
                    }
                    sublisting.EnumSelector("LimitModeAmountMelee_title".Translate(), ref LimitModeAmountMelee, "LimitModeAmount_option_", valueTooltipPostfix: null, tooltip: "LimitModeAmountMelee_desc".Translate());
                    switch (LimitModeAmountMelee)
                    {
                        case LimitModeAmountOfSidearms.AbsoluteWeight:
                            sublisting.SliderLabeled("MaximumMassAmountAbsolute_title".Translate(), ref LimitModeAmountMelee_AbsoluteMass, 0, InferredValues.maxCapacity, valueSuffix: " kg");
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            sublisting.SliderLabeled("MaximumMassAmountRelative_title".Translate(), ref LimitModeAmountMelee_RelativeMass, 0, 1, valueSuffix: "%");
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            sublisting.Spinner("MaximumSlots_title".Translate(), ref LimitModeAmountMelee_Slots, min: 1, tooltip: "MaximumSlots_desc".Translate());
                            break;
                        case LimitModeAmountOfSidearms.None:
                            break;
                    }
                }

                limitModesSection.NewHiddenColumn(ref limitModesSectionHeight);

                {//ranged 
                    sublisting.EnumSelector("LimitModeSingleRanged_title".Translate(), ref LimitModeSingleRanged, "LimitModeSingle_option_", valueTooltipPostfix: null, tooltip: "LimitModeSingleRanged_desc".Translate());
                    switch (LimitModeSingleRanged)
                    {
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            sublisting.SliderLabeled("MaximumMassSingleAbsolute_title".Translate(), ref LimitModeSingleRanged_AbsoluteMass, 0, InferredValues.maxWeightTotal, valueSuffix: " kg");
                            //sublisting.WeaponList(ref LimitModeSingleRanged_Match_Cache, WeaponListKind.Ranged);
                            break;
                        case LimitModeSingleSidearm.RelativeWeight:
                            sublisting.SliderLabeled("MaximumMassSingleRelative_title".Translate(), ref LimitModeSingleRanged_RelativeMass, 0, 1, valueSuffix: "%");
                            //sublisting.WeaponList(ref LimitModeSingleRanged_Match_Cache, WeaponListKind.Ranged);
                            break;
                        case LimitModeSingleSidearm.Selection:
                            //sublisting.WeaponSelector(ref LimitModeSingleRanged_Selection, WeaponListKind.Ranged);
                            break;
                        case LimitModeSingleSidearm.None:
                            break;
                    }
                    sublisting.EnumSelector("LimitModeAmountRanged_title".Translate(), ref LimitModeAmountRanged, "LimitModeAmount_option_", valueTooltipPostfix: null, tooltip: "LimitModeAmountRanged_desc".Translate());
                    switch (LimitModeAmountRanged)
                    {
                        case LimitModeAmountOfSidearms.AbsoluteWeight:
                            sublisting.SliderLabeled("MaximumMassAmountAbsolute_title".Translate(), ref LimitModeAmountRanged_AbsoluteMass, 0, InferredValues.maxCapacity, valueSuffix: " kg");
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            sublisting.SliderLabeled("MaximumMassAmountRelative_title".Translate(), ref LimitModeAmountRanged_RelativeMass, 0, 1, valueSuffix: "%");
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            sublisting.Spinner("MaximumSlots_title".Translate(), ref LimitModeAmountRanged_Slots, min: 1, tooltip: "MaximumSlots_desc".Translate());
                            break;
                        case LimitModeAmountOfSidearms.None:
                            break;
                    }
                }
                listingStandard.EndHiddenSection(limitModesSection, limitModesSectionHeight);
            }
            listingStandard.Label("LimitCarryInfo_title".Translate());

            listingStandard.End();
        }
    }
}