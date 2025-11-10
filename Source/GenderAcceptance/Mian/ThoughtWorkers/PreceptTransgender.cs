using System;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class PreceptTransgender : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        var transgenderCount = GenderUtility.CountGenderIndividuals(p, GenderIdentity.Transgender);
        var stage = Math.Min(transgenderCount - 1, 4);

        if (stage >= 0)
            return ThoughtState.ActiveAtStage(stage + 1);
        return ThoughtState.ActiveAtStage(0);
    }
}