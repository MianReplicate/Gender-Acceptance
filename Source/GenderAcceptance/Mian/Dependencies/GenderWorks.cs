using LoveyDoveySexWithRosaline;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class GenderWorks : TransDependency
{
    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        return pawn.HasMatchingGenitalia() ? GenderIdentity.Cisgender : GenderIdentity.Transgender;
    }

    public override bool HasMatchingGenitalia(Pawn pawn)
    {
        return (pawn.gender == Gender.Female && GenderUtilities.HasFemaleReproductiveOrgan(pawn)) 
               || (pawn.gender == Gender.Male && GenderUtilities.HasMaleReproductiveOrgan(pawn));
    }
}