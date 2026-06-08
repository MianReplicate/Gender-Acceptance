using System.Collections.Generic;
using System.Linq;
using GenderAcceptance.Mian.Utilities;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public abstract class TransDependency
{
    /// <summary>
    ///     Gets the pawn's gender identity (not their sex)
    /// </summary>
    /// <param name="pawn">The pawn</param>
    /// <returns>The pawn's gender identity</returns>
    public virtual ActualGender GetActualGender(Pawn pawn)
    {
        if (pawn.IsEnby())
            return ActualGender.Enby;
        
        return pawn.gender == Gender.Male ? ActualGender.Man : ActualGender.Woman;
    }
    
    /// <summary>
    ///     Retrieves whether the pawn is transgender or cisgender
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The pawn's trans status</returns>
    public abstract GenderIdentity GetCurrentIdentity(Pawn pawn);
    
    /// <summary>
    ///     Determines whether the pawn's genitalia matches up with their gender identity
    /// </summary>
    /// <param name="pawn">THe pawn to check</param>
    /// <returns>Whether genitalia matches up with the pawn's gender identity or not</returns>
    public abstract bool AppearsToHaveMatchingGenitalia(Pawn pawn);

    /// <summary>
    ///     Checks whether the culture is transphobic, accepting or neutral
    /// </summary>
    /// <param name="ideo">The culture to check</param>
    /// <returns>Whether the culture is transphobic, accepting or neutral</returns>
    public virtual CultureViewOnTrans CultureOpinionOnTrans(Ideo ideo)
    {
        return ideo?.HasPrecept(IdeologyGADefOf.Transgender_Despised) ?? false ? CultureViewOnTrans.Despised :
            ideo?.HasPrecept(IdeologyGADefOf.Transgender_Adored) ?? false ? CultureViewOnTrans.Adored :
            CultureViewOnTrans.Neutral;
    }

    /// <summary>
    ///     Calculates how gendered a pawn is depending on the trans mod used.
    ///     The higher the points, the more masculine. The closer to zero, the more androgynous. If below 0, they are feminine.
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The gendered points for the pawn</returns>
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
    
    /// <summary>
    /// Checks for whether a pawn is enby or not.
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>Whether a pawn is enby or not</returns>
    public virtual bool IsEnby(Pawn pawn)
    {
        return pawn.gender != Gender.Male && pawn.gender != Gender.Female;
    }
}