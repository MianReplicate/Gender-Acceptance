using System;
using System.Collections.Generic;
using GenderAcceptance.Mian.MentalStates;
using RimWorld;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian.JobDrivers;

public class Transvestigate : JobDriver
{
    private const TargetIndex TargetInd = TargetIndex.A;

    private Pawn Target => (Pawn)(Thing)pawn.CurJob.GetTarget(TargetIndex.A);

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);

        yield return TransvestigatingSpreeDelayToil();
        var toil = Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
        toil.socialMode = RandomSocialMode.Off;
        yield return toil;
        yield return TransvestigateToil();
    }

    private Toil TransvestigateToil()
    {
        return Toils_General.Do((Action)(() =>
        {
            pawn.AttemptTransvestigate(Target);
            if (pawn.MentalState is TransvestigateSpree mentalState2)
            {
                mentalState2.lastTransvestigatedTicks = Find.TickManager.TicksGame;
                if (mentalState2.target != Target)
                    return;
                mentalState2.transvestigatedTargetAtLeastOnce = true;
            }
        }));
    }

    private Toil TransvestigatingSpreeDelayToil()
    {
        var toil = ToilMaker.MakeToil();
        toil.initAction = WaitAction;
        toil.tickIntervalAction = delta => WaitAction();
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;

        void WaitAction()
        {
            if (pawn.MentalState is TransvestigateSpree mentalState &&
                Find.TickManager.TicksGame - mentalState.lastTransvestigatedTicks < 1200)
                return;
            pawn.jobs.curDriver.ReadyForNextToil();
        }
    }
}