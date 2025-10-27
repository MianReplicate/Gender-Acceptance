using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class ThoughtWorker_Cisphobia : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
    {
        if (((p.story?.traits?.HasTrait(GADefOf.Cisphobic) ?? false)
             && otherPawn.GetCurrentIdentity() == GenderIdentity.Cisgender) || (
                p.GetCurrentIdentity() == GenderIdentity.Cisgender && (otherPawn.story?.traits?.HasTrait(GADefOf.Cisphobic) ?? false)))
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}