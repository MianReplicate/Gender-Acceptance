using System.Collections.Generic;
using BetterRomance;
using BetterRomance.HarmonyPatches;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderAcceptance.Patches.Mod_Integration;

public static class WayBetterRomance
{
    public static void Patch(Harmony harmony)
    {
        // The below patches just replace gender attraction calls with our own so that the transphobia trait will work properly
        // (transphobic people don't believe trans people are the gender they say they are)
        
        harmony.Patch(typeof(SexualityUtility).GetMethod(nameof(SexualityUtility.CouldWeBeLovers)),
            prefix: typeof(WayBetterRomance).GetMethod(nameof(CouldWeBeLoversReplacement)));

        harmony.Patch(typeof(SexualityUtility).GetMethod(nameof(SexualityUtility.WouldConsiderMarriage)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceGenderAttractionCalls)));
        
        harmony.Patch(typeof(SexualityUtility).GetMethod(nameof(SexualityUtility.CouldWeBeMarried)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceGenderAttractionCalls)));

        harmony.Patch(typeof(HookupUtility).GetMethod(nameof(HookupUtility.HookupOption)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceGenderAttractionCalls)));
        
        harmony.Patch(typeof(HookupUtility).GetMethod(nameof(HookupUtility.HookupEligiblePair)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceGenderAttractionCalls)));
        
        harmony.Unpatch(typeof(InteractionWorker_Breakup).GetMethod(nameof(InteractionWorker_Breakup.RandomSelectionWeight))
        , typeof(InteractionWorker_Breakup_RandomSelectionWeight)
            .GetMethod(nameof(InteractionWorker_Breakup_RandomSelectionWeight.Postfix)));
        harmony.Patch(typeof(InteractionWorker_Breakup).GetMethod(nameof(InteractionWorker_Breakup.RandomSelectionWeight)),
            postfix: typeof(WayBetterRomance).GetMethod(nameof(InteractionWorker_Breakup_RandomSelectionWeight_Postfix)));
    }

    // Required for the transphobia trait to work properly
    public static bool CouldWeBeLoversReplacement(Pawn first, Pawn second, ref bool __result)
    {
        __result = Helper.AttractedToEachOther(first, second);
        return false;
    }
    
    // Changes from Way Better Romance:
    // Changed AttractedToGender method to AttractedToPerson to account for transphobia
    public static void InteractionWorker_Breakup_RandomSelectionWeight_Postfix(Pawn initiator, Pawn recipient, ref float __result)
    {
        //This increases chances if gender does not match sexuality
        if (!Helper.AttractedToPerson(initiator, recipient))
        {
            __result *= 2f;
        }
        if (initiator.IsAsexual() && !recipient.IsAsexual())
        {
            __result *= 1.5f;
        }
    }
}