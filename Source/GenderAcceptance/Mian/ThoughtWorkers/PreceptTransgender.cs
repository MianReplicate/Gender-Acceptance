using System;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class PreceptTransgender : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        int transgenderCount = GenderUtility.CountGenderIndividuals(p, GenderIdentity.Transgender);
        int stage = Math.Min(transgenderCount - 1, 4);

        if (stage >= 0)
            return ThoughtState.ActiveAtStage(stage + 1);
        return ThoughtState.ActiveAtStage(0);
    }
}