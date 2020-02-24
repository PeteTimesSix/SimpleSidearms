using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Verse;

namespace SimpleSidearms
{
    static class UnsafeUtilities
    {
        internal static void ExportConfigViaReflection(SimpleSidearms mod)
        {
            ModSettingsPack pack = HugsLibController.SettingsManager.GetModSettings(mod.ModIdentifier);
            if (pack == null)
                return;
            XElement root = new XElement("root");
            pack.CallByReflection("WriteXml", root);
            //should only have one
            Log.Message("Exporting current config!");
            StringBuilder builder = new StringBuilder();
            foreach(XElement child in root.Elements())
            {
                builder.AppendLine(child.ToString());
            }
            HugsLibUtility.CopyToClipboard(builder.ToString());
        }

        internal static object GetPropertyValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(src, null);
        }

        internal static void SetPropertyValue(object src, string propName, object value)
        {
            src.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(src, value, null);
        }

        internal static object CallByReflection(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(o, args);
            }
            return null;
        }
    }
}
