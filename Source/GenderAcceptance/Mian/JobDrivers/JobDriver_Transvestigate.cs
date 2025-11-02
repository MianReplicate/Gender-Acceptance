using System;
using System.Collections.Generic;
using GenderAcceptance.Mian.MentalState;
using GenderAcceptance.Mian.Verbs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian.JobDrivers;

public class JobDriver_Transvestigate : JobDriver
{
    private const TargetIndex TargetInd = TargetIndex.A;

    private Pawn Target => (Pawn) (Thing) this.pawn.CurJob.GetTarget(TargetIndex.A);

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull<JobDriver_Transvestigate>(TargetIndex.A);
        var verb = new Verb_Transvestigate();
        var request = new CastPositionRequest
        {
            caster = pawn,
            verb = verb,
            target = Target,
            maxRangeFromTarget =
                ((pawn == null || !pawn.Downed) ? Mathf.Max(verb.verbProps.range * 10f, 1.42f) 
                    : Mathf.Min(verb.verbProps.range, pawn.RaceProps.executionRange)),
            wantCoverFromTarget = true
        };
        
        var found = CastPositionFinder.TryFindCastPosition(request, out IntVec3 vec3);
        if (found)
        {
            yield return Toils_Goto.GotoCell(vec3, PathEndMode.ClosestTouch);
            yield return this.TransvestigatingSpreeDelayToil();
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
            Toil toil = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            toil.socialMode = RandomSocialMode.Off;
            yield return toil;
            yield return this.InteractToil();   
        }
        else
        {
            yield return Toils_General.Wait(30);
        }
    }

    private Toil InteractToil()
    {
        return Toils_General.Do((Action) (() =>
        {
            if (!(this.pawn.MentalState is MentalState_TransvestigateSpree mentalState2))
                return;
            this.pawn.AttemptTransvestigate(this.Target);
            mentalState2.lastTransvestigatedTicks = Find.TickManager.TicksGame;
            if (mentalState2.target != this.Target)
                return;
            mentalState2.transvestigatedTargetAtLeastOnce = true;
        }));
    }

    private Toil TransvestigatingSpreeDelayToil()
    {
        Toil toil = ToilMaker.MakeToil(nameof (TransvestigatingSpreeDelayToil));
        toil.initAction = new Action(WaitAction);
        toil.tickIntervalAction = (Action<int>) (delta => WaitAction());
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;

        void WaitAction()
        {
            if (this.pawn.MentalState is MentalState_TransvestigateSpree mentalState && Find.TickManager.TicksGame - mentalState.lastTransvestigatedTicks < 1200)
                return;
            this.pawn.jobs.curDriver.ReadyForNextToil();
        }
    }
}