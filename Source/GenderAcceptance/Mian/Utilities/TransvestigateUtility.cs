using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian;

public class TransvestigateUtility
{
    public static bool CanChaseAndInvestigate(
        Pawn bully,
        Pawn investigated,
        bool skipReachabilityCheck = false,
        bool allowPrisoners = true)
    {
        if (!investigated.RaceProps.Humanlike ||
            (investigated.Faction != bully.Faction && (!allowPrisoners || investigated.HostFaction != bully.Faction)) ||
            investigated == bully || investigated.Dead || !investigated.Spawned ||
            !investigated.Position.InHorDistOf(bully.Position, 40f))
            return false;
        return skipReachabilityCheck ||
               bully.CanReach((LocalTargetInfo)(Thing)investigated, PathEndMode.Touch, Danger.Deadly);
    }

    public static void GetInvestigatingCandidatesFor(
        Pawn bully,
        List<Pawn> outCandidates,
        bool allowPrisoners = true)
    {
        outCandidates.Clear();
        var region = bully.GetRegion();
        if (region == null)
            return;
        var traverseParams = TraverseParms.For(bully);
        RegionTraverser.BreadthFirstTraverse(region, (from, to) => to.Allows(traverseParams, false), r =>
        {
            var thingList = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
            for (var index = 0; index < thingList.Count; ++index)
            {
                var investigated = (Pawn)thingList[index];
                if (CanChaseAndInvestigate(bully, investigated, true, allowPrisoners))
                    outCandidates.Add(investigated);
            }

            return false;
        }, 40);
    }
}