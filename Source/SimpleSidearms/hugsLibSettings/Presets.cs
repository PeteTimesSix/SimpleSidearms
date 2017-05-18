using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib.Settings;
using System.Xml.Linq;
using static SimpleSidearms.Globals;
using HugsLib;

namespace SimpleSidearms.hugsLibSettings
{
    static class Presets
    {
        internal static void presetChanged(Globals.Preset preset)
        {
            SimpleSidearms.ResetSettings();
            switch (preset)
            {
                case Globals.Preset.Basic:
                    BasicPreset();
                    break;
                case Globals.Preset.LoadoutOnly:
                    LoadoutOnlyPreset();
                    break;
            }
            SimpleSidearms.ActivePreset.Value = preset;
            HugsLibController.SettingsManager.SaveChanges();
        }

        private static void BasicPreset()
        {
            SimpleSidearms.CQCAutoSwitch.Value = true;
            SimpleSidearms.SpeedSelectionBiasMelee.Value = 1.25f;

            SimpleSidearms.RangedCombatAutoSwitch.Value = true;
            SimpleSidearms.SpeedSelectionBiasRanged.Value = 1.25f;

            SimpleSidearms.SingleshotAutoSwitch.Value = true;

            SimpleSidearms.LimitModeSingle.Value = Globals.LimitModeSingleSidearm.AbsoluteWeight;
            SimpleSidearms.LimitModeSingle_Absolute.Value = 3.6f;
            SimpleSidearms.LimitModeAmount.Value = Globals.LimitModeAmountOfSidearms.RelativeWeight;
            SimpleSidearms.LimitModeAmount_Relative.Value = 0.25f;

            SimpleSidearms.SidearmSpawnChance.Value = 0.75f;

            SimpleSidearms.SidearmsEnableNeolithicExtension.Value = true;
            SimpleSidearms.SidearmsNeolithicExtension.Value = new StringListHandler();
            SimpleSidearms.SidearmsNeolithicExtension.Value.InnerList.Add("MeleeWeapon_Shiv");
            SimpleSidearms.SidearmsNeolithicExtension.Value.InnerList.Add("MeleeWeapon_Gladius");

            SimpleSidearms.DropMode.Value = Globals.DroppingModeOptionsEnum.Panic;
        }

        private static void LoadoutOnlyPreset()
        {
            SimpleSidearms.SingleshotAutoSwitch.Value = true;

            SimpleSidearms.LimitModeSingle.Value = Globals.LimitModeSingleSidearm.AbsoluteWeight;
            SimpleSidearms.LimitModeSingle_Absolute.Value = 3.6f;
            SimpleSidearms.LimitModeAmount.Value = Globals.LimitModeAmountOfSidearms.RelativeWeight;
            SimpleSidearms.LimitModeAmount_Relative.Value = 0.25f;

            SimpleSidearms.SidearmSpawnChance.Value = 0.75f;

            SimpleSidearms.DropMode.Value = Globals.DroppingModeOptionsEnum.Never;
        }

    }
}
