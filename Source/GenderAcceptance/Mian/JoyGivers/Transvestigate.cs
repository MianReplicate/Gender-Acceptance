using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian.JoyGivers;

public class Transvestigate : JoyGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        if (!pawn.GetTransphobicStatus().GenerallyTransphobic)
            return null;
        if (PawnUtility.WillSoonHaveBasicNeed(pawn))
            return null;
        var candidates = new List<Pawn>();
        TransvestigateUtility.GetInvestigatingCandidatesFor(pawn, candidates);
        if (!candidates.Any())
            return null;

        return JobMaker.MakeJob(def.jobDef, candidates.RandomElement());
    }

    public override float GetChance(Pawn pawn)
    {
        return base.GetChance(pawn) * (pawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false ? 1.5f : 1) *
               (pawn.story?.traits?.HasTrait(GADefOf.Chaser) ?? false ? 2f : 1);
    }
}