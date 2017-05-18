using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSidearms.rimworld
{
    internal class ThingStuffPairComparer : IEqualityComparer<ThingStuffPair>
    {
        public bool Equals(ThingStuffPair x, ThingStuffPair y)
        {
            if (x.thing.Equals(y.thing) & x.stuff.Equals(y.stuff))
                return true;
            else
                return false;
        }

        public int GetHashCode(ThingStuffPair obj)
        {
            return obj.stuff.GetHashCode()+ obj.thing.GetHashCode();
        }
    }
}
