using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian.MentalStateWorkers;

public class TransvestigateSpree : MentalStateWorker
{
    public override bool StateCanOccur(Pawn pawn)
    {
        if (!base.StateCanOccur(pawn)) return false;

        if (!pawn.GetTransphobicStatus().GenerallyTransphobic)
            return false;

        var candidates = new List<Pawn>();
        TransvestigateUtility.GetInvestigatingCandidatesFor(pawn, candidates);
        var flag = candidates.Count >= 1;
        return flag;
    }
}