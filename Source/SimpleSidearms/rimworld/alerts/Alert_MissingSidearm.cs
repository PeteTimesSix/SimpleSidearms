using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace SimpleSidearms.rimworld.alerts
{
    class Alert_MissingSidearm : Alert
    {
        protected string explanation;

        public Alert_MissingSidearm()
        {
            this.defaultLabel = "Alert_MissingSidearm_label".Translate();
            explanation = "Alert_MissingSidearm_desc";
        }

        public override string GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn current in this.AffectedPawns())
            {
                stringBuilder.AppendLine("    " + current.Name);
            }
            return explanation.Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            //return true;
            Pawn pawn = this.AffectedPawns().FirstOrDefault<Pawn>();
            if (pawn != null)
            {
                return AlertReport.CulpritIs(pawn);
            }
            return AlertReport.Inactive;
        }

        [DebuggerHidden]
        private IEnumerable<Pawn> AffectedPawns()
        {
            HashSet<Pawn> pawns = new HashSet<Pawn>();
            if(PawnsFinder.AllMaps_FreeColonistsSpawned != null)
            {
                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
                {
                    if (pawn.health != null && pawn.Dead)
                    {
                        Log.Error("Dead pawn in PawnsFinder.AllMaps_FreeColonists:" + pawn);
                    }
                    else
                    {
                        if (pawn.health != null && pawn.Downed)
                            continue;
                        if (pawn.drafter != null && pawn.Drafted)
                            continue;
                        if (pawn.CurJob != null && pawn.CurJob.def != null && (pawn.CurJob.def == SidearmsDefOf.EquipSecondary || pawn.CurJob.def == SidearmsDefOf.EquipSecondaryCombat))
                            continue;

                        GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
                        if (pawnMemory != null)
                        {
                            foreach (string wepName in pawnMemory.weapons)
                            {
                                if (pawnMemory.primary != null && wepName.Equals(pawnMemory.primary))
                                    continue;
                                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(wepName);
                                if (def == null)
                                    continue;

                                if (!pawn.hasWeaponSomewhere(wepName))
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
