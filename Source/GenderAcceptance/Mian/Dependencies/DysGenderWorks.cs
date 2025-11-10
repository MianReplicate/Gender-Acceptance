using Identity;
using LoveyDoveySexWithRosaline;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

// If Dysphoria and GenderWorks are both installed
public class DysGenderWorks : Dysphoria
{
    public override bool AppearsToHaveMatchingGenitalia(Pawn pawn)
    {
        var breasts = DefOfDysphoria.Breasts;
        var noBreasts = DefOfDysphoria.NoBreasts;
        var hediffs = pawn.health.hediffSet;
        
        return (pawn.GetGenderedAppearance() == Gendered.Feminine && GenderUtilities.HasFemaleReproductiveOrgan(pawn) && hediffs.HasHediff(breasts))
               || (pawn.GetGenderedAppearance() == Gendered.Masculine && GenderUtilities.HasMaleReproductiveOrgan(pawn) && hediffs.HasHediff(noBreasts));
    }
}