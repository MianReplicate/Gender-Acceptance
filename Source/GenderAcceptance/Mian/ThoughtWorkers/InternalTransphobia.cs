using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.ThoughtWorkers;

public class ThoughtWorker_InternalTransphobia : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (p.GetCurrentIdentity() == GenderIdentity.Transgender && (p.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false))
        {
            var count = Helper.CountGenderIndividuals(p.Map, GenderIdentity.Transgender);
            return count <= 2 ? ThoughtState.ActiveAtStage(0) : ThoughtState.ActiveAtStage(1);
        }
        return ThoughtState.Inactive;
    }
}