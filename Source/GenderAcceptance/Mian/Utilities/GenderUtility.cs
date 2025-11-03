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
        if (pawn.gender != Gender.Male && pawn.gender != Gender.Female)
            return GenderIdentity.Transgender; // nonbinary moment?

        return TransDependencies.TransLibrary.GetCurrentIdentity(pawn);
    }

    // Used to help people figure out who is trans or not.
    public static bool HasMatchingGenitalia(this Pawn pawn)
    {
        if (pawn.gender != Gender.Male && pawn.gender != Gender.Female)
            return true; // nonbinary moment !
        
        return TransDependencies.TransLibrary.HasMatchingGenitalia(pawn);
    }
    
    public static bool LooksCis(this Pawn pawn)
    {
        if (pawn.gender != Gender.Male && pawn.gender != Gender.Female)
            return true;
        
        return TransDependencies.TransLibrary.LooksCis(pawn);
    }
}