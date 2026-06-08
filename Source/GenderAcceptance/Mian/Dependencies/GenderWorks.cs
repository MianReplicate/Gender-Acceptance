using GenderAcceptance.Mian.Utilities;
using LoveyDoveySexWithRosaline;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

[TransLibrary(["lovelydovey.sex.withrosaline"], "Intimacy - Gender Works")]
public class GenderWorks : TransDependency
{
    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        return pawn.AppearsToHaveMatchingGenitalia() ? GenderIdentity.Cisgender : GenderIdentity.Transgender;
    }

    public override bool AppearsToHaveMatchingGenitalia(Pawn pawn)
    {
        return (pawn.GetGenderedAppearance() == Gendered.Feminine && GenderUtilities.HasFemaleReproductiveOrgan(pawn))
               || (pawn.GetGenderedAppearance() == Gendered.Masculine &&
                   GenderUtilities.HasMaleReproductiveOrgan(pawn));
    }
}