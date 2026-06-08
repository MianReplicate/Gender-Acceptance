using GenderAcceptance.Mian.Utilities;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

[TransLibrary(["runaway.simpletrans"], "Simple Trans (Expanded)")]
public class SimpleTrans : TransDependency
{

    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        return SimpleTransHediffs.IsCisgender(pawn) ? GenderIdentity.Cisgender : GenderIdentity.Transgender;
    }

    public override bool AppearsToHaveMatchingGenitalia(Pawn pawn)
    {
        return (pawn.GetGenderedAppearance() == Gendered.Masculine &&
                HasPhallus(pawn))
               || (pawn.GetGenderedAppearance() == Gendered.Feminine &&
                   HasVulva(pawn));
    }

    public bool HasVulva(Pawn pawn)
    {
        return (pawn.health?.hediffSet?.HasHediff(SimpleTransHediffs.canCarryDef) ?? false) || SimpleTransHediffs.HasBionicCarry(pawn);
    }
    
    public bool HasPhallus(Pawn pawn)
    {
        return (pawn.health?.hediffSet?.HasHediff(SimpleTransHediffs.canSireDef) ?? false) || SimpleTransHediffs.HasBionicSire(pawn);
    }
}