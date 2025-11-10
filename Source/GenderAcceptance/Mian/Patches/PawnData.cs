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
        var knowntransgenders = __instance.GetModifiableKnownTransgenders(Scribe.mode == LoadSaveMode.Saving);
        
        if (knowntransgenders != null)
        {
            Scribe_Collections.Look<Pawn, TransKnowledge>(ref knowntransgenders, "GABelievedToBeTransgenders", LookMode.Reference, LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                __instance.SetTransKnowledges(knowntransgenders);
            }   
        }
    }
}