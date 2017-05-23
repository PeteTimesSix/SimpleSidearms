using HugsLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    public class SimpleSidearmsData : UtilityWorldObject
    {
        public Dictionary<int, SwapControlsHandler> handlers = new Dictionary<int, SwapControlsHandler>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<int, SwapControlsHandler>(ref handlers, "handlers", LookMode.Value, LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (handlers == null) handlers = new Dictionary<int, SwapControlsHandler>();
            }
        }
    }
}
