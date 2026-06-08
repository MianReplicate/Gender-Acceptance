using GenderAcceptance.Mian.Utilities;
using Identity;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

[TransLibrary(["cammy.identity.gender"], "Identity: Dysphoria")]
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

    public override ActualGender GetActualGender(Pawn pawn)
    {
        if (pawn.GetCurrentIdentity() == GenderIdentity.Cisgender)
            return base.GetActualGender(pawn);

        var traits = pawn.story?.traits;

        if (traits != null)
        {
            if (traits.HasTrait(DefOfDysphoria.maleGender))
                return ActualGender.Man;
            if (traits.HasTrait(DefOfDysphoria.femaleGender))
                return ActualGender.Woman;
            if (traits.HasTrait(DefOfDysphoria.androgyneGender))
                return ActualGender.Enby;
        }

        return base.GetActualGender(pawn);
    }

    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        foreach (var gender in genders)
            if (pawn.story?.traits?.HasTrait(gender) ?? false)
                return GenderIdentity.Transgender;

        return GenderIdentity.Cisgender;
    }

    public override CultureViewOnTrans CultureOpinionOnTrans(Ideo ideo)
    {
        if (ideo?.HasPrecept(Trans_Abhorrent) ?? false)
            return CultureViewOnTrans.Abhorrent;
        if (ideo?.HasPrecept(Trans_Disapproved) ?? false)
            return CultureViewOnTrans.Despised;
        if (ideo?.HasPrecept(Trans_Approved) ?? false)
            return CultureViewOnTrans.Adored;
        if (ideo?.HasPrecept(Trans_Exalted) ?? false)
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