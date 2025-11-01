using LoveyDoveySexWithRosaline;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class GenderWorks : TransDependency
{
    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        return GenderUtilities.HasFemaleReproductiveOrgan(pawn) && pawn.gender == Gender.Female ? GenderIdentity.Cisgender :
                GenderUtilities.HasMaleReproductiveOrgan(pawn) && pawn.gender == Gender.Male ? GenderIdentity.Cisgender : GenderIdentity.Transgender;
    }

    public override bool HasMatchingGenitalia(Pawn pawn)
    {
        return (pawn.gender == Gender.Female && GenderUtilities.HasFemaleReproductiveOrgan(pawn)) 
               || (pawn.gender == Gender.Male && GenderUtilities.HasMaleReproductiveOrgan(pawn));
    }
}