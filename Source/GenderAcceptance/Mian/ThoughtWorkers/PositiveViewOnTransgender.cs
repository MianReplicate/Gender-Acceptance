using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class ThoughtWorker_PositiveViewOnTransgender : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
    {
        if (pawn.Ideo?.HasPrecept(GADefOf.Transgender_Adored) ?? false)
        {
            if (otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender)
            {
                return ThoughtState.ActiveAtStage(0);
            }
        }
        return ThoughtState.Inactive;
    }
}