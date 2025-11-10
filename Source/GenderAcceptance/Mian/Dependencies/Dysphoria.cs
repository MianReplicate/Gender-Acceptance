using Identity;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class Dysphoria : TransDependency
{
    private static readonly PreceptDef Trans_Abhorrent = DefDatabase<PreceptDef>.GetNamed("Trans_Abhorrent");
    private static readonly PreceptDef Trans_Disapproved = DefDatabase<PreceptDef>.GetNamed("Trans_Disapproved");
    private static PreceptDef Trans_Neutral = DefDatabase<PreceptDef>.GetNamed("Trans_Neutral");
    private static readonly PreceptDef Trans_Approved = DefDatabase<PreceptDef>.GetNamed("Trans_Approved");
    private static readonly PreceptDef Trans_Exalted = DefDatabase<PreceptDef>.GetNamed("Trans_Exalted");

    private static readonly TraitDef[] genders = new TraitDef[3]
    {
        DefOfDysphoria.maleGender,
        DefOfDysphoria.femaleGender,
        DefOfDysphoria.androgyneGender
    };

    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        foreach (var gender in genders)
            if (pawn.story?.traits?.HasTrait(gender) ?? false)
                return GenderIdentity.Transgender;

        return GenderIdentity.Cisgender;
    }

    public override CultureViewOnTrans CultureOpinionOnTrans(Pawn pawn)
    {
        if (pawn.Ideo?.HasPrecept(Trans_Abhorrent) ?? false)
            return CultureViewOnTrans.Abhorrent;
        if (pawn.Ideo?.HasPrecept(Trans_Disapproved) ?? false)
            return CultureViewOnTrans.Despised;
        if (pawn.Ideo?.HasPrecept(Trans_Approved) ?? false)
            return CultureViewOnTrans.Adored;
        if (pawn.Ideo?.HasPrecept(Trans_Exalted) ?? false)
            return CultureViewOnTrans.Exalted;

        return CultureViewOnTrans.Neutral;
    }

    public override bool AppearsToHaveMatchingGenitalia(Pawn pawn)
    {
        var breasts = DefOfDysphoria.Breasts;
        var noBreasts = DefOfDysphoria.NoBreasts;
        var hediffs = pawn.health.hediffSet;

        return (hediffs.HasHediff(breasts) && pawn.GetGenderedAppearance() == Gendered.Feminine) ||
               (hediffs.HasHediff(noBreasts) && pawn.GetGenderedAppearance() == Gendered.Masculine);
    }

    public override float GetGenderedPoints(Pawn pawn)
    {
        var femStat = pawn.GetStatValue(DefOfDysphoria.FemStat);
        var mascStat = pawn.GetStatValue(DefOfDysphoria.MascStat);

        return (mascStat - femStat) / 20 + base.GetGenderedPoints(pawn);
    }
}