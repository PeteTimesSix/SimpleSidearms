using LudeonTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Rimworld
{
    [StaticConstructorOnStartup]
    public static class DebugMenuEntries
    {
        private const string CATEGORY = "Simple Sidearms";

        [DebugAction(category = CATEGORY, actionType = DebugActionType.Action)]
        static void ToggleBrainscope()
        {
            SimpleSidearms.Settings.ShowBrainscope = !SimpleSidearms.Settings.ShowBrainscope;
        }
    }
}
