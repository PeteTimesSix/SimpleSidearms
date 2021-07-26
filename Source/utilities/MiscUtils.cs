using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms.Utilities
{
    public static class MiscUtils
    {
        public static readonly float ANTI_OSCILLATION_FACTOR = 0.1f;

        public static bool shouldDrop(DroppingModeEnum mode)
        {
            switch (Settings.DropMode)
            {
                case DroppingModeOptionsEnum.Never:
                    return false;
                case DroppingModeOptionsEnum.InDistress:
                    if (mode == DroppingModeEnum.InDistress)
                        return true;
                    else
                        return false;
                case DroppingModeOptionsEnum.InCombat:
                    if (mode == DroppingModeEnum.InDistress || mode == DroppingModeEnum.Combat)
                        return true;
                    else
                        return false;
                case DroppingModeOptionsEnum.Always:
                default:
                    return true;
            }
        }

        public static void DoNothing()
        {
        }
    }

}
