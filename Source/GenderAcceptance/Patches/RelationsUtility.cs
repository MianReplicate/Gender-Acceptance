using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Patches;

[HarmonyPatch(typeof(RimWorld.RelationsUtility))]
public static class RelationsUtility
{
    // Replace the calls last in case any mod tries to mess with the calls
    [HarmonyPatch(nameof(RimWorld.RelationsUtility.RomanceEligiblePair))]
    [HarmonyTranspiler]
    [HarmonyPriority(Priority.Last)]
    public static IEnumerable<CodeInstruction> RomanceEligiblePatch(IEnumerable<CodeInstruction> instructions)
    {
        return Helper.ReplaceGenderAttractionCalls(instructions);
    }
    
    // Way Better Romance removes the gender calls entirely, so let's make sure to run AFTER they remove them so that we don't mess up their thing
    // In case any other mods do something similar, replace the calls last.
    [HarmonyPatch(nameof(RimWorld.RelationsUtility.RomanceOption))]
    [HarmonyTranspiler]
    [HarmonyAfter("rimworld.divineDerivative.romance")]
    [HarmonyPriority(Priority.Last)]
    public static IEnumerable<CodeInstruction> RomanceOptionPatch(IEnumerable<CodeInstruction> instructions)
    {
        return Helper.ReplaceGenderAttractionCalls(instructions);
    }
}