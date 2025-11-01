using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class SimpleTrans : TransDependency
{
    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        if (pawn.health?.hediffSet?.HasHediff(SimpleTransPregnancyUtility.transDef) ?? false)
        {
            return GenderIdentity.Transgender;
        }
        if (pawn.health?.hediffSet?.HasHediff(SimpleTransPregnancyUtility.cisDef) ?? false)
        {
            return GenderIdentity.Cisgender;
        }
        return GenderIdentity.Cisgender;
    }

    public override bool HasMatchingGenitalia(Pawn pawn)
    {
        return (pawn.gender == Gender.Male && pawn.health.hediffSet.HasHediff(SimpleTransPregnancyUtility.canSireDef))
            || (pawn.gender == Gender.Female && pawn.health.hediffSet.HasHediff(SimpleTransPregnancyUtility.canCarryDef));
    }
}