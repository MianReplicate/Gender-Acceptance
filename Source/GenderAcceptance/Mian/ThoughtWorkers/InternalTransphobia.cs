using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Utilities;
using RimWorld;
using Verse;
using GenderUtility = GenderAcceptance.Mian.Utilities.GenderUtility;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class InternalTransphobia : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (p.GetCurrentIdentity() == GenderIdentity.Transgender &&
            p.ideo?.Ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Despised)
        {
            var count = GenderUtility.CountGenderIndividuals(p, GenderIdentity.Transgender);
            return count <= 2 ? ThoughtState.ActiveAtStage(0) : ThoughtState.ActiveAtStage(1);
        }

        return ThoughtState.Inactive;
    }
}