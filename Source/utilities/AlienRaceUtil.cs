using System;
using System.Diagnostics;
using System.Reflection;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Utilities
{
    [StaticConstructorOnStartup]
    public static class AlienRaceUtil
    {
        public static bool AlienRacesLoaded;
        private static MethodInfo CanEquipMethodInfo;
        static AlienRaceUtil()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            AlienRacesLoaded = false;
            var runningMods = LoadedModManager.RunningModsListForReading;
            foreach (var runningMod in runningMods)
            {
                foreach (Assembly asm in runningMod.assemblies.loadedAssemblies)
                {
                    Type cls = asm.GetType("AlienRace.RaceRestrictionSettings");
                    if (cls != null)
                    {
                        AlienRacesLoaded = true;
                        CanEquipMethodInfo = cls.GetMethod("CanEquip");

                    }
                }
            }
            sw.Stop();
        }

        public static bool RaceCanEquip(ThingDef weapon, ThingDef race)
        {
            if (weapon != null && CanEquipMethodInfo != null)
            {
                var result = CanEquipMethodInfo.Invoke(null, new[] { weapon, race });
                return (bool)result;
            }
            return true;
        }

    }
}
