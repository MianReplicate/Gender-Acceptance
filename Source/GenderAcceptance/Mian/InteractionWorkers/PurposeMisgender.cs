using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class PurposeMisgender : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        var transphobic = initiator.GetTransphobicStatus(recipient);
        if (transphobic.GenerallyTransphobic && initiator.BelievesIsTrans(recipient))
            return 1 * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient) *
                   (transphobic.HasTransphobicTrait ? 1.5f : 1);
        return 0.0f;
    }
}