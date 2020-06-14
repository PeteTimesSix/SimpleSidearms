using RimWorld;
using SimpleSidearms.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SimpleSidearms.rimworld
{
    public class JobGiver_RetrieveWeapon : ThinkNode_JobGiver
    { 

        public static Job TryGiveJobStatic(Pawn pawn, bool inCombat)
        {
            if (RestraintsUtility.InRestraints(pawn))
                return null;
            else
            {
                if (!pawn.IsValidSidearmsCarrier())
                    return null;

                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (pawnMemory == null)
                    return null;

                //if (pawnMemory.IsUsingAutotool(true))
                //    return null;

                //WeaponAssingment.equipBestWeaponFromInventoryByPreference(pawn, Globals.DroppingModeEnum.Calm);

                if (pawnMemory.RememberedWeapons is null)
                    Log.Warning("pawnMemory of "+pawn.Label+" is missing remembered weapons");

                Dictionary<ThingDefStuffDefPair, int> dupeCounters = new Dictionary<ThingDefStuffDefPair, int>();

                foreach (ThingDefStuffDefPair weaponMemory in pawnMemory.RememberedWeapons)
                {
                    if (!dupeCounters.ContainsKey(weaponMemory))
                        dupeCounters[weaponMemory] = 0;

                    if (!pawn.hasWeaponType(weaponMemory, dupeCounters[weaponMemory]))
                    {
                        float maxDist = 1000f;
                        if (pawn.Faction != Faction.OfPlayer)
                            maxDist = 30f;
                        if (inCombat)
                            maxDist = 12f;

                        bool bladelinkable = weaponMemory.thing.HasComp(typeof(CompBladelinkWeapon));
                        bool biocodeable = weaponMemory.thing.HasComp(typeof(CompBiocodableWeapon));

                        IEnumerable<ThingWithComps> matchingWeapons = pawn.Map.listerThings.ThingsOfDef(weaponMemory.thing).OfType<ThingWithComps>().Where(t => t.Stuff == weaponMemory.stuff);
                        if (bladelinkable)
                        {
                            matchingWeapons = matchingWeapons.Where(t =>
                            {
                                CompBladelinkWeapon bladelink = t.GetComp<CompBladelinkWeapon>();
                                return (bladelink != null && bladelink.bondedPawn == pawn);
                            });
                        }
                        if (biocodeable)
                        {
                            matchingWeapons = matchingWeapons.Where(t =>
                            {
                                CompBiocodableWeapon biocode = t.GetComp<CompBiocodableWeapon>();
                                if (biocode == null)
                                    return true; //not sure how this could ever happen...
                                if (biocode.Biocoded && biocode.CodedPawn != pawn)
                                    return false;
                                return true;
                            });
                        }

                        Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, matchingWeapons, PathEndMode.OnCell, TraverseParms.For(pawn), maxDist,
                            (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t),
                            (Thing t) => SimpleSidearms.ReEquipBest ? t.GetStatValue(StatDefOf.MeleeWeapon_AverageDPS, false) : 0);
                                                            //this works properly because better ranged weapons also happen to be better at pistolwhipping
                                                            //okay past me, WHAT? Why?

                        if (thing == null)
                            continue;

                        if (!inCombat)
                            return JobMaker.MakeJob(SidearmsDefOf.ReequipSecondary, thing);
                        else
                            return JobMaker.MakeJob(SidearmsDefOf.ReequipSecondaryCombat, thing, pawn.Position);

                    }

                    dupeCounters[weaponMemory]++;
                }

                return null;
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            return TryGiveJobStatic(pawn, false);
        }
    }
}
