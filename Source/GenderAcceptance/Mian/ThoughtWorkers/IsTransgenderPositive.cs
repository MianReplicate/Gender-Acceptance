using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class ThoughtWorker_IsTransgenderPositive : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
    {
        if (ModsConfig.IsActive("cammy.identity.gender"))
            return ThoughtState.Inactive;
        
        if (pawn.CultureOpinionOnTrans() == CultureViewOnTrans.Adored && pawn.BelievesIsTrans(otherPawn))
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}