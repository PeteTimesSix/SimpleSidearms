using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSidearms
{
    public static class Globals
    {
        internal enum RangeCategory : byte{ Touch, Short, Medium, Long }
        

        internal enum DroppingModeEnum { InDistress, Combat, Calm, UsedUp }
        internal enum DroppingModeOptionsEnum { Never, InDistress, InCombat, Always }

        internal enum WeaponSearchType { Both, Ranged, Melee, MeleeCapable }

        internal enum LimitModeSingleSidearm { None, RelativeWeight, AbsoluteWeight, Selection }
        internal enum LimitModeAmountOfSidearms { MaximumCarryWeightOnly, RelativeWeight, AbsoluteWeight, Slots }

        internal enum Preset { NoneApplied, Custom, Lite, Disabled, LoadoutOnly, Basic, Advanced, Excessive, Brawler };

        internal static readonly float ANTI_OSCILLATION_FACTOR = 0.1f;
    }
}
