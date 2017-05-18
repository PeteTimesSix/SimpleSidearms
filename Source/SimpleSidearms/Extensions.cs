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
