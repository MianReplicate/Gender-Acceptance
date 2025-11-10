
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian.MentalStates;

public class TransvestigateSpree : Verse.AI.MentalState
{
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look<int>(ref this.targetFoundTicks, "targetFoundTicks", 0, false);
        Scribe_References.Look<Pawn>(ref this.target, "target", false);
        Scribe_Values.Look<bool>(ref this.transvestigatedTargetAtLeastOnce, "transvestigatedTargetAtLeastOnce", false, false);
        Scribe_Values.Look<int>(ref this.lastTransvestigatedTicks, "lastTransvestigatedTicks", 0, false);
    }

    public override RandomSocialMode SocialModeMax()
    {
        return RandomSocialMode.Off;
    }
    
    public override void PostStart(string reason)
    {
	    base.PostStart(reason);
	    this.ChooseNextTarget();
    }

    public override void MentalStateTick(int delta)
    {
	    if (this.target != null && !TransvestigateUtility.CanChaseAndInvestigate(this.pawn, this.target, false, true))
	    {
		    this.ChooseNextTarget();
	    }
	    if (this.pawn.IsHashIntervalTick(250, delta) && (this.target == null || this.transvestigatedTargetAtLeastOnce))
	    {
		    this.ChooseNextTarget();
	    }
	    base.MentalStateTick(delta);
    }

    private void ChooseNextTarget()
    {
	    var list = new List<Pawn>();
	    TransvestigateUtility.GetInvestigatingCandidatesFor(this.pawn, list, true);
	    if (!list.Any<Pawn>())
	    {
		    this.target = null;
		    this.transvestigatedTargetAtLeastOnce = false;
		    this.targetFoundTicks = -1;
		    return;
	    }
	    Pawn pawn;
	    if (this.target != null && Find.TickManager.TicksGame - this.targetFoundTicks > 1250 && list.Any((Pawn x) => x != this.target))
	    {
		    pawn = list.Where((Pawn x) => x != this.target).RandomElementByWeight((Pawn x) => this.GetCandidateWeight(x));
	    }
	    else
	    {
		    pawn = list.RandomElementByWeight((Pawn x) => this.GetCandidateWeight(x));
	    }
	    if (pawn != this.target)
	    {
		    this.target = pawn;
		    this.transvestigatedTargetAtLeastOnce = false;
		    this.targetFoundTicks = Find.TickManager.TicksGame;
	    }
    }

    private float GetCandidateWeight(Pawn candidate)
    {
	    float num = Mathf.Min(this.pawn.Position.DistanceTo(candidate.Position) / 40f, 1f);
	    return 1f - num + 0.01f;
    }

    private int targetFoundTicks;

    private const int CheckChooseNewTargetIntervalTicks = 250;

    private const int MaxSameTargetChaseTicks = 1250;
    
    public Pawn target;

    public bool transvestigatedTargetAtLeastOnce;

    public int lastTransvestigatedTicks = -999999;
}