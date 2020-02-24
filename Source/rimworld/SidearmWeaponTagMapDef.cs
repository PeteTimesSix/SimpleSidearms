using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld
{
    class SidearmWeaponTagMapDef : Def
    {
        public string sourceTag;

        public List<string> resultTags = new List<string>();
    }
}
