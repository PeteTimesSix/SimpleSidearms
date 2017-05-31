using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SimpleSidearms
{
    public static class Extensions
    {
        public static bool hasWeaponSomewhere(this Pawn pawn, string wepName)
        {
            if (pawn == null)
                Log.Warning("got handed null pawn");
            if (pawn.equipment != null)
                if (pawn.equipment.Primary != null)
                    if (pawn.equipment.Primary.def.defName.Equals(wepName))
                        return true;

            if (pawn.inventory != null)
            {
                if (pawn.inventory.innerContainer != null)
                {
                    foreach (Thing thing in pawn.inventory.innerContainer)
                    {
                        if (thing.def.defName.Equals(wepName))
                            return true;
                    }
                }
            }
            return false;
        }

        public static bool hasWeaponSomewhere(this Pawn pawn, ThingDef wepDef)
        {
            if (pawn == null)
                Log.Warning("got handed null pawn");
            if (pawn.equipment != null)
                if (pawn.equipment.Primary != null)
                    if (pawn.equipment.Primary.def.Equals(wepDef))
                        return true;

            if (pawn.inventory != null)
            {
                if (pawn.inventory.innerContainer != null)
                {
                    foreach (Thing thing in pawn.inventory.innerContainer)
                    {
                        if (thing.def.Equals(wepDef))
                            return true;
                    }
                }
            }
            return false;
        }

        public static bool Contains(this Rect rect, Rect otherRect)
        {
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMin)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMin)))
                return false;
            return true;
        }

        public static bool ButtonText(Rect rect, string label, Color color, bool drawBackground = true, bool doMouseoverSound = false, bool active = true)
        {
            return Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, color, active);
        }
    }
}
