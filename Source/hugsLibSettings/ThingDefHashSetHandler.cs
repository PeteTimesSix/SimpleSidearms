using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.hugsLibSettings
{
    public class ThingDefHashSetHandler : SettingHandleConvertible
    {
        private HashSet<ThingDef> defs = new HashSet<ThingDef>();
        public HashSet<ThingDef> InnerList { get { return defs; } set { defs = value; } }

        public override void FromString(string settingValue)
        {
            defs = new HashSet<ThingDef>();
            if (!settingValue.Equals(string.Empty))
            {
                foreach (string str in settingValue.Split('|'))
                {
                    defs.Add(DefDatabase<ThingDef>.GetNamed(str));
                }
            }
        }

        public override string ToString()
        {
            return defs != null ? String.Join("|", defs.ToList().ConvertAll(t => t.defName).ToArray()) : "";
        }
    }
}
