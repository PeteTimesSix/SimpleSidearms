using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    public struct ThingStuffPairExposable : IExposable, IEquatable<ThingStuffPair>, IEquatable<ThingStuffPairExposable>
    {
        private ThingDef thing;
        private ThingDef stuff;
        public ThingStuffPair Val { get { return new ThingStuffPair(thing, stuff); } }

        public ThingStuffPairExposable(ThingStuffPair val)
        {
            this.thing = val.thing;
            this.stuff = val.stuff;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look<ThingDef>(ref this.thing, "thing");
            Scribe_Defs.Look<ThingDef>(ref this.stuff, "stuff");
        }

        public static bool operator ==(ThingStuffPairExposable a, ThingStuffPairExposable b)
        {
            return a.Val == b.Val;
        }

        public static bool operator !=(ThingStuffPairExposable a, ThingStuffPairExposable b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Val.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj is ThingStuffPairExposable)
                return this == (ThingStuffPairExposable)obj;
            if (obj is ThingStuffPair)
                return this.Val == (ThingStuffPair)obj;
            return false;
        }

        public bool Equals(ThingStuffPair other)
        {
            return ((IEquatable<ThingStuffPair>)Val).Equals(other);
        }

        public bool Equals(ThingStuffPairExposable other)
        {
            return this.Val == other.Val;
        }
    }
}
