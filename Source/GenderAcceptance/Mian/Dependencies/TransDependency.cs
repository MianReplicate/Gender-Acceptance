using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public abstract class TransDependency : ITransDependency
{
    public abstract GenderIdentity GetCurrentIdentity(Pawn pawn);
    public abstract bool AppearsToHaveMatchingGenitalia(Pawn pawn);

    public virtual CultureViewOnTrans CultureOpinionOnTrans(Pawn pawn)
    {
        return pawn.Ideo?.HasPrecept(IdeologyGADefOf.Transgender_Despised) ?? false ? CultureViewOnTrans.Despised :
            pawn.Ideo?.HasPrecept(IdeologyGADefOf.Transgender_Adored) ?? false ? CultureViewOnTrans.Adored :
            CultureViewOnTrans.Neutral;
    }

    public virtual float GetGenderedPoints(Pawn pawn)
    {
        var genderPoints = 0;
        
        var bodyType = pawn.story?.bodyType;
        if (bodyType != null)
        {
            var def = BodyTypeGenderedDef.FromBodyType(bodyType);
            if (def != null)
                genderPoints += def.genderPoints;
        }
        
        var genders = pawn.apparel.WornApparel.Select(apparel => apparel.def.apparel.gender).ToList();
        
        var headGender = pawn.story?.headType?.gender;
        if (headGender.HasValue)
        {
            genders.Add(headGender.Value);
        }
        
        foreach (var gender in genders)
        {
            switch (gender)
            {
                case Gender.Female:
                    genderPoints -= 1;
                    break;
                case Gender.Male:
                    genderPoints += 1;
                    break;
            }
        }
        
        var styleGenders = new List<StyleGender>();

        var bodyTattoo = pawn.style?.BodyTattoo?.styleGender;
        if(bodyTattoo.HasValue)
            styleGenders.Add(bodyTattoo.Value);
        
        var faceTattoo = pawn.style?.FaceTattoo?.styleGender;
        if(faceTattoo.HasValue)
            styleGenders.Add(faceTattoo.Value);
        
        var beard = pawn.style?.beardDef?.styleGender;
        if(beard.HasValue)
            styleGenders.Add(beard.Value);

        var hair = pawn.story?.hairDef?.styleGender;
        if(hair.HasValue)
            styleGenders.Add(hair.Value);
        
        foreach (var styleGender in styleGenders)
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