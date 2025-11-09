using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class PurposeMisgender : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        var trannyphobic = initiator.GetTrannyphobicStatus();
        if (trannyphobic.GenerallyTransphobic && initiator.BelievesIsTrans(recipient))
        {
            return 1 * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient) * (trannyphobic.HasTransphobicTrait ? 1.5f : 1);
        }
        return 0.0f;
    }
}