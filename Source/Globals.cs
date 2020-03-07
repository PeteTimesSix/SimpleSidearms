using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSidearms
{
    public static class Globals
    {
        public enum RangeCategory : byte{ Touch, Short, Medium, Long }


        public enum PrimaryWeaponMode { Ranged, Melee, BySkill, ByGenerated }

        public enum DroppingModeEnum { InDistress, Combat, Calm, UsedUp }
        public enum DroppingModeOptionsEnum { Never, InDistress, InCombat, Always }

        public enum WeaponSearchType { Both, Ranged, Melee, MeleeCapable }

        public enum LimitModeSingleSidearm { None, RelativeWeight, AbsoluteWeight, Selection }
        public enum LimitModeAmountOfSidearms { MaximumCarryWeightOnly, RelativeWeight, AbsoluteWeight, Slots }

        public enum Preset { NoneApplied, Custom, Lite, Disabled, LoadoutOnly, Basic, Advanced, Excessive, Brawler };

        public static readonly float ANTI_OSCILLATION_FACTOR = 0.1f;
    }
}
