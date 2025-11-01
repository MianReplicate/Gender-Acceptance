using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class ThoughtWorker_IsCisphobic : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
    {
        if ((pawn.story?.traits?.HasTrait(GADefOf.Cisphobic) ?? false ) && !pawn.BelievesIsTrans(otherPawn))
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}