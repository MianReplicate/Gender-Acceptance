using System;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Utilities;

public static class GenderUtility
{
    /// <summary>
    ///     Gets the chaser factor for if a pawn finds another more attractive because of their chaserness
    /// </summary>
    /// <param name="pawn">The chaser</param>
    /// <param name="target">The receiver</param>
    /// <returns>The value for being more attractive to a chaser</returns>
    public static float ChaserFactor(Pawn pawn, Pawn target)
    {
        if (pawn.FindsExtraordinarilyAttractive(target))
            return 2f;
        return 0f;
    }

    /// <summary>
    ///     If initiator is a chaser, and the recipient is transgender according to the initiator's pov and they are attracted to their gender, they will find them more attractive
    /// </summary>
    /// <param name="initiator">The chaser pawn</param>
    /// <param name="recipient">The receiver</param>
    /// <returns>Whether initiator is a chaser and if they find recipient particularly attractive for being trans</returns>
    public static bool FindsExtraordinarilyAttractive(this Pawn initiator, Pawn recipient)
    {
        if (!recipient.RaceProps.Humanlike)
            return false;
        if ((initiator.story?.traits?.HasTrait(GADefOf.Chaser) ?? false) && initiator.BelievesIsTrans(recipient))
            return RelationsUtility.AttractedToGender(initiator, recipient.gender);
        return false;
    }

    /// <summary>
    ///     Makes a status log of what makes a pawn transphobic towards another. Non-humanlike pawns cannot be considered transphobic.
    /// </summary>
    /// <param name="pawn">The pawn to check for transphobia</param>
    /// <param name="recipient">The receiver</param>
    /// <returns>The status of the pawn's transphobia to another</returns>    
    public static TransphobicStatus GetTransphobicStatus(this Pawn pawn, Pawn recipient = null)
    {
        if (pawn?.RaceProps?.Humanlike ?? false)
        {
            var chaser = recipient != null
                ? FindsExtraordinarilyAttractive(pawn, recipient)
                : pawn.story?.traits?.HasTrait(GADefOf.Chaser) ?? false;
            var transphobicTrait = pawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false;
            var transphobicPrecept = pawn.GetCurrentIdentity() == GenderIdentity.Cisgender
                                     && (pawn.Ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Despised ||
                                         pawn.Ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Abhorrent);

            return new TransphobicStatus
            {
                GenerallyTransphobic = chaser || transphobicTrait || transphobicPrecept,
                ChaserAttributeCounts = chaser,
                HasTransphobicTrait = transphobicTrait,
                TransphobicPreceptCounts = transphobicPrecept
            };   
        }

        return new TransphobicStatus();
    }

    /// <summary>
    ///     Gets the opposite biological sex for a pawn (or None if neither)
    /// </summary>
    /// <param name="pawn">The pawn to use</param>
    /// <returns>The opposite biological sex</returns>
    public static Gender GetOppositeGender(this Pawn pawn)
    {
        return pawn.gender == Gender.Female ? Gender.Male : pawn.gender == Gender.Male ? Gender.Female : Gender.None;
    }
    
    /// <summary>
    ///     Counts the amount of a specific gender there is on a map from a pawn's pov
    /// </summary>
    /// <param name="perceiver">The pawn to perceive from</param>
    /// <returns>The amount of gendered individuals there are</returns>
    public static int CountGenderIndividuals(Pawn perceiver, GenderIdentity gender)
    {
        var count = 0;
        var colonists = perceiver.Map.mapPawns.FreeColonists;

        foreach (var pawn in colonists)
        {
            if (pawn == perceiver) continue;
            var believesIsTrans = perceiver.BelievesIsTrans(pawn);
            var identity = believesIsTrans ? GenderIdentity.Transgender : GenderIdentity.Cisgender;
            
            if (pawn.Dead || identity != gender) continue;

            count++;
        }

        return count;
    }

    /// <summary>
    ///     Checks whether the culture is transphobic, accepting or neutral
    /// </summary>
    /// <param name="ideo">The culture to check</param>
    /// <returns>Whether the culture is transphobic, accepting or neutral</returns>
    public static CultureViewOnTrans CultureOpinionOnTrans(this Ideo ideo)
    {
        return TransDependencies.TransLibrary.CultureOpinionOnTrans(ideo);
    }

    /// <summary>
    ///     Retrieves whether the pawn is transgender or cisgender. Defaults to true for enby folks
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The pawn's gender identity</returns>
    public static GenderIdentity GetCurrentIdentity(this Pawn pawn)
    {
        if (pawn.IsEnby())
            return GenderIdentity.Transgender; // nonbinary moment?

        return TransDependencies.TransLibrary.GetCurrentIdentity(pawn);
    }

    /// <summary>
    ///     Gets the pawn's gender identity (not their sex)
    /// </summary>
    /// <param name="pawn">The pawn</param>
    /// <returns>The pawn's gender identity</returns>
    public static ActualGender GetActualGender(this Pawn pawn)
    {
        return TransDependencies.TransLibrary.GetActualGender(pawn);
    }

    /// <summary>
    ///     Determines whether the pawn's genitalia matches up with their gender identity. Defaults to true for androgynous folks.
    /// </summary>
    /// <param name="pawn">THe pawn to check</param>
    /// <returns>Whether genitalia matches up with the pawn's gender identity or not</returns>
    public static bool AppearsToHaveMatchingGenitalia(this Pawn pawn)
    {
        if (!pawn.RaceProps.Humanlike)
            return true;
        if (pawn.GetGenderedAppearance() == Gendered.Androgynous)
            return true;

        return TransDependencies.TransLibrary.AppearsToHaveMatchingGenitalia(pawn);
    }

    /// <summary>
    ///     Gets the gendered appearance for a gender (This might be altered in the future to be more complex based on different cultures)
    /// </summary>
    /// <param name="gender">The gender to check</param>
    /// <returns>The gendered appearance for the gender</returns>
    public static Gendered GetGenderedAppearance(this Gender gender)
    {
        switch (gender)
        {
            case Gender.Male:
                return Gendered.Masculine;
            case Gender.Female:
                return Gendered.Feminine;
            default:
                return Gendered.Androgynous;
        }
    }

    /// <summary>
    ///     Gets appearance based on their gendered points
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>Whether they are masculine, feminine, or androgynous</returns>
    public static Gendered GetGenderedAppearance(this Pawn pawn)
    {
        var genderedPoints = pawn.GetGenderedPoints();
        return genderedPoints > 1 ? Gendered.Masculine : genderedPoints < -1 ? Gendered.Feminine : Gendered.Androgynous;
    }
    
    /// <summary>
    ///     Calculates a pawn's gendered points, the higher the more masculine, the lower the more feminine
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The gendered points for the pawn</returns>
    public static float GetGenderedPoints(this Pawn pawn)
    {
        if(!pawn.RaceProps.Humanlike)
            throw new ArgumentException($"{pawn.Name} must be humanlike to get gendered points!");
        
        return TransDependencies.TransLibrary.GetGenderedPoints(pawn);
    }

    /// <summary>
    ///     Determines whether one is enby by if their biological sex is not male or female
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>Whether a pawn is enby or not</returns>

    private static bool IsEnbyBySexTerm(this Pawn pawn)
    {
        return pawn.gender != Gender.Male && pawn.gender != Gender.Female;
    }

    /// <summary>
    /// Checks for whether a pawn is enby or not. This is a method meant to be patched by other mods that may add their own enby pawn variants
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>Whether a pawn is enby or not</returns>
    public static bool IsEnby(this Pawn pawn)
    {
        return pawn.IsEnbyBySexTerm(); // A catch-all for any enby mods that might add a third sex
    }

    /// <summary>
    ///     Calculates a pawn's gendered points and makes it relative to their identity
    ///     The higher the points, the more their appearance fits their identity. The lower, the more their appearance does not
    ///     fit.
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The relative gendered points for the pawn</returns>
    public static float CalculateRelativeAppearanceFromIdentity(this Pawn pawn)
    {
        var points = pawn.GetGenderedPoints();

        if (pawn.GetActualGender() == ActualGender.Man)
            return points;
        if (pawn.GetActualGender() == ActualGender.Woman)
        {
            return points < 0 ? Math.Abs(points) : -points;
        }

        return -Math.Abs(points); // assume they are enby
    }

    public static float AppearanceMismatchChanceFactor(this Pawn pawn)
    {
        return AppearanceMismatchFactorCurve.Evaluate(pawn.CalculateRelativeAppearanceFromIdentity());
    }
    
    private static readonly SimpleCurve AppearanceMismatchFactorCurve = new SimpleCurve
    {
        {
            new CurvePoint(0f, 0f)
        },
        {
            new CurvePoint(-1f, 1f)
        },
        {
            new CurvePoint(-2f, 2f)
        },
        {
            new CurvePoint(-4f, 3f)
        },
        {
            new CurvePoint(-6f, 4f)
        },
        {
            new CurvePoint(-10f, 6f)
        }
    };
    
}