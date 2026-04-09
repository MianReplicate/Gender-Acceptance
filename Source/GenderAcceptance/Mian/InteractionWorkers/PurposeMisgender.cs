using GenderAcceptance.Mian.Utilities;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class PurposeMisgender : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        var transphobic = initiator.GetTransphobicStatus(recipient);
        if (transphobic.GenerallyTransphobic)
            return (initiator.BelievesIsTrans(recipient) ? 1 : 0.05f) * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient) *
                   (transphobic.HasTransphobicTrait ? 1.5f : 1) * recipient.AppearanceMismatchChanceFactor();
        
        return 0.0f;
    }
}