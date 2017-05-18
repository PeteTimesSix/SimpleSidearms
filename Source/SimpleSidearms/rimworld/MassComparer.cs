using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSidearms.rimworld
{
    class MassComparer : IComparer<ThingStuffPair>
    {
        public int Compare(ThingStuffPair x, ThingStuffPair y)
        {
            return x.thing.GetStatValueAbstract(StatDefOf.Mass, x.stuff).CompareTo(y.thing.GetStatValueAbstract(StatDefOf.Mass, y.stuff));
        }
    }
}
