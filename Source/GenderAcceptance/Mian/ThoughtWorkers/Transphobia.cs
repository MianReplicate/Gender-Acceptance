using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class ThoughtWorker_Transphobia : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
    {
        if ((p.IsTransphobic(false) &&
             otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender) || (
                p.GetCurrentIdentity() == GenderIdentity.Transgender && (otherPawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false)))
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}