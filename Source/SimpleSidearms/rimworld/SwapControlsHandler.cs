using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    [StaticConstructorOnStartup]
    public class SwapControlsHandler : IExposable
    {
        internal bool currentWeaponLocked = false;
        internal bool autoLockOnManualSwap = true;

        public Pawn Owner { get; set; }

        public SwapControlsHandler() : this(null) { }

        public SwapControlsHandler(Pawn owner)
        {
            Owner = owner;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref currentWeaponLocked, "currentWeaponLocked", false, true);
            Scribe_Values.Look<bool>(ref autoLockOnManualSwap, "autoLockOnManualSwap", true, true);
        }

        public static SwapControlsHandler GetHandlerForPawn(Pawn pawn)
        {
            if (SimpleSidearms.saveData == null) throw new Exception("Cannot get handler- saveData not loaded");
            var pawnId = pawn.thingIDNumber;
            SwapControlsHandler handler;
            if (!SimpleSidearms.saveData.handlers.TryGetValue(pawnId, out handler))
            {
                handler = new SwapControlsHandler(pawn);
                SimpleSidearms.saveData.handlers.Add(pawnId, handler);
            }
            return handler;
        }
    }
}
