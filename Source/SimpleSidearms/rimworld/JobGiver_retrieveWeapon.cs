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
    class JobGiver_RetrieveWeapon : ThinkNode_JobGiver
    { 
        private const int PickupCheckIntervalMin = 600;
         
        private const int PickupCheckIntervalMax = 1200;

        private int GetNextTryTick(Pawn pawn)
        {
            int result;
            if (pawn.mindState.thinkData.TryGetValue(base.UniqueSaveKey, out result))
            {
                return result;
            }
            return int.MaxValue; 
        }

        private void SetNextTryTick(Pawn pawn, int val)
        {
            pawn.mindState.thinkData[base.UniqueSaveKey] = val;
        }

        public static Job TryGiveJobStatic(Pawn pawn, bool inCombat)
        {

            if (RestraintsUtility.InRestraints(pawn))
                return null;
      
            {
                //SetNextTryTick(pawn, Find.TickManager.TicksGame + UnityEngine.Random.Range(PickupCheckIntervalMin, PickupCheckIntervalMax));
                

                Pawn_EquipmentTracker equipment = pawn.equipment;
                if (equipment == null)
                    return null;

                GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
                if (pawnMemory == null)
                    return null;

                WeaponAssingment.reequipPrimaryIfNeededAndAvailable(pawn, pawnMemory);

                foreach (string wepName in pawnMemory.weapons)
                {
                    ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(wepName);
                    if (def == null)
                        continue;

                    if (!pawn.hasWeaponSomewhere(wepName))
                    {
                        ThingRequest request = new ThingRequest();
                        request.singleDef = def;
                        float maxDist = 1000f;
                        if (pawn.Faction != Faction.OfPlayer || inCombat)
                            maxDist = 30f;
                        Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, request, PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), maxDist, (Thing x) => (!x.IsForbidden(pawn)) && pawn.CanReserve(x, 1, -1, null, false));

                        if (thing == null)
                            continue;

                        if(!inCombat)
                            return new Job(SidearmsDefOf.EquipSecondary, thing);
                        else
                            return new Job(SidearmsDefOf.EquipSecondaryCombat, thing, pawn.Position);
                        
                    }
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
