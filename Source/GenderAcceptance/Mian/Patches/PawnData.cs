using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(Verse.Pawn))]
public class PawnData
{
    [HarmonyPatch(nameof(Verse.Pawn.ExposeData))]
    [HarmonyPostfix]
    public static void GetExtraData(ref Verse.Pawn __instance)
    {
        var knownTrannies = __instance.GetBelievedToBeTrannies(Scribe.mode == LoadSaveMode.Saving);

        Scribe_Collections.Look(ref knownTrannies, "GABelievedToBeTrannies", LookMode.Reference);

        if (Scribe.mode != LoadSaveMode.Saving)
        {
            TransKnowledge.SetBelievedToBeTrannies(__instance, knownTrannies);
        }
    }
}