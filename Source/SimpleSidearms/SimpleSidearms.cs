using Harmony;
using HugsLib.Settings;
using RimWorld;
using SimpleSidearms.hugsLibSettings;
using SimpleSidearms.rimworld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public override string ModIdentifier { get { return "SimpleSidearms"; } }

        internal enum OptionsTab { Folded, Presets, Automation, Allowances, Spawning, Misc}
        private float TabsNegativeOffset = 100f;
        
        internal static SettingHandle<OptionsTab> ActiveTab;

        internal static SettingHandle<bool> WIP;
        internal static SettingHandle<Preset> ActivePreset;

        /*internal static SettingHandle<Preset> Preset0;
        internal static SettingHandle<Preset> Preset1;*/
        /*internal static SettingHandle<Preset> Preset2;
        internal static SettingHandle<Preset> Preset3;
        internal static SettingHandle<Preset> Preset4;
        internal static SettingHandle<Preset> Preset5;*/

        internal static SettingHandle<bool> CQCAutoSwitch;
        internal static SettingHandle<bool> CQCFistSwitch;
        internal static SettingHandle<bool> CQCTargetOnly;
        internal static SettingHandle<bool> Underline0;
        internal static SettingHandle<bool> RangedCombatAutoSwitch;
        internal static SettingHandle<float> RangedCombatAutoSwitchMaxWarmup;
        internal static SettingHandle<bool> SingleshotAutoSwitch;

        internal static SettingHandle<float> SpeedSelectionBiasMelee;
        internal static SettingHandle<float> SpeedSelectionBiasRanged;

        internal static SettingHandle<bool> LimitCarryInfo;

        internal static SettingHandle<bool> SeparateModes;
        internal static SettingHandle<LimitModeSingleSidearm> LimitModeSingle;
        internal static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmount;

        internal static SettingHandle<LimitModeSingleSidearm> LimitModeSingleMelee;
        internal static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmountMelee;
        internal static SettingHandle<bool> Underline1;
        internal static SettingHandle<LimitModeSingleSidearm> LimitModeSingleRanged;
        internal static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmountRanged;
        internal static SettingHandle<bool> Underline2;
        internal static SettingHandle<LimitModeAmountOfSidearms> LimitModeAmountTotal;

        #region LimitModeSingle
        internal static SettingHandle<float> LimitModeSingle_Relative;
        internal static SettingHandle<WeaponListKind> LimitModeSingle_RelativeMatches;
        internal static SettingHandle<float> LimitModeSingle_Absolute;
        internal static SettingHandle<WeaponListKind> LimitModeSingle_AbsoluteMatches;
        internal static SettingHandle<StringListHandler> LimitModeSingle_Selection;
        #endregion
        #region LimitModeAmount
        internal static SettingHandle<float> LimitModeAmount_Relative;
        internal static SettingHandle<float> LimitModeAmount_Absolute;
        internal static SettingHandle<int> LimitModeAmount_Slots;
        #endregion

        #region LimitModeSingleMelee
        internal static SettingHandle<float> LimitModeSingleMelee_Relative;
        internal static SettingHandle<WeaponListKind> LimitModeSingleMelee_RelativeMatches;
        internal static SettingHandle<float> LimitModeSingleMelee_Absolute;
        internal static SettingHandle<WeaponListKind> LimitModeSingleMelee_AbsoluteMatches;
        internal static SettingHandle<StringListHandler> LimitModeSingleMelee_Selection;
        #endregion
        #region LimitModeAmountMelee
        internal static SettingHandle<float> LimitModeAmountMelee_Relative;
        internal static SettingHandle<float> LimitModeAmountMelee_Absolute;
        internal static SettingHandle<int> LimitModeAmountMelee_Slots;
        #endregion
        #region LimitModeSingleRanged
        internal static SettingHandle<float> LimitModeSingleRanged_Relative;
        internal static SettingHandle<WeaponListKind> LimitModeSingleRanged_RelativeMatches;
        internal static SettingHandle<float> LimitModeSingleRanged_Absolute;
        internal static SettingHandle<WeaponListKind> LimitModeSingleRanged_AbsoluteMatches;
        internal static SettingHandle<StringListHandler> LimitModeSingleRanged_Selection;
        #endregion
        #region LimitModeAmountRanged
        internal static SettingHandle<float> LimitModeAmountRanged_Relative;
        internal static SettingHandle<float> LimitModeAmountRanged_Absolute;
        internal static SettingHandle<int> LimitModeAmountRanged_Slots;
        #endregion
        #region LimitModeAmountTotal
        internal static SettingHandle<float> LimitModeAmountTotal_Relative;
        internal static SettingHandle<float> LimitModeAmountTotal_Absolute;
        internal static SettingHandle<int> LimitModeAmountTotal_Slots;
        #endregion
        
        internal static SettingHandle<float> SidearmSpawnChance;
        internal static SettingHandle<bool> SidearmsEnableNeolithicExtension;
        internal static SettingHandle<StringListHandler> SidearmsNeolithicExtension;

        internal static SettingHandle<DroppingModeOptionsEnum> DropMode;

        public override void DefsLoaded()
        {
            float maxWeightMelee;
            float maxWeightRanged;
            GettersFilters.getHeaviestWeapons(PawnSidearmsGenerator.getWeaponsList(), out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            float maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);

            float maxCapacity = 35f;

            ActiveTab = Settings.GetHandle<OptionsTab>("ActiveTab", "ActiveTab_title".Translate(), "ActiveTab_desc".Translate(), OptionsTab.Folded, null, "ActiveTab_option_");
            ActiveTab.CustomDrawer = rect => {
                string[] names = Enum.GetNames(ActiveTab.Value.GetType());
                float[] forcedWidths = new float[names.Length];
                forcedWidths[0] = 30;
                rect = new Rect(rect);
                rect.position = new Vector2(rect.position.x - TabsNegativeOffset, rect.position.y);
                rect.width = rect.width + TabsNegativeOffset;
                return CustomDrawer_Enumlist(ActiveTab, rect, names, forcedWidths, ExpansionMode.Vertical);
            };
            ActiveTab.Unsaved = true;

            LimitCarryInfo = Settings.GetHandle<bool>("WIP", null, "Work in progress".Translate(), false);
            LimitCarryInfo.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            LimitCarryInfo.CustomDrawer = rect => { return CustomDrawer_RighthandSideLabel(rect, "Presets are currently being implemented."); };
            LimitCarryInfo.Unsaved = true;


            ActivePreset = Settings.GetHandle<Preset>("ActivePreset", null, null, Preset.NoneApplied);
            ActivePreset.NeverVisible = true;

            /*Preset0 = Settings.GetHandle<Preset>("Preset0", "Preset0_title".Translate(), "Preset0_desc".Translate(), Preset.Basic);
            Preset0.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset0.CustomDrawer = rect => { return CustomDrawer_Button(rect, Preset0, "Preset0_label"); };
            Preset1 = Settings.GetHandle<Preset>("Preset1", "Preset1_title".Translate(), "Preset1_desc".Translate(), Preset.LoadoutOnly);
            Preset1.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Presets; };
            Preset1.CustomDrawer = rect => { return CustomDrawer_Button(rect, Preset1, "Preset1_label"); };*/
            /*Preset2 = Settings.GetHandle<Preset>("Preset2", "Preset2_title".Translate(), "Preset2_desc".Translate(), Preset.Basic);
            Presets.setupPresetSetting(Preset2);
            Preset3 = Settings.GetHandle<Preset>("Preset3", "Preset3_title".Translate(), "Preset3_desc".Translate(), Preset.Basic);
            Presets.setupPresetSetting(Preset3);
            Preset4 = Settings.GetHandle<Preset>("Preset4", "Preset4_title".Translate(), "Preset4_desc".Translate(), Preset.Basic);
            Presets.setupPresetSetting(Preset4);
            Preset5 = Settings.GetHandle<Preset>("Preset5", "Preset5_title".Translate(), "Preset5_desc".Translate(), Preset.Basic);
            Presets.setupPresetSetting(Preset5);*/

            CQCAutoSwitch = Settings.GetHandle<bool>("CQCAutoSwitch", "CQCAutoSwitch_title".Translate(), "CQCAutoSwitch_desc".Translate(), false);
            CQCFistSwitch = Settings.GetHandle<bool>("CQCFistSwitch", "CQCFistSwitch_title".Translate(), "CQCFistSwitch_desc".Translate(), false);
            CQCTargetOnly = Settings.GetHandle<bool>("CQCTargetOnly", "CQCTargetOnly_title".Translate(), "CQCTargetOnly_desc".Translate(), false);
            CQCAutoSwitch.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };
            CQCFistSwitch.VisibilityPredicate = delegate { return CQCAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };
            CQCTargetOnly.VisibilityPredicate = delegate { return CQCAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };
            SpeedSelectionBiasMelee = Settings.GetHandle<float>("SpeedSelectionBiasMelee", "SpeedSelectionBiasMelee_title".Translate(), "SpeedSelectionBiasMelee_desc".Translate(), 1f);
            SpeedSelectionBiasMelee.VisibilityPredicate = delegate { return CQCAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };

            Underline0 = Settings.GetHandle<bool>("NilA", null, "", false);
            Underline0.Unsaved = true;
            Underline0.CustomDrawer = rect => { return CustomDrawer_RighthandSideLine(rect, Underline0); };
            Underline0.CustomDrawerHeight = 3f;
            Underline0.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };

            RangedCombatAutoSwitch = Settings.GetHandle<bool>("RangedCombatAutoSwitch", "RangedCombatAutoSwitch_title".Translate(), "RangedCombatAutoSwitch_desc".Translate(), false);
            RangedCombatAutoSwitchMaxWarmup = Settings.GetHandle<float>("RangedCombatAutoSwitchMaxWarmup", "RangedCombatAutoSwitchMaxWarmup_title".Translate(), "RangedCombatAutoSwitchMaxWarmup_desc".Translate(), 1f);
            RangedCombatAutoSwitch.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };
            RangedCombatAutoSwitchMaxWarmup.VisibilityPredicate = delegate { return RangedCombatAutoSwitch.Value == true && ActiveTab == OptionsTab.Automation; };
            RangedCombatAutoSwitchMaxWarmup.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, RangedCombatAutoSwitchMaxWarmup, true); };

            SingleshotAutoSwitch = Settings.GetHandle<bool>("SingleshotAutoSwitch", "SingleshotAutoSwitch_title".Translate(), "SingleshotAutoSwitch_desc".Translate(), false);
            SpeedSelectionBiasRanged = Settings.GetHandle<float>("SpeedSelectionBiasRanged", "SpeedSelectionBiasRanged_title".Translate(), "SpeedSelectionBiasRanged_desc".Translate(), 1f);
            SingleshotAutoSwitch.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Automation; };
            SpeedSelectionBiasRanged.VisibilityPredicate = delegate { return (RangedCombatAutoSwitch.Value == true | SingleshotAutoSwitch.Value == true) && ActiveTab == OptionsTab.Automation; };

            SeparateModes = Settings.GetHandle<bool>("SeparateModes", "SeparateModes_title".Translate(), "SeparateModes_desc".Translate(), false);
            SeparateModes.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Allowances; };

            LimitModeSingle = Settings.GetHandle<LimitModeSingleSidearm>("LimitModeSingle", "LimitModeSingle_title".Translate(), "LimitModeSingle_desc".Translate(), LimitModeSingleSidearm.None, null, "LimitModeSingle_option_");
            LimitModeSingle.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeSingle.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeSingle, rect, names, forcedWidths, ExpansionMode.Vertical); };
            #region subItems
            LimitModeSingle_Relative = Settings.GetHandle<float>("LimitModeSingle_Relative", "MaximumMassSingleRelative_title".Translate(), "MaximumMassSingleRelative_desc".Translate(), 0f);
            LimitModeSingle_RelativeMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingle_RelativeMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Both);
            LimitModeSingle_Absolute = Settings.GetHandle<float>("LimitModeSingle_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 0f);
            LimitModeSingle_AbsoluteMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingle_AbsoluteMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Both);
            LimitModeSingle_Selection = Settings.GetHandle<StringListHandler>("LimitModeSingle_Selection", "SidearmSelection_title".Translate(), "SidearmSelection_desc".Translate(), null);
            LimitModeSingle_Relative.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingle_Relative, true); };
            LimitModeSingle_RelativeMatches.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_RelativeMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveRelative(rect, LimitModeSingle_RelativeMatches); };
            LimitModeSingle_RelativeMatches.Unsaved = true;
            LimitModeSingle_Absolute.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingle_Absolute, false, 0, maxWeightTotal); };
            LimitModeSingle_AbsoluteMatches.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_AbsoluteMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveAbsolute(rect, LimitModeSingle_AbsoluteMatches); };
            LimitModeSingle_AbsoluteMatches.Unsaved = true;
            LimitModeSingle_Selection.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeSingle == LimitModeSingleSidearm.Selection) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingle_Selection.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_active(rect, LimitModeSingle_Selection, WeaponListKind.Both); };
            #endregion
            LimitModeAmount = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmount", "LimitModeAmount_title".Translate(), "LimitModeAmount_desc".Translate(), LimitModeAmountOfSidearms.MaximumCarryWeightOnly, null, "LimitModeAmount_option_");
            LimitModeAmount.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmount.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmount, rect, names, forcedWidths, ExpansionMode.Vertical); };
            #region subItems
            LimitModeAmount_Relative = Settings.GetHandle<float>("LimitModeAmount_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmount_Absolute = Settings.GetHandle<float>("LimitModeAmount_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmount_Slots = Settings.GetHandle<int>("LimitModeAmount_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 4);
            LimitModeAmount_Relative.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeAmount == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmount_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmount_Relative, true); };
            LimitModeAmount_Absolute.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeAmount == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmount_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmount_Absolute, false, 0, maxCapacity); };
            LimitModeAmount_Slots.VisibilityPredicate = delegate { return (SeparateModes == false) && (LimitModeAmount == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion
            LimitModeSingleMelee = Settings.GetHandle<LimitModeSingleSidearm>("LimitModeSingleMelee", "LimitModeSingleMelee_title".Translate(), "LimitModeSingleMelee_desc".Translate(), LimitModeSingleSidearm.None, null, "LimitModeSingle_option_");
            LimitModeSingleMelee.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeSingleMelee.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeSingleMelee, rect, names, forcedWidths, ExpansionMode.Vertical); };
            #region subItems
            LimitModeSingleMelee_Relative = Settings.GetHandle<float>("LimitModeSingleMelee_Relative", "MaximumMassSingleRelative_title".Translate(), "MaximumMassSingleRelative_desc".Translate(), 0f);
            LimitModeSingleMelee_RelativeMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleMelee_RelativeMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Melee);
            LimitModeSingleMelee_Absolute = Settings.GetHandle<float>("LimitModeSingleMelee_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 0f);
            LimitModeSingleMelee_AbsoluteMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleMelee_AbsoluteMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Melee);
            LimitModeSingleMelee_Selection = Settings.GetHandle<StringListHandler>("LimitModeSingleMelee_Selection", "SidearmSelection_title".Translate(), "SidearmSelection_desc".Translate(), null);
            LimitModeSingleMelee_Relative.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleMelee_Relative, true); };
            LimitModeSingleMelee_RelativeMatches.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_RelativeMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveRelative(rect, LimitModeSingleMelee_RelativeMatches); };
            LimitModeSingleMelee_RelativeMatches.Unsaved = true;
            LimitModeSingleMelee_Absolute.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleMelee_Absolute, false, 0, maxWeightMelee); };
            LimitModeSingleMelee_AbsoluteMatches.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_AbsoluteMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveAbsolute(rect, LimitModeSingleMelee_AbsoluteMatches); };
            LimitModeSingleMelee_AbsoluteMatches.Unsaved = true;
            LimitModeSingleMelee_Selection.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleMelee == LimitModeSingleSidearm.Selection) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleMelee_Selection.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_active(rect, LimitModeSingleMelee_Selection, WeaponListKind.Melee); };
            #endregion
            LimitModeAmountMelee = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmountMelee", "LimitModeAmountMelee_title".Translate(), "LimitModeAmountMelee_desc".Translate(), LimitModeAmountOfSidearms.RelativeWeight, null, "LimitModeAmount_option_");
            LimitModeAmountMelee.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmountMelee.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmountMelee, rect, names, forcedWidths, ExpansionMode.Vertical); };
            #region subItems
            LimitModeAmountMelee_Relative = Settings.GetHandle<float>("LimitModeAmountMelee_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmountMelee_Absolute = Settings.GetHandle<float>("LimitModeAmountMelee_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmountMelee_Slots = Settings.GetHandle<int>("LimitModeAmountMelee_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 2);
            LimitModeAmountMelee_Relative.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountMelee == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountMelee_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountMelee_Relative, true); };
            LimitModeAmountMelee_Absolute.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountMelee == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountMelee_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountMelee_Absolute, false, 0, maxCapacity); };
            LimitModeAmountMelee_Slots.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountMelee == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion

            Underline1 = Settings.GetHandle<bool>("NilB", null, "", false);
            Underline1.Unsaved = true;
            Underline1.CustomDrawer = rect => { return CustomDrawer_RighthandSideLine(rect, Underline1); };
            Underline1.CustomDrawerHeight = 3f;
            Underline1.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Allowances && SeparateModes.Value == true; };

            LimitModeSingleRanged = Settings.GetHandle<LimitModeSingleSidearm>("LimitModeSingleRanged", "LimitModeSingleRanged_title".Translate(), "LimitModeSingleRanged_desc".Translate(), LimitModeSingleSidearm.None, null, "LimitModeSingle_option_");
            LimitModeSingleRanged.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeSingleRanged.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeSingleRanged, rect, names, forcedWidths, ExpansionMode.Vertical); };
            #region subItems
            LimitModeSingleRanged_Relative = Settings.GetHandle<float>("LimitModeSingleRanged_Relative", "MaximumMassSingleRelative_title".Translate(), "MaximumMassSingleRelative_desc".Translate(), 0f);
            LimitModeSingleRanged_RelativeMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleRanged_RelativeMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Ranged);
            LimitModeSingleRanged_Absolute = Settings.GetHandle<float>("LimitModeSingleRanged_Absolute", "MaximumMassSingleAbsolute_title".Translate(), "MaximumMassSingleAbsolute_desc".Translate(), 0f);
            LimitModeSingleRanged_AbsoluteMatches = Settings.GetHandle<WeaponListKind>("LimitModeSingleRanged_AbsoluteMatches", "WeaponMatch_title".Translate(), "WeaponMatch_desc".Translate(), WeaponListKind.Ranged);
            LimitModeSingleRanged_Selection = Settings.GetHandle<StringListHandler>("LimitModeSingleRanged_Selection", "SidearmSelection_title".Translate(), "SidearmSelection_desc".Translate(), null);
            LimitModeSingleRanged_Relative.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleRanged_Relative, true); };
            LimitModeSingleRanged_RelativeMatches.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_RelativeMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveRelative(rect, LimitModeSingleRanged_RelativeMatches); };
            LimitModeSingleRanged_RelativeMatches.Unsaved = true;
            LimitModeSingleRanged_Absolute.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeSingleRanged_Absolute, false, 0, maxWeightRanged); };
            LimitModeSingleRanged_AbsoluteMatches.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_AbsoluteMatches.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_passiveAbsolute(rect, LimitModeSingleRanged_AbsoluteMatches); };
            LimitModeSingleRanged_AbsoluteMatches.Unsaved = true;
            LimitModeSingleRanged_Selection.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeSingleRanged == LimitModeSingleSidearm.Selection) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged_Selection.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_active(rect, LimitModeSingleRanged_Selection, WeaponListKind.Ranged); };
            #endregion
            LimitModeAmountRanged = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmountRanged", "LimitModeAmountRanged_title".Translate(), "LimitModeAmountRanged_desc".Translate(), LimitModeAmountOfSidearms.MaximumCarryWeightOnly, null, "LimitModeAmount_option_");
            LimitModeAmountRanged.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmountRanged.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmountRanged, rect, names, forcedWidths, ExpansionMode.Vertical); };
            #region subItems
            LimitModeAmountRanged_Relative = Settings.GetHandle<float>("LimitModeAmountRanged_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmountRanged_Absolute = Settings.GetHandle<float>("LimitModeAmountRanged_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmountRanged_Slots = Settings.GetHandle<int>("LimitModeAmountRanged_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 2);
            LimitModeAmountRanged_Relative.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountRanged == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountRanged_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountRanged_Relative, true); };
            LimitModeAmountRanged_Absolute.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountRanged == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountRanged_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountRanged_Absolute, false, 0, maxCapacity); };
            LimitModeAmountRanged_Slots.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountRanged == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion

            Underline2 = Settings.GetHandle<bool>("NilC", null, "", false);
            Underline2.Unsaved = true;
            Underline2.CustomDrawer = rect => { return CustomDrawer_RighthandSideLine(rect, Underline2); };
            Underline2.CustomDrawerHeight = 3f;
            Underline2.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Allowances && SeparateModes.Value == true; };

            LimitModeAmountTotal = Settings.GetHandle<LimitModeAmountOfSidearms>("LimitModeAmountTotal", "LimitModeAmountTotal_title".Translate(), "LimitModeAmountTotal_desc".Translate(), LimitModeAmountOfSidearms.MaximumCarryWeightOnly, null, "LimitModeAmount_option_");
            LimitModeAmountTotal.CustomDrawer = rect => { string[] names = Enum.GetNames(LimitModeAmountTotal.Value.GetType()); float[] forcedWidths = new float[names.Length]; return CustomDrawer_Enumlist(LimitModeAmountTotal, rect, names, forcedWidths, ExpansionMode.Vertical); };
            #region subItems
            LimitModeAmountTotal_Relative = Settings.GetHandle<float>("LimitModeAmountTotal_Relative", "MaximumMassAmountRelative_title".Translate(), "MaximumMassAmountRelative_desc".Translate(), 0f);
            LimitModeAmountTotal_Absolute = Settings.GetHandle<float>("LimitModeAmountTotal_Absolute", "MaximumMassAmountAbsolute_title".Translate(), "MaximumMassAmountAbsolute_desc".Translate(), 0f);
            LimitModeAmountTotal_Slots = Settings.GetHandle<int>("LimitModeAmountTotal_Selection", "MaximumSlots_title".Translate(), "MaximumSlots_desc".Translate(), 2);
            LimitModeAmountTotal_Relative.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountTotal == LimitModeAmountOfSidearms.RelativeWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountTotal_Relative.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountTotal_Relative, true); };
            LimitModeAmountTotal_Absolute.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountTotal == LimitModeAmountOfSidearms.AbsoluteWeight) && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountTotal_Absolute.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, LimitModeAmountTotal_Absolute, false, 0, maxCapacity); };
            LimitModeAmountTotal_Slots.VisibilityPredicate = delegate { return (SeparateModes == true) && (LimitModeAmountTotal == LimitModeAmountOfSidearms.Slots) && (ActiveTab == OptionsTab.Allowances); };
            #endregion 

            LimitModeSingle.VisibilityPredicate = delegate { return SeparateModes.Value == false && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmount.VisibilityPredicate = delegate { return SeparateModes.Value == false && (ActiveTab == OptionsTab.Allowances); };

            LimitModeSingleMelee.VisibilityPredicate = delegate { return SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountMelee.VisibilityPredicate = delegate { return SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeSingleRanged.VisibilityPredicate = delegate { return SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountRanged.VisibilityPredicate = delegate { return SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };
            LimitModeAmountTotal.VisibilityPredicate = delegate { return SeparateModes.Value == true && (ActiveTab == OptionsTab.Allowances); };

            LimitCarryInfo = Settings.GetHandle<bool>("LimitCarryInfo", null, "LimitCarryInfo_desc".Translate(), false);
            LimitCarryInfo.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Allowances; };
            LimitCarryInfo.CustomDrawer = rect => { return CustomDrawer_RighthandSideLabel(rect, "LimitCarryInfo_title".Translate()); };
            LimitCarryInfo.Unsaved = true;
            
            SidearmSpawnChance = Settings.GetHandle<float>("SidearmSpawnChance", "SidearmSpawnChance_title".Translate(), "SidearmSpawnChance_desc".Translate(), 0f);
            SidearmSpawnChance.CustomDrawer = rect => { return CustomDrawer_FloatSlider(rect, SidearmSpawnChance, true); };
            SidearmSpawnChance.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };

            SidearmsEnableNeolithicExtension = Settings.GetHandle<bool>("SidearmsEnableNeolithicExtension", "SidearmsEnableNeolithicExtension_title".Translate(), "SidearmsEnableNeolithicExtension_desc".Translate(), false);
            SidearmsEnableNeolithicExtension.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning; };
            SidearmsNeolithicExtension = Settings.GetHandle<StringListHandler>("SidearmsNeolithicExtension", "SidearmsNeolithicExtension_title".Translate(), "SidearmsNeolithicExtension_desc".Translate(), null);
            SidearmsNeolithicExtension.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Spawning && SidearmsEnableNeolithicExtension.Value == true; };
            SidearmsNeolithicExtension.CustomDrawer = rect => { return SettingsUIs.CustomDrawer_MatchingWeapons_active(rect, SidearmsNeolithicExtension, WeaponListKind.Both,"Tribal","Not tribal", true); };

            DropMode = Settings.GetHandle<DroppingModeOptionsEnum>("DropMode", "DropMode_title".Translate(), "DropMode_desc".Translate(), DroppingModeOptionsEnum.Never, null, "DropMode_option_");
            DropMode.CustomDrawer = rect => {
                string[] names = Enum.GetNames(DropMode.Value.GetType());
                float[] forcedWidths = new float[names.Length];
                return CustomDrawer_Enumlist(DropMode, rect, names, forcedWidths, ExpansionMode.Vertical);
            };
            DropMode.VisibilityPredicate = delegate { return ActiveTab == OptionsTab.Misc; }; 

            if(ActivePreset.Value == Preset.NoneApplied)
            {
                Presets.presetChanged(Preset.Basic);
            }
        }

        public static void ResetSettings()
        {

            CQCAutoSwitch.ResetToDefault();
            CQCFistSwitch.ResetToDefault();
            CQCTargetOnly.ResetToDefault();
            Underline0.ResetToDefault();
            RangedCombatAutoSwitch.ResetToDefault();
            RangedCombatAutoSwitchMaxWarmup.ResetToDefault();
            SingleshotAutoSwitch.ResetToDefault();

            SpeedSelectionBiasMelee.ResetToDefault();
            SpeedSelectionBiasRanged.ResetToDefault();

            LimitCarryInfo.ResetToDefault();

            SeparateModes.ResetToDefault();
            LimitModeSingle.ResetToDefault();
            LimitModeAmount.ResetToDefault();

            LimitModeSingleMelee.ResetToDefault();
            LimitModeAmountMelee.ResetToDefault();
            Underline1.ResetToDefault();
            LimitModeSingleRanged.ResetToDefault();
            LimitModeAmountRanged.ResetToDefault();
            Underline2.ResetToDefault();
            LimitModeAmountTotal.ResetToDefault();

        #region LimitModeSingle
            LimitModeSingle_Relative.ResetToDefault();
            LimitModeSingle_RelativeMatches.ResetToDefault();
            LimitModeSingle_Absolute.ResetToDefault();
            LimitModeSingle_AbsoluteMatches.ResetToDefault();
            LimitModeSingle_Selection.ResetToDefault();
        #endregion
        #region LimitModeAmount
            LimitModeAmount_Relative.ResetToDefault();
            LimitModeAmount_Absolute.ResetToDefault();
            LimitModeAmount_Slots.ResetToDefault();
        #endregion

        #region LimitModeSingleMelee
            LimitModeSingleMelee_Relative.ResetToDefault();
            LimitModeSingleMelee_RelativeMatches.ResetToDefault();
            LimitModeSingleMelee_Absolute.ResetToDefault();
            LimitModeSingleMelee_AbsoluteMatches.ResetToDefault();
            LimitModeSingleMelee_Selection.ResetToDefault();
        #endregion
        #region LimitModeAmountMelee
            LimitModeAmountMelee_Relative.ResetToDefault();
            LimitModeAmountMelee_Absolute.ResetToDefault();
            LimitModeAmountMelee_Slots.ResetToDefault();
        #endregion
        #region LimitModeSingleRanged
            LimitModeSingleRanged_Relative.ResetToDefault();
            LimitModeSingleRanged_RelativeMatches.ResetToDefault();
            LimitModeSingleRanged_Absolute.ResetToDefault();
            LimitModeSingleRanged_AbsoluteMatches.ResetToDefault();
            LimitModeSingleRanged_Selection.ResetToDefault();
        #endregion
        #region LimitModeAmountRanged
            LimitModeAmountRanged_Relative.ResetToDefault();
            LimitModeAmountRanged_Absolute.ResetToDefault();
            LimitModeAmountRanged_Slots.ResetToDefault();
        #endregion
        #region LimitModeAmountTotal
            LimitModeAmountTotal_Relative.ResetToDefault();
            LimitModeAmountTotal_Absolute.ResetToDefault();
            LimitModeAmountTotal_Slots.ResetToDefault();
        #endregion

            SidearmSpawnChance.ResetToDefault();
            SidearmsEnableNeolithicExtension.ResetToDefault();
            SidearmsNeolithicExtension.ResetToDefault();

            DropMode.ResetToDefault();
    }

    public override void Initialize()
        {
            base.Initialize();
        }
    }
}
