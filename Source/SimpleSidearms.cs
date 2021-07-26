using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PeteTimesSix.SimpleSidearms
{
    public class SimpleSidearms : Mod
    {
        public static SimpleSidearms_Settings Settings { get; private set; }

        public SimpleSidearms(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SimpleSidearms_Settings>();
            Settings.StartupChecks();

            var harmony = new Harmony("PeteTimesSix.CompactHediffs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory()
        {
            return "SimpleSidearms_ModTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }

}
