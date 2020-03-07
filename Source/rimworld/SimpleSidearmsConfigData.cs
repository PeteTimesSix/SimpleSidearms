using HugsLib.Utils;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    public class SimpleSidearmsConfigData : WorldComponent
    {
        public Dictionary<int, GoldfishModule> memories = null;


        public SimpleSidearmsConfigData(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Scribe_Collections.Look<int, GoldfishModule>(ref memories, "memories", LookMode.Value, LookMode.Deep);
                if (memories != null && memories.Count < 1)
                    memories = null;
            }
            else
            {
                Dictionary<int, GoldfishModule> no_memories = new Dictionary<int, GoldfishModule>();
                Scribe_Collections.Look<int, GoldfishModule>(ref no_memories, "memories", LookMode.Value, LookMode.Deep);
            }
        }
    }
}
