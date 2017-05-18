using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSidearms.hugsLibSettings
{
    internal class StringListHandler : SettingHandleConvertible
    {
        private List<string> strings = new List<string>();
        public List<string> InnerList { get { return strings; } set { strings = value; } }

        public override void FromString(string settingValue)
        {
            strings = settingValue.Split('|').ToList();
        }

        public override string ToString()
        {
            return strings != null ? String.Join("|", strings.ToArray()) : "";
        }
    }
}
