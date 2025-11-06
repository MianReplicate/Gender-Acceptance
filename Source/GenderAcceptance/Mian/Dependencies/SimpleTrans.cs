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

    public override bool AppearsToHaveMatchingGenitalia(Pawn pawn)
    {
        return (pawn.GetGenderedAppearance() == Gendered.Masculine && pawn.health.hediffSet.HasHediff(SimpleTransPregnancyUtility.canSireDef))
            || (pawn.GetGenderedAppearance() == Gendered.Feminine && pawn.health.hediffSet.HasHediff(SimpleTransPregnancyUtility.canCarryDef));
    }
}