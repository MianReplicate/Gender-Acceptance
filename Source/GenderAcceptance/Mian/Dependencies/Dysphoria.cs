using Identity;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public class Dysphoria : TransDependency
{
    private static PreceptDef Trans_Abhorrent = DefDatabase<PreceptDef>.GetNamed("Trans_Abhorrent");
    private static PreceptDef Trans_Disapproved = DefDatabase<PreceptDef>.GetNamed("Trans_Disapproved");
    private static PreceptDef Trans_Neutral = DefDatabase<PreceptDef>.GetNamed("Trans_Neutral");
    private static PreceptDef Trans_Approved = DefDatabase<PreceptDef>.GetNamed("Trans_Approved");
    private static PreceptDef Trans_Exalted = DefDatabase<PreceptDef>.GetNamed("Trans_Exalted");

    private static TraitDef[] genders = new TraitDef[3]
    {
        DefOfDysphoria.maleGender,
        DefOfDysphoria.femaleGender,
        DefOfDysphoria.androgyneGender
    };
    public override GenderIdentity GetCurrentIdentity(Pawn pawn)
    {
        foreach (var gender in genders)
        {
            if (pawn.story?.traits?.HasTrait(gender) ?? false)
            {
                return GenderIdentity.Transgender;
            }   
        }
        return GenderIdentity.Cisgender;
    }

    public override CultureViewOnTrans CultureOpinionOnTrans(Pawn pawn)
    {
        return (pawn.Ideo?.HasPrecept(Trans_Abhorrent) ?? false) || (pawn.Ideo?.HasPrecept(Trans_Disapproved) ?? false) ? CultureViewOnTrans.Despised : 
            (pawn.Ideo?.HasPrecept(Trans_Approved) ?? false) || (pawn.Ideo?.HasPrecept(Trans_Exalted) ?? false) ? CultureViewOnTrans.Adored : CultureViewOnTrans.Neutral;
    }

    public override bool HasMatchingGenitalia(Pawn pawn)
    {
        var breasts = DefOfDysphoria.Breasts;
        var noBreasts = DefOfDysphoria.NoBreasts;
        var hediffs = pawn.health.hediffSet;

        return (hediffs.HasHediff(breasts) && pawn.gender == Gender.Female) || (hediffs.HasHediff(noBreasts) && pawn.gender == Gender.Male);
    }

    public override bool LooksCis(Pawn pawn)
    {
        StatDef femStat = DefOfDysphoria.FemStat;
        StatDef mascStat = DefOfDysphoria.MascStat;

        return (pawn.gender == Gender.Male && pawn.GetStatValue(mascStat) > 50)
            || (pawn.gender == Gender.Female && pawn.GetStatValue(femStat) > 50);
    }
    
    public override bool FeaturesAppearances()
    {
        return true;
    }
}