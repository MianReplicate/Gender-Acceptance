using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian.MentalStates;

public class TransvestigateSpree : MentalState
{
    private const int CheckChooseNewTargetIntervalTicks = 250;

    private const int MaxSameTargetChaseTicks = 1250;

    public int lastTransvestigatedTicks = -999999;

    public Pawn target;

    private int targetFoundTicks;

    public bool transvestigatedTargetAtLeastOnce;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref targetFoundTicks, "targetFoundTicks");
        Scribe_References.Look(ref target, "target");
        Scribe_Values.Look(ref transvestigatedTargetAtLeastOnce, "transvestigatedTargetAtLeastOnce");
        Scribe_Values.Look(ref lastTransvestigatedTicks, "lastTransvestigatedTicks");
    }

    public override RandomSocialMode SocialModeMax()
    {
        return RandomSocialMode.Off;
    }

    public override void PostStart(string reason)
    {
        base.PostStart(reason);
        ChooseNextTarget();
    }

    public override void MentalStateTick(int delta)
    {
        if (target != null && !TransvestigateUtility.CanChaseAndInvestigate(pawn, target)) ChooseNextTarget();
        if (pawn.IsHashIntervalTick(250, delta) && (target == null || transvestigatedTargetAtLeastOnce))
            ChooseNextTarget();
        base.MentalStateTick(delta);
    }

    private void ChooseNextTarget()
    {
        var list = new List<Pawn>();
        TransvestigateUtility.GetInvestigatingCandidatesFor(this.pawn, list);
        if (!list.Any())
        {
            target = null;
            transvestigatedTargetAtLeastOnce = false;
            targetFoundTicks = -1;
            return;
        }

        Pawn pawn;
        if (target != null && Find.TickManager.TicksGame - targetFoundTicks > 1250 && list.Any(x => x != target))
            pawn = list.Where(x => x != target).RandomElementByWeight(x => GetCandidateWeight(x));
        else
            pawn = list.RandomElementByWeight(x => GetCandidateWeight(x));
        if (pawn != target)
        {
            target = pawn;
            transvestigatedTargetAtLeastOnce = false;
            targetFoundTicks = Find.TickManager.TicksGame;
        }
    }

    private float GetCandidateWeight(Pawn candidate)
    {
        var num = Mathf.Min(pawn.Position.DistanceTo(candidate.Position) / 40f, 1f);
        return 1f - num + 0.01f;
    }
}