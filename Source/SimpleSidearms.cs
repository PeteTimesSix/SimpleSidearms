using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using SimpleSidearms.hugsLibSettings;
using SimpleSidearms.rimworld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static SimpleSidearms.Globals;
using static SimpleSidearms.hugsLibSettings.SettingsUIs;

namespace SimpleSidearms
{
    public class SimpleSidearms : HugsLib.ModBase
    {
        public override string ModIdentifier { get { return "PeteTimesSix.SimpleSidearms"; } }
        
        public static SimpleSidearmsConfigData configData;

        public enum OptionsTab {Presets, Automation, Allowances, Spawning, Misc}
        public float TabsNegativeOffset = 100f;

        public static SettingHandle<OptionsTab> ActiveTab;

        public static SettingHandle<Preset> ActivePreset;

        public static SettingHandle<Preset> PresetCustom;
        public static SettingHandle<Preset> Preset0;
        public static SettingHandle<Preset> Preset1;
        public static SettingHandle<Preset> Preset1half;
        public static SettingHandle<Preset> Preset2;
        public static SettingHandle<Preset> Preset3;
        public static SettingHandle<Preset> Preset4;
        public static SettingHandle<Preset> Preset5;
        public static SettingHandle<bool> PresetExport;

        public static SettingHandle<bool> ToolAutoSwitch;
        public static SettingHandle<bool> OptimalMelee;
        public static SettingHandle<bool> CQCAutoSwitch;
        public static SettingHandle<bool> CQCTargetOnly;
        //public static SettingHandle<bool> Underline0;
        public static SettingHandle<bool> RangedCombatAutoSwitch;
        public static SettingHandle<float> RangedCombatAutoSwitchMaxWarmup;

        public static SettingHandle<float> SpeedSelectionBiasMelee;
        public static SettingHandle<float> SpeedSelectionBiasRanged;

        public static SettingHandle<bool> LimitCarryInfo;
        public static SettingHandle<bool> CEOverrideInfo;

        public static SettingHandle<bool> SeparateModes;
        public static SettingHandle<LimitModeSingleSidearm> LimitModeSingle;
        public static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmount;

        public static SettingHandle<LimitModeSingleSidearm> LimitModeSingleMelee;
        public static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmountMelee;
        //public static SettingHandle<bool> Underline1;
        public static SettingHandle<LimitModeSingleSidearm> LimitModeSingleRanged;
        public static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmountRanged;
        //public static SettingHandle<bool> Underline2;
        public static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmountTotal;

        #region LimitModeSingle
        public static SettingHandle<float> LimitModeSingle_Relative;
        public static SettingHandle<WeaponListKind> LimitModeSingle_RelativeMatches;
        public static SettingHandle<float> LimitModeSingle_Absolute;
        public static SettingHandle<WeaponListKind> LimitModeSingle_AbsoluteMatches;
        public static SettingHandle<ThingDefHashSetHandler> LimitModeSingle_Selection;
        #endregion
        #region LimitModeAmount
        public static SettingHandle<float> LimitModeAmount_Relative;
        public static SettingHandle<float> LimitModeAmount_Absolute;
        public static SettingHandle<int> LimitModeAmount_Slots;
        #endregion

        #region LimitModeSingleMelee
        public static SettingHandle<float> LimitModeSingleMelee_Relative;
        public static SettingHandle<WeaponListKind> LimitModeSingleMelee_RelativeMatches;
        public static SettingHandle<float> LimitModeSingleMelee_Absolute;
        public static SettingHandle<WeaponListKind> LimitModeSingleMelee_AbsoluteMatches;
        public static SettingHandle<ThingDefHashSetHandler> LimitModeSingleMelee_Selection;
        #endregion
        #region LimitModeAmountMelee
        public static SettingHandle<float> LimitModeAmountMelee_Relative;
        public static SettingHandle<float> LimitModeAmountMelee_Absolute;
        public static SettingHandle<int> LimitModeAmountMelee_Slots;
        #endregion
        #region LimitModeSingleRanged
        public static SettingHandle<float> LimitModeSingleRanged_Relative;
        public static SettingHandle<WeaponListKind> LimitModeSingleRanged_RelativeMatches;
        public static SettingHandle<float> LimitModeSingleRanged_Absolute;
        public static SettingHandle<WeaponListKind> LimitModeSingleRanged_AbsoluteMatches;
        public static SettingHandle<ThingDefHashSetHandler> LimitModeSingleRanged_Selection;
        #endregion
        #region LimitModeAmountRanged
        public static SettingHandle<float> LimitModeAmountRanged_Relative;
        public static SettingHandle<float> LimitModeAmountRanged_Absolute;
        public static SettingHandle<int> LimitModeAmountRanged_Slots;
        #endregion
        #region LimitModeAmountTotal
        public static SettingHandle<float> LimitModeAmountTotal_Relative;
        public static SettingHandle<float> LimitModeAmountTotal_Absolute;
        public static SettingHandle<int> LimitModeAmountTotal_Slots;
        #endregion

        public static SettingHandle<float> SidearmSpawnChance;
        public static SettingHandle<float> SidearmSpawnChanceDropoff;
        public static SettingHandle<float> SidearmBudgetMultiplier;
        public static SettingHandle<float> SidearmBudgetDropoff;

        public static SettingHandle<GoldfishModule.PrimaryWeaponMode> ColonistDefaultWeaponMode;
        public static SettingHandle<GoldfishModule.PrimaryWeaponMode> NPCDefaultWeaponMode;

        public static SettingHandle<DroppingModeOptionsEnum> DropMode;
        public static SettingHandle<bool> ReEquipBest;

        public static Color noHighlight = new Color(0, 0, 0, 0);
        public static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);
        public static Color highlight2 = new Color(0, 0.5f, 0, 0.1f);
        public static Color highlight3 = new Color(0, 0, 0.5f, 0.1f);
        public static Color highlight4 = new Color(0.5f, 0, 0.5f, 0.1f);
        public static Color highlight5 = new Color(0.5f, 0.5f, 0, 0.1f);
        public static Color highlight6 = new Color(0, 0.5f, 0.5f, 0.1f);

        public static bool ceOverride = false;
        public static bool CEOverride { get { return ceOverride; } }

        public override void DefsLoaded()
        {
            Settings.EntryName = "Simple Sidearms";

            float maxWeightMelee;
            float maxWeightRanged;
            GettersFilters.getHeaviestWeapons(out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            float maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);

            float maxCapacity = 35f;

            ActiveTab = Settings.GetHandle<OptionsTab>("ActiveTab", "ActiveTab_title".Translate(), "ActiveTab_desc".Translate(), OptionsTab.Presets, null, "ActiveTab_option_");
            ActiveTab.CustomDrawer = rect => {string[] names = Enum.GetNames(ActiveTab.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(ActiveTab, rect, names, forcedWidths, ExpansionMode.Vertical, noHighlight); };
            ActiveTab.Unsaved = true; 
            
            ActivePreset = Settings.GetHandle<Preset>("ActivePreset", null, null, Preset.NoneApplied, null, "Preset_option_"); 
            ActivePreset.CustomDrawer = rect => { return false; };
            //ActivePreset.NeverVisible = true; //so for some reason, when I set this the default preset doesnt get assinged on resetToDefaults. u wot mate
             
            //OptionsTab.Presets
            PresetCustom = Settings.GetHandle<Preset>("PresetCustom", "PresetCustom_title".Translate(), "PresetCustom_desc".Translate(), Preset.Custom, null, "Preset_option_");
            PresetCustom.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            PresetCustom.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, PresetCustom, "PresetCustom_label", this, noHighlight); };

            Preset0 = Settings.GetHandle<Preset>("Preset0", "Preset0_title".Translate(), "Preset0_desc".Translate(), Preset.Disabled, null, "Preset_option_");
            Preset0.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset0.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, Preset0, "Preset0_label", this, noHighlight); };
            Preset1half = Settings.GetHandle<Preset>("Preset1half", "Preset1half_title".Translate(), "Preset1half_desc".Translate(), Preset.Lite, null, "Preset_option_");
            Preset1half.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset1half.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, Preset1half, "Preset1half_label", this, noHighlight); };
            Preset1 = Settings.GetHandle<Preset>("Preset1", "Preset1_title".Translate(), "Preset1_desc".Translate(), Preset.LoadoutOnly, null, "Preset_option_");
            Preset1.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset1.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, Preset1, "Preset1_label", this, noHighlight); };
            Preset2 = Settings.GetHandle<Preset>("Preset2", "Preset2_title".Translate(), "Preset2_desc".Translate(), Preset.Basic, null, "Preset_option_");
            Preset2.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset2.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, Preset2, "Preset2_label", this, noHighlight); };
            Preset3 = Settings.GetHandle<Preset>("Preset3", "Preset3_title".Translate(), "Preset3_desc".Translate(), Preset.Advanced, null, "Preset_option_");
            Preset3.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset3.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, Preset3, "Preset3_label", this, noHighlight); };
            Preset4 = Settings.GetHandle<Preset>("Preset4", "Preset4_title".Translate(), "Preset4_desc".Translate(), Preset.Excessive, null, "Preset_option_");
            Preset4.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset4.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, Preset4, "Preset4_label", this, noHighlight); };
            Preset5 = Settings.GetHandle<Preset>("Preset5", "Preset5_title".Translate(), "Preset5_desc".Translate(), Preset.Brawler, null, "Preset_option_");
            Preset5.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset5.CustomDrawer = rect => { return CustomDrawer_ButtonLoadConfig(rect, Preset5, "Preset5_label", this, noHighlight); };

            PresetExport = Settings.GetHandle<bool>("PresetExport", "PresetExport_title".Translate(), "PresetExport_desc".Translate(), false);
            PresetExport.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            PresetExport.Unsaved = true;
            PresetExport.CustomDrawer = rect => { return CustomDrawer_ButtonExportConfig(rect, "PresetExport_label", this, noHighlight); };

            //OptionsTab.Automation
            ToolAutoSwitch = Settings.GetHandle<bool>("ToolAutoSwitch", "ToolAutoSwitch_title".Translate(), "ToolAutoSwitch_desc".Translate(), false);
            ToolAutoSwitch.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };
            ToolAutoSwitch.CustomDrawer = rect => { return HugsDrawerRebuild_Checkbox(ToolAutoSwitch, rect, highlight6); };

            OptimalMelee = Settings.GetHandle<bool>("OptimalMelee", "OptimalMelee_title".Translate(), "OptimalMelee_desc".Translate(), false);
            CQCAutoSwitch = Settings.GetHandle<bool>("CQCAutoSwitch", "CQCAutoSwitch_title".Translate(), "CQCAutoSwitch_desc".Translate(), false);
            CQCTargetOnly = Settings.GetHandle<bool>("CQCTargetOnly", "CQCTargetOnly_title".Translate(), "CQCTargetOnly_desc".Translate(), false);

            OptimalMelee.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };
            CQCAutoSwitch.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };
            CQCTargetOnly.VisibilityPredicate = delegate { return CQCAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };

            OptimalMelee.CustomDrawer = rect => { return HugsDrawerRebuild_Checkbox(OptimalMelee, rect, highlight1); };
            CQCAutoSwitch.CustomDrawer = rect => { return HugsDrawerRebuild_Checkbox(CQCAutoSwitch, rect, highlight1); };
            CQCTargetOnly.CustomDrawer = rect => { return HugsDrawerRebuild_Checkbox(CQCTargetOnly, rect, highlight1); };

            /*Underline0 = Settings.GetHandle<bool>("NilA", null, "", false);
            Underline0.Unsaved = true;
            Underline0.CustomDrawer = rect => { return CustomDrawer_RighthandSideLine(rect, Underline0, noHighlight); };
            Underline0.CustomDrawerHeight = 3f;
            Underline0.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };*/

            RangedCombatAutoSwitch = Settings.GetHandle<bool>("RangedCombatAutoSwitch", "RangedCombatAutoSwitch_title".Translate(), "RangedCombatAutoSwitch_desc".Translate(), false);
            RangedCombatAutoSwitchMaxWarmup = Settings.GetHandle<float>("RangedCombatAutoSwitchMaxWarmup", "RangedCombatAutoSwitchMaxWarmup_title".Translate(), "RangedCombatAutoSwitchMaxWarmup_desc".Translate(), 0.75f);
            RangedCombatAutoSwitch.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };
            RangedCombatAutoSwitchMaxWarmup.VisibilityPredicate = delegate { return RangedCombatAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };
            RangedCombatAutoSwitch.CustomDrawer = rect => { return HugsDrawerRebuild_Checkbox(RangedCombatAutoSwitch, rect, highlight2); };
            RangedCombatAutoSwitchMaxWarmup.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, RangedCombatAutoSwitchMaxWarmup, true, highlight2); };

            SpeedSelectionBiasMelee = Settings.GetHandle<float>("SpeedSelectionBiasMelee", "SpeedSelectionBiasMelee_title".Translate(), "SpeedSelectionBiasMelee_desc".Translate(), 1.0f);
            SpeedSelectionBiasMelee.VisibilityPredicate = delegate { return CQCAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };
            SpeedSelectionBiasMelee.CustomDrawer = rect => { return CustomDrawer_SpeedBiasSliderMelee(rect, SpeedSelectionBiasMelee,  highlight3); };
            SpeedSelectionBiasMelee.CustomDrawerHeight = 32f + 24f;
            SpeedSelectionBiasRanged = Settings.GetHandle<float>("SpeedSelectionBiasRanged", "SpeedSelectionBiasRanged_title".Translate(), "SpeedSelectionBiasRanged_desc".Translate(), 1.25f);
            SpeedSelectionBiasRanged.CustomDrawer = rect => { return CustomDrawer_SpeedBiasSliderRanged(rect, SpeedSelectionBiasRanged, highlight3); };
            SpeedSelectionBiasRanged.VisibilityPredicate = delegate { return RangedCombatAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };
            SpeedSelectionBiasRanged.CustomDrawerHeight = 32f + 24f;

            //OptionsTab.Allowances
            SeparateModes = Settings.GetHandle<bool>("SeparateModes", "SeparateModes_title".Translate(), "SeparateModes_desc".Translate(), false);
            SeparateModes.CustomDrawer = rect => { return HugsDrawerRebuild_Checkbox(SeparateModes, rect, noHighlight); };
            SeparateModes.VisibilityPredicate = delegate { return !CEOverride && ActiveTab == OptionsTab.Allowances; };

            LimitModeSingle = Settings.GetHandle<LimitModeSingleSidearm>("LimitModeSingle", "LimitModeSingle_title".Translate(), "LimitModeSingle_desc".Translate(), LimitModeSingleSidearm.None, null, "LimitModeSingle_option_");
            LimitModeSingle.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeSingle.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeSingle, rect, names, forcedWidths, ExpansionMode.Vertical, highlight1); };
            #region subItems
            LimitModeSingle_Relative = Settings.GetHandle<float>("LimitModeSingle_Relative", "MaximumMassSingleRelative_title".Translate(), "MaximumMassSingleRelative_desc".Translate(), 0f);
            LimitModeSingle_RelativeMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingle_RelativeMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Both, null, "WeaponListKind_option_");
            LimitModeSingle_Absolute = Settings.GetHandle<float>("LimitModeSingle_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 0f);
            LimitModeSingle_AbsoluteMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingle_AbsoluteMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Both, null, "WeaponListKind_option_");
            LimitModeSingle_Selection = Settings.GetHandle<ThingDefHashSetHandler>("LimitModeSingle_Selection", "SidearmSelection_title".Translate(), "SidearmSelection_desc".Translate(), null);
            LimitModeSingle_Relative.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingle_Relative, true, highlight1); };
            LimitModeSingle_RelativeMatches.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_RelativeMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveRelative(rect, LimitModeSingle_RelativeMatches, highlight1); };
            LimitModeSingle_RelativeMatches.Unsaved = true;
            LimitModeSingle_Absolute.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingle_Absolute, false, 0, maxWeightTotal, highlight1); };
            LimitModeSingle_AbsoluteMatches.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_AbsoluteMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveAbsolute(rect, LimitModeSingle_AbsoluteMatches, highlight1); };
            LimitModeSingle_AbsoluteMatches.Unsaved = true;
            LimitModeSingle_Selection.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.Selection) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_Selection.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_active(rect, LimitModeSingle_Selection, WeaponListKind.Both, highlight1, "ConsideredSidearms".Translate(), "NotConsideredSidearms".Translate()); };
            #endregion
            LimitModeAmount = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmount", "LimitModeAmount_title".Translate(), "LimitModeAmount_desc".Translate(), LimitModeAmountOfSidearms.MaximumCarryWeightOnly, null, "LimitModeAmount_option_");
            LimitModeAmount.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmount.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmount, rect, names, forcedWidths, ExpansionMode.Vertical, highlight2); };
            #region subItems
            LimitModeAmount_Relative = Settings.GetHandle<float>("LimitModeAmount_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmount_Absolute = Settings.GetHandle<float>("LimitModeAmount_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmount_Slots = Settings.GetHandle<int>("LimitModeAmount_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 0);
            LimitModeAmount_Slots.CustomDrawer = rect => { return HugsDrawerRebuild_Spinner(LimitModeAmount_Slots, rect, highlight2); };
            LimitModeAmount_Relative.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeAmount == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmount_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmount_Relative, true, highlight2); };
            LimitModeAmount_Absolute.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeAmount == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmount_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmount_Absolute, false, 0, maxCapacity, highlight2); };
            LimitModeAmount_Slots.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == false) && (LimitModeAmount == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion
            LimitModeSingleMelee = Settings.GetHandle<LimitModeSingleSidearm>("LimitModeSingleMelee", "LimitModeSingleMelee_title".Translate(), "LimitModeSingleMelee_desc".Translate(), LimitModeSingleSidearm.None, null, "LimitModeSingle_option_");
            LimitModeSingleMelee.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeSingleMelee.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeSingleMelee, rect, names, forcedWidths, ExpansionMode.Vertical, highlight1); };
            #region subItems
            LimitModeSingleMelee_Relative = Settings.GetHandle<float>("LimitModeSingleMelee_Relative", "MaximumMassSingleRelative_title".Translate(), "MaximumMassSingleRelative_desc".Translate(), 0f);
            LimitModeSingleMelee_RelativeMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleMelee_RelativeMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Melee, null, "WeaponListKind_option_");
            LimitModeSingleMelee_Absolute = Settings.GetHandle<float>("LimitModeSingleMelee_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 0f);
            LimitModeSingleMelee_AbsoluteMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleMelee_AbsoluteMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Melee, null, "WeaponListKind_option_");
            LimitModeSingleMelee_Selection = Settings.GetHandle<ThingDefHashSetHandler>("LimitModeSingleMelee_Selection", "SidearmSelection_title".Translate(), "SidearmSelection_desc".Translate(), null);
            LimitModeSingleMelee_Relative.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleMelee_Relative, true, highlight1); };
            LimitModeSingleMelee_RelativeMatches.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_RelativeMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveRelative(rect, LimitModeSingleMelee_RelativeMatches, highlight1); };
            LimitModeSingleMelee_RelativeMatches.Unsaved = true;
            LimitModeSingleMelee_Absolute.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleMelee_Absolute, false, 0, maxWeightMelee, highlight1); };
            LimitModeSingleMelee_AbsoluteMatches.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_AbsoluteMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveAbsolute(rect, LimitModeSingleMelee_AbsoluteMatches, highlight1); };
            LimitModeSingleMelee_AbsoluteMatches.Unsaved = true;
            LimitModeSingleMelee_Selection.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.Selection) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_Selection.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_active(rect, LimitModeSingleMelee_Selection, WeaponListKind.Melee, highlight1, "ConsideredSidearms".Translate(), "NotConsideredSidearms".Translate()); };
            #endregion
            LimitModeAmountMelee = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmountMelee", "LimitModeAmountMelee_title".Translate(), "LimitModeAmountMelee_desc".Translate(), LimitModeAmountOfSidearms.MaximumCarryWeightOnly, null, "LimitModeAmount_option_");
            LimitModeAmountMelee.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmountMelee.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmountMelee, rect, names, forcedWidths, ExpansionMode.Vertical, highlight2); };
            #region subItems
            LimitModeAmountMelee_Relative = Settings.GetHandle<float>("LimitModeAmountMelee_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmountMelee_Absolute = Settings.GetHandle<float>("LimitModeAmountMelee_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmountMelee_Slots = Settings.GetHandle<int>("LimitModeAmountMelee_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 0);
            LimitModeAmountMelee_Slots.CustomDrawer = rect => { return HugsDrawerRebuild_Spinner(LimitModeAmountMelee_Slots, rect, highlight2); };
            LimitModeAmountMelee_Relative.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountMelee == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountMelee_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountMelee_Relative, true, highlight2); };
            LimitModeAmountMelee_Absolute.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountMelee == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountMelee_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountMelee_Absolute, false, 0, maxCapacity, highlight2); };
            LimitModeAmountMelee_Slots.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountMelee == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion

            /*Underline1 = Settings.GetHandle<bool>("NilB", null, "", false);
            Underline1.Unsaved = true;
            Underline1.CustomDrawer = rect => { return CustomDrawer_RighthandSideLine(rect, Underline1); };
            Underline1.CustomDrawerHeight = 3f;
            Underline1.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Allowances && SeparateModes.Value == true; };*/

            LimitModeSingleRanged = Settings.GetHandle<LimitModeSingleSidearm>("LimitModeSingleRanged", "LimitModeSingleRanged_title".Translate(), "LimitModeSingleRanged_desc".Translate(), LimitModeSingleSidearm.None, null, "LimitModeSingle_option_");
            LimitModeSingleRanged.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeSingleRanged.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeSingleRanged, rect, names, forcedWidths, ExpansionMode.Vertical, highlight3); };
            #region subItems
            LimitModeSingleRanged_Relative = Settings.GetHandle<float>("LimitModeSingleRanged_Relative", "MaximumMassSingleRelative_title".Translate(), "MaximumMassSingleRelative_desc".Translate(), 0f);
            LimitModeSingleRanged_RelativeMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleRanged_RelativeMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Ranged, null, "WeaponListKind_option_");
            LimitModeSingleRanged_Absolute = Settings.GetHandle<float>("LimitModeSingleRanged_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 0f);
            LimitModeSingleRanged_AbsoluteMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleRanged_AbsoluteMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Ranged, null, "WeaponListKind_option_");
            LimitModeSingleRanged_Selection = Settings.GetHandle<ThingDefHashSetHandler>("LimitModeSingleRanged_Selection", "SidearmSelection_title".Translate(), "SidearmSelection_desc".Translate(), null);
            LimitModeSingleRanged_Relative.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleRanged_Relative, true, highlight3); };
            LimitModeSingleRanged_RelativeMatches.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_RelativeMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveRelative(rect, LimitModeSingleRanged_RelativeMatches, highlight3); };
            LimitModeSingleRanged_RelativeMatches.Unsaved = true;
            LimitModeSingleRanged_Absolute.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleRanged_Absolute, false, 0, maxWeightRanged, highlight3); };
            LimitModeSingleRanged_AbsoluteMatches.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_AbsoluteMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveAbsolute(rect, LimitModeSingleRanged_AbsoluteMatches, highlight3); };
            LimitModeSingleRanged_AbsoluteMatches.Unsaved = true;
            LimitModeSingleRanged_Selection.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.Selection) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_Selection.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_active(rect, LimitModeSingleRanged_Selection, WeaponListKind.Ranged, highlight3, "ConsideredSidearms".Translate(), "NotConsideredSidearms".Translate()); };
            #endregion
            LimitModeAmountRanged = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmountRanged", "LimitModeAmountRanged_title".Translate(), "LimitModeAmountRanged_desc".Translate(), LimitModeAmountOfSidearms.MaximumCarryWeightOnly, null, "LimitModeAmount_option_");
            LimitModeAmountRanged.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmountRanged.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmountRanged, rect, names, forcedWidths, ExpansionMode.Vertical, highlight4); };
            #region subItems
            LimitModeAmountRanged_Relative = Settings.GetHandle<float>("LimitModeAmountRanged_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmountRanged_Absolute = Settings.GetHandle<float>("LimitModeAmountRanged_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmountRanged_Slots = Settings.GetHandle<int>("LimitModeAmountRanged_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 0);
            LimitModeAmountRanged_Slots.CustomDrawer = rect => { return HugsDrawerRebuild_Spinner(LimitModeAmountRanged_Slots, rect, highlight4); };
            LimitModeAmountRanged_Relative.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountRanged == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountRanged_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountRanged_Relative, true, highlight4); };
            LimitModeAmountRanged_Absolute.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountRanged == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountRanged_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountRanged_Absolute, false, 0, maxCapacity, highlight4); };
            LimitModeAmountRanged_Slots.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountRanged == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion

            /*Underline2 = Settings.GetHandle<bool>("NilC", null, "", false);
            Underline2.Unsaved = true;
            Underline2.CustomDrawer = rect => { return CustomDrawer_RighthandSideLine(rect, Underline2); };
            Underline2.CustomDrawerHeight = 3f;
            Underline2.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Allowances && SeparateModes.Value == true; };*/

            LimitModeAmountTotal = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmountTotal", "LimitModeAmountTotal_title".Translate(), "LimitModeAmountTotal_desc".Translate(), LimitModeAmountOfSidearms.MaximumCarryWeightOnly, null, "LimitModeAmount_option_");
            LimitModeAmountTotal.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmountTotal.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmountTotal, rect, names, forcedWidths, ExpansionMode.Vertical, highlight5); };
            #region subItems
            LimitModeAmountTotal_Relative = Settings.GetHandle<float>("LimitModeAmountTotal_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmountTotal_Absolute = Settings.GetHandle<float>("LimitModeAmountTotal_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmountTotal_Slots = Settings.GetHandle<int>("LimitModeAmountTotal_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 0);
            LimitModeAmountTotal_Slots.CustomDrawer = rect => { return HugsDrawerRebuild_Spinner(LimitModeAmountTotal_Slots, rect, highlight5); };
            LimitModeAmountTotal_Relative.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountTotal == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountTotal_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountTotal_Relative, true, highlight5); };
            LimitModeAmountTotal_Absolute.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountTotal == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountTotal_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountTotal_Absolute, false, 0, maxCapacity, highlight5); };
            LimitModeAmountTotal_Slots.VisibilityPredicate = delegate { return !CEOverride && (SeparateModes == true) && (LimitModeAmountTotal == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion 

            LimitModeSingle.VisibilityPredicate = delegate { return !CEOverride && SeparateModes.Value == false && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmount.VisibilityPredicate = delegate { return !CEOverride && SeparateModes.Value == false && (ActiveTab == OptionsTab.Allowances); };

            LimitModeSingleMelee.VisibilityPredicate = delegate { return !CEOverride && SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountMelee.VisibilityPredicate = delegate { return !CEOverride && SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged.VisibilityPredicate = delegate { return !CEOverride && SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountRanged.VisibilityPredicate = delegate { return !CEOverride && SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountTotal.VisibilityPredicate = delegate { return !CEOverride && SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };

            LimitCarryInfo = Settings.GetHandle<bool>("LimitCarryInfo", null, "LimitCarryInfo_desc".Translate(), false);
            LimitCarryInfo.VisibilityPredicate = delegate { return !CEOverride && ActiveTab == OptionsTab.Allowances; };
            LimitCarryInfo.CustomDrawer = rect => { return CustomDrawer_RighthandSideLabel(rect, "LimitCarryInfo_title".Translate(), noHighlight); };
            LimitCarryInfo.Unsaved = true;

            CEOverrideInfo = Settings.GetHandle<bool>("CEOverrideInfo", null, "CEOverrideInfo_desc".Translate(), false);
            CEOverrideInfo.VisibilityPredicate = delegate { return CEOverride && ActiveTab == OptionsTab.Allowances; };
            CEOverrideInfo.CustomDrawer = rect => { return CustomDrawer_RighthandSideLabel(rect, "CEOverrideInfo_title".Translate(), noHighlight); };
            CEOverrideInfo.Unsaved = true;

            //OptionsTab.Spawning
            SidearmSpawnChance = Settings.GetHandle<float>("SidearmSpawnChance", "SidearmSpawnChance_title".Translate(), "SidearmSpawnChance_desc".Translate(), 0.5f);
            SidearmSpawnChance.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, SidearmSpawnChance, true, noHighlight); };
            SidearmSpawnChance.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };

            SidearmSpawnChanceDropoff = Settings.GetHandle<float>("SidearmSpawnChanceDropoff", "SidearmSpawnChanceDropoff_title".Translate(), "SidearmSpawnChanceDropoff_desc".Translate(), 0.25f);
            SidearmSpawnChanceDropoff.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, SidearmSpawnChanceDropoff, true, noHighlight); };
            SidearmSpawnChanceDropoff.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };

            SidearmBudgetMultiplier = Settings.GetHandle<float>("SidearmBudgetMultiplier", "SidearmBudgetMultiplier_title".Translate(), "SidearmBudgetMultiplier_desc".Translate(), 0.5f);
            SidearmBudgetMultiplier.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, SidearmBudgetMultiplier, true, noHighlight); };
            SidearmBudgetMultiplier.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };


            SidearmBudgetDropoff = Settings.GetHandle<float>("SidearmBudgetDropoff", "SidearmBudgetDropoff_title".Translate(), "SidearmBudgetDropoff_desc".Translate(), 0.25f);
            SidearmBudgetDropoff.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, SidearmBudgetDropoff, true, noHighlight); };
            SidearmBudgetDropoff.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };

            ColonistDefaultWeaponMode = Settings.GetHandle<GoldfishModule.PrimaryWeaponMode>("ColonistDefaultWeaponMode", "ColonistDefaultWeaponMode_title".Translate(), "ColonistDefaultWeaponMode_desc".Translate(), GoldfishModule.PrimaryWeaponMode.BySkill, null, "PrimaryWeaponMode_option_");
            ColonistDefaultWeaponMode.CustomDrawer = rect => { string[] names = Enum.GetNames(ColonistDefaultWeaponMode.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(ColonistDefaultWeaponMode, rect, names, forcedWidths, ExpansionMode.Vertical, noHighlight); };
            ColonistDefaultWeaponMode.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };

            NPCDefaultWeaponMode = Settings.GetHandle<GoldfishModule.PrimaryWeaponMode>("NPCDefaultWeaponMode", "NPCDefaultWeaponMode_title".Translate(), "NPCDefaultWeaponMode_desc".Translate(), GoldfishModule.PrimaryWeaponMode.ByGenerated, null, "PrimaryWeaponMode_option_");
            NPCDefaultWeaponMode.CustomDrawer = rect => { string[] names = Enum.GetNames(NPCDefaultWeaponMode.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(NPCDefaultWeaponMode, rect, names, forcedWidths, ExpansionMode.Vertical, noHighlight); };
            NPCDefaultWeaponMode.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };

            //OptionsTab.Misc
            DropMode = Settings.GetHandle<DroppingModeOptionsEnum>("DropMode", "DropMode_title".Translate(), "DropMode_desc".Translate(), DroppingModeOptionsEnum.Never, null, "DropMode_option_");
            DropMode.CustomDrawer = rect => {
                string[] names = Enum.GetNames(DropMode.Value.GetType());
                float[] forcedWidths = new float[names.Length];
                return CustomDrawer_Enumlist(DropMode, rect, names, forcedWidths, ExpansionMode.Vertical, noHighlight);
            };
            DropMode.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Misc; };

            ReEquipBest = Settings.GetHandle<bool>("ReEquipBest", "ReEquipBest_title".Translate(), "ReEquipBest_desc".Translate(), true);
            ReEquipBest.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Misc; };
            ReEquipBest.CustomDrawer = rect => { return HugsDrawerRebuild_Checkbox(ReEquipBest, rect, highlight2); };

            OptimalMelee.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            CQCAutoSwitch.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            CQCTargetOnly.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            RangedCombatAutoSwitch.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            RangedCombatAutoSwitchMaxWarmup.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            SpeedSelectionBiasMelee.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            SpeedSelectionBiasRanged.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            SeparateModes.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingle.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingle_Absolute.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingle_Relative.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingle_Selection.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmount.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmount_Absolute.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmount_Relative.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmount_Slots.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleMelee.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleMelee_Absolute.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleMelee_Relative.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleMelee_Selection.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleRanged.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleRanged_Absolute.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleRanged_Relative.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeSingleRanged_Selection.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountMelee.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountMelee_Absolute.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountMelee_Relative.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountMelee_Slots.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountRanged.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountRanged_Absolute.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountRanged_Relative.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountRanged_Slots.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountTotal.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountTotal_Absolute.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountTotal_Relative.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            LimitModeAmountTotal_Slots.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            SidearmSpawnChance.OnValueChanged += delegate { if(ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            SidearmSpawnChanceDropoff.OnValueChanged += delegate { if (ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            SidearmBudgetMultiplier.OnValueChanged += delegate { if (ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            SidearmBudgetDropoff.OnValueChanged += delegate { if (ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            ColonistDefaultWeaponMode.OnValueChanged += delegate { if (ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };
            NPCDefaultWeaponMode.OnValueChanged += delegate { if (ActivePreset.Value != Preset.NoneApplied) ActivePreset.Value = Preset.Custom; };

            /*ceOverride = CEPatcher.isCEPresent(this.HarmonyInst);

            if (CEOverride)
                CEPatcher.patchCE(this.HarmonyInst);*/

        }

        public int delay = 0;
        public override void Update()
        {
            base.Update();
            if (ActivePreset.Value == Preset.NoneApplied)
            {
                if (delay > 5)
                {
                    Presets.presetChanged(Preset.Basic, this);
                    delay = 0;
                }
                else
                    delay++;
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            //avoiding death to BetterLoading
        }


        public override void WorldLoaded()
        {
            configData = Find.World.GetComponent<SimpleSidearmsConfigData>();
            //saveData =   UtilityWorldObjectManager.GetUtilityWorldObject<SimpleSidearmsData>();
        }

        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);

            if(CEOverride)
                LessonAutoActivator.TeachOpportunity(SidearmsDefOf.Concept_CEOverride, OpportunityType.Critical);
        }

        public void UpdateConfig(string config, bool resetFirst)
        {
            ModSettingsPack pack = HugsLibController.SettingsManager.GetModSettings(ModIdentifier);
            if (pack == null)
                return;

            if (resetFirst)
            {
                //TODO: this seems to fail sometime.
                foreach (SettingHandle handle in pack.Handles)
                    handle.ResetToDefault();
            }

            XElement element = XElement.Parse(config);
            foreach (var childNode in element.Elements())
            {
                bool found = false;
                foreach (SettingHandle handle in pack.Handles)
                {
                    if (handle.Name.Equals(childNode.Name.ToString()))
                    {
                        int inti;
                        if (int.TryParse(childNode.Value, out inti))
                        {
                            handle.StringValue = childNode.Value;
                            found = true;
                            break;
                        }
                        else
                        {
                            handle.StringValue = childNode.Value;
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                    Log.Message("WARNING: could not update " + childNode.Name);
            }
        }
    } 
}
