using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static SimpleSidearms.Globals;
using static SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.utilities
{
    public static class MiscUtils
    {

        internal static bool shouldDrop(DroppingModeEnum mode)
        {
            switch (SimpleSidearms.DropMode.Value)
            {
                case DroppingModeOptionsEnum.Never:
                    return false;
                case DroppingModeOptionsEnum.Panic:
                    if (mode == DroppingModeEnum.Panic)
                        return true;
                    else
                        return false;
                case DroppingModeOptionsEnum.PanicOrRange:
                    if (mode == DroppingModeEnum.Panic || mode == DroppingModeEnum.Range)
                        return true;
                    else
                        return false;
                case DroppingModeOptionsEnum.Always:
                default:
                    return true;
            }
        }

        internal static void DoNothing()
        {
        }
    }

}
