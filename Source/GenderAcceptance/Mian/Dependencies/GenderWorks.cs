using LoveyDoveySexWithRosaline;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class GenderWorks
{
    public static GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        return GenderUtilities.HasFemaleReproductiveOrgan(pawn) && pawn.gender == Gender.Female ? GenderIdentity.Cisgender :
                GenderUtilities.HasMaleReproductiveOrgan(pawn) && pawn.gender == Gender.Male ? GenderIdentity.Cisgender : GenderIdentity.Transgender;
    }
}