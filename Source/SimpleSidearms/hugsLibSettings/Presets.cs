using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib.Settings;
using System.Xml.Linq;
using static SimpleSidearms.Globals;
using HugsLib;
using Verse;

namespace SimpleSidearms.hugsLibSettings
{
    static class Presets
    {
        internal static void presetChanged(Globals.Preset preset, SimpleSidearms mod)
        {
            switch (preset)
            {
                case Globals.Preset.Custom:
                    SimpleSidearms.ActivePreset.Value = preset;
                    return;
                case Globals.Preset.Disabled:
                    mod.UpdateConfig(Preset_Disabled, true);
                    break;
                case Globals.Preset.Lite:
                    mod.UpdateConfig(Preset_Lite, true);
                    break;
                case Globals.Preset.LoadoutOnly:
                    mod.UpdateConfig(Preset_LoadoutOnly, true);
                    break;
                case Globals.Preset.Basic:
                    mod.UpdateConfig(Preset_Basic, true);
                    break;
                case Globals.Preset.Advanced:
                    mod.UpdateConfig(Preset_Advanced, true);
                    break;
                case Globals.Preset.Excessive:
                    mod.UpdateConfig(Preset_Excessive, true);
                    break;
                case Globals.Preset.Brawler:
                    mod.UpdateConfig(Preset_Brawler, true);
                    break;
            }
            SimpleSidearms.ActivePreset.Value = preset;
            SimpleSidearms.ActiveTab.Value = SimpleSidearms.OptionsTab.Presets;
            HugsLibController.SettingsManager.SaveChanges();
        }

        public static string Preset_Disabled =
@"<SimpleSidearms>
  <ActivePreset>Disabled</ActivePreset>
  <LimitModeSingle>RelativeWeight</LimitModeSingle>
  <LimitModeAmount>RelativeWeight</LimitModeAmount>
</SimpleSidearms>";

        public static string Preset_Lite =
@"<SimpleSidearms>
  <ActivePreset>Lite</ActivePreset>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <SpeedSelectionBiasMelee>1.25</SpeedSelectionBiasMelee>
  <SeparateModes>True</SeparateModes>
  <LimitModeSingle>RelativeWeight</LimitModeSingle>
  <LimitModeAmount>RelativeWeight</LimitModeAmount>
  <LimitModeSingleMelee_Selection></LimitModeSingleMelee_Selection>
  <LimitModeAmountMelee>Slots</LimitModeAmountMelee>
  <LimitModeAmountMelee_Selection>1</LimitModeAmountMelee_Selection>
  <LimitModeAmountRanged>Slots</LimitModeAmountRanged>
  <SidearmSpawnChance>0.5</SidearmSpawnChance>
  <SidearmsEnableNeolithicExtension>True</SidearmsEnableNeolithicExtension>
  <SidearmsNeolithicExtension>MeleeWeapon_Shiv</SidearmsNeolithicExtension>
</SimpleSidearms>";

        public static string Preset_LoadoutOnly =
@"<SimpleSidearms>
  <ActivePreset>LoadoutOnly</ActivePreset>
  <SingleshotAutoSwitch>True</SingleshotAutoSwitch>
  <SeparateModes>True</SeparateModes>
  <SpeedSelectionBiasRanged>1.25</SpeedSelectionBiasRanged>
  <LimitModeSingleMelee>Selection</LimitModeSingleMelee>
  <LimitModeSingleMelee_Selection>MeleeWeapon_Shiv|MeleeWeapon_Knife</LimitModeSingleMelee_Selection>
  <LimitModeAmountMelee>Slots</LimitModeAmountMelee>
  <LimitModeAmountMelee_Selection>1</LimitModeAmountMelee_Selection>
  <LimitModeSingleRanged>Selection</LimitModeSingleRanged>
  <LimitModeSingleRanged_Selection>Weapon_GrenadeFrag|Weapon_GrenadeEMP|Weapon_GrenadeMolotov|Gun_Pistol|Gun_MachinePistol|Gun_HeavySMG</LimitModeSingleRanged_Selection>
  <LimitModeAmountRanged>Slots</LimitModeAmountRanged>
  <LimitModeAmountRanged_Selection>1</LimitModeAmountRanged_Selection>
  <LimitModeAmountTotal>Slots</LimitModeAmountTotal>
  <LimitModeAmountTotal_Selection>2</LimitModeAmountTotal_Selection>
</SimpleSidearms>";

        public static string Preset_Basic =
@"<SimpleSidearms>
  <ActivePreset>Basic</ActivePreset>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <SpeedSelectionBiasMelee>1.25</SpeedSelectionBiasMelee>
  <RangedCombatAutoSwitch>True</RangedCombatAutoSwitch>
  <RangedCombatAutoSwitchMaxWarmup>0.5</RangedCombatAutoSwitchMaxWarmup>
  <SingleshotAutoSwitch>True</SingleshotAutoSwitch>
  <SpeedSelectionBiasRanged>1.25</SpeedSelectionBiasRanged>
  <LimitModeSingle>AbsoluteWeight</LimitModeSingle>
  <LimitModeSingle_Absolute>3.35</LimitModeSingle_Absolute>
  <LimitModeAmount>Slots</LimitModeAmount>
  <LimitModeAmount_Selection>2</LimitModeAmount_Selection>
  <SidearmSpawnChance>0.5</SidearmSpawnChance>
  <SidearmsEnableNeolithicExtension>True</SidearmsEnableNeolithicExtension>
  <SidearmsNeolithicExtension>MeleeWeapon_Shiv</SidearmsNeolithicExtension>
  <DropMode>Panic</DropMode>
  <ReEquipBest>True</ReEquipBest>
</SimpleSidearms>
";

        public static string Preset_Advanced =
@"<SimpleSidearms>
  <ActivePreset>Advanced</ActivePreset>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <SpeedSelectionBiasMelee>1.25</SpeedSelectionBiasMelee>
  <RangedCombatAutoSwitch>True</RangedCombatAutoSwitch>
  <RangedCombatAutoSwitchMaxWarmup>0.5</RangedCombatAutoSwitchMaxWarmup>
  <SingleshotAutoSwitch>True</SingleshotAutoSwitch>
  <SpeedSelectionBiasRanged>1.25</SpeedSelectionBiasRanged>
  <LimitModeSingle>AbsoluteWeight</LimitModeSingle>
  <LimitModeSingle_Absolute>3.9</LimitModeSingle_Absolute>
  <LimitModeAmount>RelativeWeight</LimitModeAmount>
  <LimitModeAmount_Relative>0.15</LimitModeAmount_Relative>
  <SidearmSpawnChance>0.5</SidearmSpawnChance>
  <SidearmsEnableNeolithicExtension>True</SidearmsEnableNeolithicExtension>
  <SidearmsNeolithicExtension>MeleeWeapon_Shiv</SidearmsNeolithicExtension>
  <DropMode>Panic</DropMode>
  <ReEquipBest>True</ReEquipBest>
</SimpleSidearms>
";

        public static string Preset_Excessive =
@"<SimpleSidearms>
  <ActivePreset>Excessive</ActivePreset>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <SpeedSelectionBiasMelee>1.25</SpeedSelectionBiasMelee>
  <RangedCombatAutoSwitch>True</RangedCombatAutoSwitch>
  <RangedCombatAutoSwitchMaxWarmup>0.5</RangedCombatAutoSwitchMaxWarmup>
  <SingleshotAutoSwitch>True</SingleshotAutoSwitch>
  <SidearmSpawnChance>1</SidearmSpawnChance>
  <SidearmsEnableNeolithicExtension>True</SidearmsEnableNeolithicExtension>
  <SidearmsNeolithicExtension>MeleeWeapon_Shiv|MeleeWeapon_Knife|MeleeWeapon_Gladius|MeleeWeapon_Mace|MeleeWeapon_LongSword</SidearmsNeolithicExtension>
  <DropMode>Panic</DropMode>
  <ReEquipBest>True</ReEquipBest>
</SimpleSidearms>";

        public static string Preset_Brawler =
 @"<SimpleSidearms>
  <ActivePreset>Brawler</ActivePreset>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <SpeedSelectionBiasMelee>1.25</SpeedSelectionBiasMelee>
  <SeparateModes>True</SeparateModes>
  <LimitModeAmountMelee>RelativeWeight</LimitModeAmountMelee>
  <LimitModeAmountMelee_Relative>0.15</LimitModeAmountMelee_Relative>
  <LimitModeAmountRanged>Slots</LimitModeAmountRanged>
  <LimitModeAmountRanged_Selection>0</LimitModeAmountRanged_Selection>
  <SidearmSpawnChance>1</SidearmSpawnChance>
  <SidearmsEnableNeolithicExtension>True</SidearmsEnableNeolithicExtension>
  <SidearmsNeolithicExtension>MeleeWeapon_Shiv|MeleeWeapon_Knife|MeleeWeapon_Gladius|MeleeWeapon_Mace|MeleeWeapon_LongSword</SidearmsNeolithicExtension>
  <DropMode>Panic</DropMode>
  <ReEquipBest>True</ReEquipBest>
</SimpleSidearms>";

    }
}
