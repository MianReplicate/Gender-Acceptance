using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class SimpleTrans
{
    public static GenderIdentity GetCurrentIdentity(Pawn pawn)
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
}