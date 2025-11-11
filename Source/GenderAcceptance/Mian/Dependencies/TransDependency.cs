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

        var apparelDefs = pawn.apparel.WornApparel.Select(apparel => apparel.def);
        var overrideList = pawn.ideo?.Ideo?.GetAllPreceptsOfType<Precept_Apparel>().Where(precept => apparelDefs.Contains(precept.apparelDef)).ToList();
        var genders = apparelDefs.Select(apparel =>
        {
            var preceptForApparel = overrideList?.Find(precept => precept.apparelDef == apparel);
            return preceptForApparel != null ? preceptForApparel.TargetGender : apparel.apparel.gender;
        }).ToList();
        
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

        var styleItems = new List<StyleItemDef>();

        var bodyTattoo = pawn.style?.BodyTattoo;
        if(bodyTattoo != null)
            styleItems.Add(bodyTattoo);
        
        var faceTattoo = pawn.style?.FaceTattoo;
        if(faceTattoo != null)
            styleItems.Add(faceTattoo);
        
        var beard = pawn.style?.beardDef;
        if(beard != null)
            styleItems.Add(beard);

        var hair = pawn.story?.hairDef;
        if(hair != null)
            styleItems.Add(hair);

        var ideoStyle = pawn.ideo?.Ideo?.style;
        var styleGenders = styleItems.Select(item => 
            ideoStyle != null ? ideoStyle.GetGender(item) : item.styleGender).ToList();
        
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