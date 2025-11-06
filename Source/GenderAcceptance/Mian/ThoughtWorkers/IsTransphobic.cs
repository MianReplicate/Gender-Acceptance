using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class IsTransphobic : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
    {
        if (pawn.IsTrannyphobic(false) && pawn.BelievesIsTrans(otherPawn))
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}