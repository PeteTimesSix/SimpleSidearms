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

        protected override Job TryGiveJob(Pawn pawn)
        {
            //Log.Message("retrieval precheck for " + pawn.LabelShort);
            if (pawn.Faction != Faction.OfPlayer)
            {
                return null;
            }

            if (pawn.Drafted)
            {
                return null;
            }

            /*if (Find.TickManager.TicksGame > GetNextTryTick(pawn))
            {
                return null;
            }
            else*/
            {
                //SetNextTryTick(pawn, Find.TickManager.TicksGame + UnityEngine.Random.Range(PickupCheckIntervalMin, PickupCheckIntervalMax));

                //Log.Message("retrieval check for " + pawn.LabelShort);

                Pawn_EquipmentTracker equipment = pawn.equipment;
                if (equipment == null)
                    return null;

                GoldfishModule pawnMemory = GoldfishModule.GetGoldfishForPawn(pawn);
                if (pawnMemory == null)
                    return null;

                WeaponAssingment.reequipPrimaryIfNeededAndAvailable(pawn, pawnMemory);

                foreach(string wepName in pawnMemory.weapons)
                {
                    //Log.Message("checking "+wepName);
                    ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(wepName);
                    if (def == null)
                        continue;

                    if (!pawn.hasWeaponSomewhere(wepName))
                    {
                        //Log.Message("looking for");
                        ThingRequest request = new ThingRequest();
                        request.singleDef = def;
                        Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, request, PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 1000f, (Thing x) => (!x.IsForbidden(pawn)) && pawn.CanReserve(x, 1, -1, null, false));

                        if (thing == null)
                            continue;

                        return new Job(SidearmsJobDefOf.EquipSecondary, thing);
                    }
                }

                return null;
            }
        }
    }
}
