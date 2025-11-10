using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class Similarity : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
    {
        if (p.GetCurrentIdentity() == GenderIdentity.Transgender && p.BelievesIsTrans(otherPawn))
            return ThoughtState.ActiveAtStage(0);
        return ThoughtState.Inactive;
    }
}