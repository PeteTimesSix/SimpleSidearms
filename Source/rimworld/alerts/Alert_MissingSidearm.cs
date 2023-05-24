using PeteTimesSix.SimpleSidearms;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

using static PeteTimesSix.SimpleSidearms.SimpleSidearms;

namespace SimpleSidearms.rimworld.alerts
{
    public class Alert_MissingSidearm : Alert
    {
        public string explanation;

        public Alert_MissingSidearm()
        {
            this.defaultLabel = "Alert_MissingSidearm_label".Translate();
            explanation = "Alert_MissingSidearm_desc";
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn current in this.AffectedPawns())
            {
                stringBuilder.AppendLine("    " + current.Name);
            }
            return explanation.Translate(stringBuilder.ToString());
        }

        int ticks = 4;
        public override AlertReport GetReport()
        {
            //Run 1/4th as often
            if (ticks - 1 != 0) return AlertReport.Inactive;
            ticks = 4;

            Pawn pawn = this.AffectedPawns().FirstOrDefault<Pawn>();
            if (pawn != null)
            {
                return AlertReport.CulpritIs(pawn);
            }
            return AlertReport.Inactive;
        }

        [DebuggerHidden]
        public IEnumerable<Pawn> AffectedPawns()
        {
            if (!Settings.ShowAlertsMissingSidearm)
                yield break;
            else 
            {
                HashSet<Pawn> pawns = new HashSet<Pawn>();
                if (PawnsFinder.AllMaps_FreeColonistsSpawned is List<Pawn> allMaps_FreeColonistsSpawned)
                {
                    for (int i = allMaps_FreeColonistsSpawned.Count - 1; i >= 0; i--)
                    {
                        Pawn pawn = allMaps_FreeColonistsSpawned[i];
                        if (!pawn.IsValidSidearmsCarrierRightNow())
                        {
                            continue;
                        }
                        
                        if (pawn.health != null && pawn.Downed)
                            continue;
                        if (pawn.Drafted)
                            continue;
                        if (pawn.CurJobDef == SidearmsDefOf.EquipSecondary)
                            continue;

                        if (CompSidearmMemory.GetMemoryCompForPawn(pawn) is CompSidearmMemory pawnMemory)
                        {
                            var rememberedWeapons = pawnMemory.RememberedWeapons;
                            for (int j = rememberedWeapons.Count; j-- > 0;)
                            {
                                if (!pawn.hasWeaponType(rememberedWeapons[j]))
                                {
                                    if (pawns.Add(pawn))
                                        yield return pawn;
                                }
                            }
                        } 
                    }
                }
            }
        }
    }
}