using GenderAcceptance.Mian;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public abstract class TransDependency : ITransDependency
{
    public abstract GenderIdentity GetCurrentIdentity(Pawn pawn);
    public abstract bool AppearsToHaveMatchingGenitalia(Pawn pawn);
    
    public virtual CultureViewOnTrans CultureOpinionOnTrans(Pawn pawn)
    {
        return pawn.Ideo?.HasPrecept(IdeologyGADefOf.Transgender_Despised) ?? false ? CultureViewOnTrans.Despised : pawn.Ideo?.HasPrecept(IdeologyGADefOf.Transgender_Adored) ?? false ? CultureViewOnTrans.Adored : CultureViewOnTrans.Neutral;
    }

    public virtual float GetGenderedPoints(Pawn pawn)
    {
        var bodyType = pawn.story?.bodyType;
        var genderPoints = 0;
        if (bodyType != null)
        {
            var def = BodyTypeGenderedDef.FromBodyType(bodyType);
            if(def != null)
                genderPoints += def.genderPoints;
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

        return genderPoints;
    }
}