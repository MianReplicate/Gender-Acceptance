using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Utilities;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class IsTransgenderPositive : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
    {
        if (ModsConfig.IsActive("cammy.identity.gender"))
            return ThoughtState.Inactive;
        if (pawn == otherPawn)
            return ThoughtState.Inactive;

        if (pawn.ideo?.Ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Adored && pawn.BelievesIsTrans(otherPawn))
            return ThoughtState.ActiveAtStage(0);
        return ThoughtState.Inactive;
    }
}