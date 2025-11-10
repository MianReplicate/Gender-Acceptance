using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(Verse.Pawn))]
public static class PawnData
{
    [HarmonyPatch(nameof(Verse.Pawn.ExposeData))]
    [HarmonyPostfix]
    public static void GetExtraData(ref Verse.Pawn __instance)
    {
        if (!__instance.RaceProps?.Humanlike ?? false)
            return;
        
        var transknowledge = __instance.GetModifiableTransgenderKnowledge(Scribe.mode == LoadSaveMode.Saving, false);
        
        Scribe_Collections.Look(
            ref transknowledge, 
            "GABelievedToBeTransgenders", 
            LookMode.Deep);
        
        if (Scribe.mode != LoadSaveMode.Saving)
        {
            __instance.SetTransKnowledges(transknowledge);
        }
    }
}