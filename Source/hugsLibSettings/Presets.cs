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
    public static class Presets
    {
        public static void presetChanged(Globals.Preset preset, SimpleSidearms mod)
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
@"<PeteTimesSix.SimpleSidearms>
  <ActivePreset>Custom</ActivePreset>
  <LimitModeAmount>Slots</LimitModeAmount>
  <SidearmSpawnChance>0</SidearmSpawnChance>
  <SidearmSpawnChanceDropoff>1</SidearmSpawnChanceDropoff>
  <SidearmBudgetMultiplier>0</SidearmBudgetMultiplier>
  <SidearmBudgetDropoff>1</SidearmBudgetDropoff>
</PeteTimesSix.SimpleSidearms>";

        public static string Preset_Lite =
@"<PeteTimesSix.SimpleSidearms>
  <ActivePreset>Custom</ActivePreset>
  <ToolAutoSwitch>True</ToolAutoSwitch>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <RangedCombatAutoSwitch>True</RangedCombatAutoSwitch>
  <SeparateModes>True</SeparateModes>
  <LimitModeAmount>Slots</LimitModeAmount>
  <LimitModeSingleMelee>AbsoluteWeight</LimitModeSingleMelee>
  <LimitModeSingleMelee_Absolute>0.6</LimitModeSingleMelee_Absolute>
  <LimitModeAmountMelee>Slots</LimitModeAmountMelee>
  <LimitModeAmountMelee_Selection>2</LimitModeAmountMelee_Selection>
  <LimitModeSingleRanged>AbsoluteWeight</LimitModeSingleRanged>
  <LimitModeSingleRanged_Absolute>2.55</LimitModeSingleRanged_Absolute>
  <LimitModeAmountRanged>Slots</LimitModeAmountRanged>
  <LimitModeAmountRanged_Selection>2</LimitModeAmountRanged_Selection>
  <LimitModeAmountTotal>Slots</LimitModeAmountTotal>
  <LimitModeAmountTotal_Selection>2</LimitModeAmountTotal_Selection>
  <SidearmSpawnChanceDropoff>1</SidearmSpawnChanceDropoff>
  <SidearmBudgetDropoff>1</SidearmBudgetDropoff>
  <DropMode>InDistress</DropMode>
</PeteTimesSix.SimpleSidearms>";

        public static string Preset_LoadoutOnly =
@"<PeteTimesSix.SimpleSidearms>
  <ActivePreset>Custom</ActivePreset>
  <LimitModeSingle>AbsoluteWeight</LimitModeSingle>
  <LimitModeSingle_Absolute>4.75</LimitModeSingle_Absolute>
  <LimitModeAmount>Slots</LimitModeAmount>
  <LimitModeAmount_Selection>3</LimitModeAmount_Selection>
  <SidearmSpawnChance>0</SidearmSpawnChance>
  <SidearmSpawnChanceDropoff>1</SidearmSpawnChanceDropoff>
  <SidearmBudgetMultiplier>0</SidearmBudgetMultiplier>
  <SidearmBudgetDropoff>1</SidearmBudgetDropoff>
</PeteTimesSix.SimpleSidearms>";

        public static string Preset_Basic =
@"<PeteTimesSix.SimpleSidearms>
  <ActivePreset>Custom</ActivePreset>
  <ToolAutoSwitch>True</ToolAutoSwitch>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <RangedCombatAutoSwitch>True</RangedCombatAutoSwitch>
  <SeparateModes>True</SeparateModes>
  <LimitModeSingle>RelativeWeight</LimitModeSingle>
  <LimitModeAmount>Slots</LimitModeAmount>
  <LimitModeSingleMelee>AbsoluteWeight</LimitModeSingleMelee>
  <LimitModeSingleMelee_Absolute>1.9</LimitModeSingleMelee_Absolute>
  <LimitModeSingleMelee_Selection></LimitModeSingleMelee_Selection>
  <LimitModeAmountMelee>Slots</LimitModeAmountMelee>
  <LimitModeAmountMelee_Selection>2</LimitModeAmountMelee_Selection>
  <LimitModeSingleRanged>AbsoluteWeight</LimitModeSingleRanged>
  <LimitModeSingleRanged_Absolute>2.55</LimitModeSingleRanged_Absolute>
  <LimitModeAmountRanged>Slots</LimitModeAmountRanged>
  <LimitModeAmountRanged_Selection>2</LimitModeAmountRanged_Selection>
  <LimitModeAmountTotal>Slots</LimitModeAmountTotal>
  <LimitModeAmountTotal_Selection>3</LimitModeAmountTotal_Selection>
  <DropMode>InDistress</DropMode>
</PeteTimesSix.SimpleSidearms>";

        public static string Preset_Advanced =
@"<PeteTimesSix.SimpleSidearms>
  <ActivePreset>Custom</ActivePreset>
  <ToolAutoSwitch>True</ToolAutoSwitch>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <RangedCombatAutoSwitch>True</RangedCombatAutoSwitch>
  <SeparateModes>True</SeparateModes>
  <LimitModeSingle>RelativeWeight</LimitModeSingle>
  <LimitModeAmount>Slots</LimitModeAmount>
  <LimitModeSingleMelee>AbsoluteWeight</LimitModeSingleMelee>
  <LimitModeSingleMelee_Absolute>2.25</LimitModeSingleMelee_Absolute>
  <LimitModeSingleRanged>AbsoluteWeight</LimitModeSingleRanged>
  <LimitModeSingleRanged_Absolute>5.0</LimitModeSingleRanged_Absolute>
  <LimitModeAmountTotal>AbsoluteWeight</LimitModeAmountTotal>
  <LimitModeAmountTotal_Absolute>10</LimitModeAmountTotal_Absolute>
  <DropMode>InDistress</DropMode>
</PeteTimesSix.SimpleSidearms>";

        public static string Preset_Excessive =
@"<PeteTimesSix.SimpleSidearms>
  <ActivePreset>Custom</ActivePreset>
  <ToolAutoSwitch>True</ToolAutoSwitch>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <RangedCombatAutoSwitch>True</RangedCombatAutoSwitch>
  <SidearmSpawnChance>0.75</SidearmSpawnChance>
  <SidearmSpawnChanceDropoff>0.5</SidearmSpawnChanceDropoff>
  <SidearmBudgetMultiplier>0.75</SidearmBudgetMultiplier>
  <SidearmBudgetDropoff>0.5</SidearmBudgetDropoff>
  <DropMode>InDistress</DropMode>
</PeteTimesSix.SimpleSidearms>";

        public static string Preset_Brawler =
 @"<PeteTimesSix.SimpleSidearms>
  <ActivePreset>Custom</ActivePreset>
  <ToolAutoSwitch>True</ToolAutoSwitch>
  <OptimalMelee>True</OptimalMelee>
  <CQCAutoSwitch>True</CQCAutoSwitch>
  <SeparateModes>True</SeparateModes>
  <LimitModeSingle>RelativeWeight</LimitModeSingle>
  <LimitModeAmount>Slots</LimitModeAmount>
  <LimitModeSingleMelee>AbsoluteWeight</LimitModeSingleMelee>
  <LimitModeSingleMelee_Absolute>4</LimitModeSingleMelee_Absolute>
  <LimitModeSingleMelee_Selection></LimitModeSingleMelee_Selection>
  <LimitModeAmountMelee>AbsoluteWeight</LimitModeAmountMelee>
  <LimitModeAmountMelee_Absolute>10</LimitModeAmountMelee_Absolute>
  <LimitModeAmountMelee_Selection>2</LimitModeAmountMelee_Selection>
  <LimitModeSingleRanged>Selection</LimitModeSingleRanged>
  <LimitModeSingleRanged_Selection></LimitModeSingleRanged_Selection>
  <LimitModeAmountRanged>Slots</LimitModeAmountRanged>
  <LimitModeAmountTotal_Selection>1</LimitModeAmountTotal_Selection>
  <DropMode>InDistress</DropMode>
</PeteTimesSix.SimpleSidearms>";

    }
}
