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
        var knownTrannies = __instance.GetKnownTrannies(Scribe.mode == LoadSaveMode.Saving);

        Scribe_Collections.Look(ref knownTrannies, "GAKnownTrannies", LookMode.Reference);

        if (Scribe.mode != LoadSaveMode.Saving)
        {
            TransKnowledge.SetKnownTrannies(__instance, knownTrannies);
        }
    }
}