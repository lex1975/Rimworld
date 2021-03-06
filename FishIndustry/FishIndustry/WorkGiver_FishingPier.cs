﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
using Verse.AI;      // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace FishIndustry
{
    /// <summary>
    /// WorkGiver_FishingPier class.
    /// </summary>
    /// <author>Rikiki</author>
    /// <permission>Use this code as you want, just remember to add a link to the corresponding Ludeon forum mod release thread.
    /// Remember learning is always better than just copy/paste...</permission>
    public class WorkGiver_FishingPier : WorkGiver_Scanner
    {
		public override ThingRequest PotentialWorkThingRequest
		{
			get
            {
                return ThingRequest.ForDef(Util_FishIndustry.FishingPierDef);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.OnCell;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
            if ((t is Building_FishingPier) == false)
            {
                return false;
            }
            Building_FishingPier fishingPier = t as Building_FishingPier;

            if (fishingPier.IsBurning()
                || (fishingPier.allowFishing == false))
            {
                return false;
            }
            if (Util_Zone_Fishing.IsAquaticTerrain(fishingPier.Map, fishingPier.fishingSpotCell) == false)
            {
                return false;
            }
            if (pawn.CanReserveAndReach(fishingPier, this.PathEndMode, Danger.Some) == false)
            {
                return false;
            }
            if (fishingPier.fishStock <= 0)
            {
                return false;
            }
            return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
            Job job = new Job();
            Building_FishingPier fishingPier = t as Building_FishingPier;

            if ((fishingPier.allowUsingCorn)
                && (HasFoodToAttractFishes(pawn) == false))
            {
                Predicate <Thing> predicate = delegate(Thing cornStack)
                {
                    return (cornStack.IsForbidden(pawn.Faction) == false)
                        && (cornStack.stackCount >= 4 * JobDriver_FishAtFishingPier.cornCountToAttractFishes);
                };
                TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn, false);
                Thing corn = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Util_FishIndustry.RawCornDef), Verse.AI.PathEndMode.ClosestTouch, traverseParams, 9999f, predicate);
                if (corn != null)
                {
                    return new Job(JobDefOf.TakeInventory, corn)
                    {
                        count = 4 * JobDriver_FishAtFishingPier.cornCountToAttractFishes
                    };
                }
            }
            job = new Job(Util_FishIndustry.FishAtFishingPierJobDef, fishingPier, fishingPier.fishingSpotCell);

            return job;
		}

        public bool HasFoodToAttractFishes(Pawn fisher)
        {
            foreach (Thing thing in fisher.inventory.innerContainer)
            {
                if ((thing.def == Util_FishIndustry.RawCornDef)
                    && (thing.stackCount >= JobDriver_FishAtFishingPier.cornCountToAttractFishes))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
