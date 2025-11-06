using System;
using System.Collections.Generic;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian;

public static class GenderUtility {

// Transphobic people see trans people as their AGAB, therefore a straight transphobic man could be attracted to a trans man based on this code
    // public static IEnumerable<CodeInstruction> ReplaceAttractGenderWithPerceivedGender(IEnumerable<CodeInstruction> instructions)
    // {
    //     var codes = new List<CodeInstruction>(instructions);
    //     for (var i = 0; i < codes.Count; i++)
    //     {
    //         // otherPawn.gender => Helper.AttractedToPerson(pawn, otherPawn)
    //         if (codes[i].LoadsField(AccessTools.Field(typeof(Pawn), nameof(Pawn.gender)))
    //             && codes[i + 1].Calls(AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.AttractedToGender))))
    //         {
    //             codes[i] = CodeInstruction.Call(typeof(Helper), nameof(AttractedToPerson));
    //             codes.RemoveAt(i + 1);
    //         }
    //     }
    //     
    //     return codes.AsEnumerable();
    // }

    public static float ChaserFactor(Pawn pawn, Pawn target)
    {
        if (DoesChaserSeeTranny(pawn, target))
            return 2f;
        return 0f;
    }
        
    // does a chaser find the tranny??
    public static bool DoesChaserSeeTranny(Pawn initiator, Pawn recipient)
    {
        if (!recipient.RaceProps.Humanlike)
            return false;
        if ((initiator.story?.traits?.HasTrait(GADefOf.Chaser) ?? false) && initiator.BelievesIsTrans(recipient))
            return RelationsUtility.AttractedToGender(initiator, recipient.gender);
        return false;
    }

    public static bool IsTrannyphobic(this Pawn pawn, bool includePrecept=true)
    {
        return ((pawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false) || (pawn.story?.traits?.HasTrait(GADefOf.Chaser) ?? false)) || 
               (includePrecept && pawn.GetCurrentIdentity() == GenderIdentity.Cisgender 
                               && pawn.CultureOpinionOnTrans() == CultureViewOnTrans.Despised);
    }

    public static Gender GetOppositeGender(this Pawn pawn)
    {
        return pawn.gender == Gender.Female ? Gender.Male : pawn.gender == Gender.Male ? Gender.Female : Gender.None;
    }

    // public static Gender GetPerceivedGender(Pawn initiator, Pawn recipient)
    // {
    //     if (recipient.GetCurrentIdentity() != GenderIdentity.Transgender)
    //         return recipient.gender;
    //
    //     if (recipient.gender != Gender.Male && recipient.gender != Gender.Female)
    //         return recipient.gender; // idk what to do otherwise if im being honest. e.g. nonbinary people
    //         
    //     return initiator.IsTrannyphobic() ? recipient.gender == Gender.Male ? Gender.Female : Gender.Male : recipient.gender;
    // }
    //
    // public static bool AttractedToPerson(Pawn initiator, Pawn recipient)
    // {
    //     return RelationsUtility.AttractedToGender(initiator, GetPerceivedGender(initiator, recipient));
    // }

    // public static bool AttractedToEachOther(Pawn pawn1, Pawn pawn2)
    // {
    //     return AttractedToPerson(pawn1, pawn2) && AttractedToPerson(pawn2, pawn1);
    // }
        
    public static int CountGenderIndividuals(Pawn perceiver, GenderIdentity gender)
    {
        int count = 0;
        List<Pawn> colonists = perceiver.Map.mapPawns.FreeColonists;

        foreach (Pawn pawn in colonists)
        {
            if (pawn.Dead || (pawn.BelievesIsTrans(pawn) && gender == GenderIdentity.Cisgender)) continue;

            count++;
        }
        return count;
    }

    public static CultureViewOnTrans CultureOpinionOnTrans(this Pawn pawn)
    {
        return TransDependencies.TransLibrary.CultureOpinionOnTrans(pawn);
    }
    public static GenderIdentity GetCurrentIdentity(this Pawn pawn)
    {
        if (pawn.IsEnbyBySexTerm())
            return GenderIdentity.Transgender; // nonbinary moment?

        return TransDependencies.TransLibrary.GetCurrentIdentity(pawn);
    }

    // Used to help people figure out who is trans or not.
    public static bool AppearsToHaveMatchingGenitalia(this Pawn pawn)
    {
        if (pawn.GetGenderedAppearance() == Gendered.Androgynous)
            return true;
        
        return TransDependencies.TransLibrary.AppearsToHaveMatchingGenitalia(pawn);
    }

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
    public static Gendered GetGenderedAppearance(this Pawn pawn)
    {
        var genderedPoints = pawn.GetGenderedPoints();
        return genderedPoints > 1 ? Gendered.Masculine : genderedPoints < -1 ? Gendered.Feminine : Gendered.Androgynous;
    }
    public static float GetGenderedPoints(this Pawn pawn)
    {
        return TransDependencies.TransLibrary.GetGenderedPoints(pawn);
    }


    public static bool IsEnbyBySexTerm(this Pawn pawn)
    {
        return pawn.gender != Gender.Male && pawn.gender != Gender.Female;
    }

    /// <summary>
    /// Calculates a pawn's gendered points and makes it relative to their identity
    /// The higher the points, the more their appearance fits their identity. The lower, the more their appearance does not fit.
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The relative gendered points for the pawn</returns>
    public static float CalculateRelativeAppearanceFromIdentity(this Pawn pawn)
    {
        var points = pawn.GetGenderedPoints();
        
        if (pawn.gender == Gender.Male)
            return points;
        if (pawn.gender == Gender.Female)
        {
            if (points < 0)
                return Math.Abs(points);
            else
                return -points;
        }
        
        return -Math.Abs(points); // assume they are enby
    }
}