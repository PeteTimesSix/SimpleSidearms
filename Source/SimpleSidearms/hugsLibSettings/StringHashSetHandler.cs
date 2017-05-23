using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSidearms.hugsLibSettings
{
    internal class StringHashSetHandler : SettingHandleConvertible
    {
        private HashSet<string> strings = new HashSet<string>();
        public HashSet<string> InnerList { get { return strings; } set { strings = value; } }

        public override void FromString(string settingValue)
        {
            strings = new HashSet<string>();
            if (!settingValue.Equals(string.Empty))
            {
                foreach (string str in settingValue.Split('|'))
                {
                    strings.Add(str);
                }
            }
        }

        public override string ToString()
        {
            return strings != null ? String.Join("|", strings.ToArray()) : "";
        }
    }
}
