using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public abstract class TransDependency : ITransDependency
{
    public abstract GenderIdentity GetCurrentIdentity(Pawn pawn);
    public abstract bool AppearsToHaveMatchingGenitalia(Pawn pawn);
    
    public virtual CultureViewOnTrans CultureOpinionOnTrans(Pawn pawn)
    {
        return pawn.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false ? CultureViewOnTrans.Despised : pawn.Ideo?.HasPrecept(GADefOf.Transgender_Adored) ?? false ? CultureViewOnTrans.Adored : CultureViewOnTrans.Neutral;
    }

    public virtual Gendered GetGendered(Pawn pawn)
    {
        var bodyType = pawn.story?.bodyType;
        var genderPoints = 0;
        if (bodyType != null)
        {
            if (bodyType == BodyTypeDefOf.Male || bodyType == BodyTypeDefOf.Fat || bodyType == BodyTypeDefOf.Hulk)
            {
                genderPoints += 1;
            }
            if (bodyType == BodyTypeDefOf.Female || bodyType == BodyTypeDefOf.Thin)
                genderPoints -= 1;
        }
        
        var styleGender = pawn.story?.hairDef?.styleGender;
        if (styleGender != null)
        {
            switch (styleGender)
            {
                case StyleGender.Male:
                    genderPoints += 2;
                    break;
                case StyleGender.MaleUsually:
                    genderPoints += 1;
                    break;
                case StyleGender.Female:
                    genderPoints -= 2;
                    break;
                case StyleGender.FemaleUsually:
                    genderPoints -= 1;
                    break;
            }
        }

        if (genderPoints > 1)
            return Gendered.Masculine;
        if (genderPoints < -1)
            return Gendered.Feminine;

        return Gendered.Androgynous;
        // fallback onto sex terms if necessary
        // if (pawn.IsEnbyBySexTerm())
        //     return Gendered.Androgynous;
        //
        // return pawn.gender == Gender.Male ? Gendered.Masculine : Gendered.Feminine;
    }
}