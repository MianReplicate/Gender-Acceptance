using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class ThoughtWorker_IsTransphobic : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
    {
        if (pawn.IsTrannyphobic(false) && otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender)
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}