using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian;

public class TransvestigateUtility
{
    private const int MaxRegionsToSearch = 40;
    public const int MaxDistance = 40;
    public const int MinTicksBetweenInvestigations = 1200;

    public static bool CanChaseAndInvestigate(
        Pawn bully,
        Pawn investigated,
        bool skipReachabilityCheck = false,
        bool allowPrisoners = true)
    {
        if (!investigated.RaceProps.Humanlike || investigated.Faction != bully.Faction && (!allowPrisoners || investigated.HostFaction != bully.Faction) || investigated == bully || investigated.Dead || !investigated.Spawned || !investigated.Position.InHorDistOf(bully.Position, 40f) || !investigated.IsInteractionBlocked(null, false, false) || investigated.HostileTo((Thing) bully) || Find.TickManager.TicksGame - investigated.mindState.lastHarmTick < 833)
            return false;
        return skipReachabilityCheck || bully.CanReach((LocalTargetInfo) (Thing) investigated, PathEndMode.Touch, Danger.Deadly);
    }

    public static void GetInvestigatingCandidatesFor(
        Pawn bully,
        List<Pawn> outCandidates,
        bool allowPrisoners = true)
    {
        outCandidates.Clear();
        Region region = bully.GetRegion();
        if (region == null)
            return;
        TraverseParms traverseParams = TraverseParms.For(bully);
        RegionTraverser.BreadthFirstTraverse(region, (RegionEntryPredicate) ((from, to) => to.Allows(traverseParams, false)), (RegionProcessor) (r =>
        {
            List<Thing> thingList = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
            for (int index = 0; index < thingList.Count; ++index)
            {
                Pawn investigated = (Pawn) thingList[index];
                if (CanChaseAndInvestigate(bully, investigated, true, allowPrisoners))
                    outCandidates.Add(investigated);
            }
            return false;
        }), 40);
    }
}