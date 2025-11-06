using LoveyDoveySexWithRosaline;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class GenderWorks : TransDependency
{
    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        return pawn.AppearsToHaveMatchingGenitalia() ? GenderIdentity.Cisgender : GenderIdentity.Transgender;
    }

    public override bool AppearsToHaveMatchingGenitalia(Pawn pawn)
    {
        return (pawn.GetGenderedAppearance() == Gendered.Feminine && GenderUtilities.HasFemaleReproductiveOrgan(pawn)) 
               || (pawn.GetGenderedAppearance() == Gendered.Masculine && GenderUtilities.HasMaleReproductiveOrgan(pawn));
    }
}