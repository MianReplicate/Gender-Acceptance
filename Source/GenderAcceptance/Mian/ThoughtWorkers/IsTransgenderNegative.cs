using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class IsTransgenderNegative : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
    {
        if (ModsConfig.IsActive("cammy.identity.gender"))
            return ThoughtState.Inactive;
        
        if (pawn.CultureOpinionOnTrans() == CultureViewOnTrans.Despised && pawn.BelievesIsTrans(otherPawn))
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}