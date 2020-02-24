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
using static SimpleSidearms.hugsLibSettings.SettingsUIs;
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

        internal static void DoNothing()
        {
        }

        internal static WeaponSearchType LimitTypeToListType(WeaponListKind type)
        {
            switch (type)
            {
                case WeaponListKind.Melee:
                    return WeaponSearchType.Melee;
                case WeaponListKind.Ranged:
                    return WeaponSearchType.Ranged;
                case WeaponListKind.Both:
                default:
                    return WeaponSearchType.Both;
            }
        }
    }

}
