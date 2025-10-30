using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class InteractionWorker_PurposeMisgender : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (initiator.IsTransphobic() && recipient.GetCurrentIdentity() == GenderIdentity.Transgender)
        {
            return 1 * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient) * (initiator.IsTransphobic(false) ? 1.5f : 1);
        }
        return 0.0f;
    }
}