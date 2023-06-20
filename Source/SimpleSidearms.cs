using HarmonyLib;
using PeteTimesSix.SimpleSidearms.Compat;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PeteTimesSix.SimpleSidearms
{
    public class SimpleSidearms : Mod
    {
        public static SimpleSidearms_Settings Settings { get; internal set; }
        public static SimpleSidearms ModSingleton { get; private set; }
        public static Harmony Harmony { get; private set; } 

        public SimpleSidearms(ModContentPack content) : base(content)
        {
            ModSingleton = this;
            Harmony = new Harmony("PeteTimesSix.SimpleSidearms");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            OptionalPatches.Patch(Harmony);
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

    [StaticConstructorOnStartup]
    public static class SimpleSidearms_PostInit 
    {
        static SimpleSidearms_PostInit()
        {
            OptionalPatches.PatchDelayed(SimpleSidearms.Harmony);

            SimpleSidearms.Settings = SimpleSidearms.ModSingleton.GetSettings<SimpleSidearms_Settings>();
            InferredValues.Init();
            SimpleSidearms.Settings.StartupChecks();

            if (SimpleSidearms.Settings.NeedsResaving)
            {
                Log.Message($"SS: Resaving settings by request (one-time migration or clearing out invalid defs).");
                SimpleSidearms.Settings.NeedsResaving = false;
                SimpleSidearms.ModSingleton.WriteSettings();
            }
        }
    }

}
