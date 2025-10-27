using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.Pawn_InteractionsTracker))]
public class Pawn_InteractionsTracker
{
    [HarmonyPatch(nameof(RimWorld.Pawn_InteractionsTracker.TryInteractWith))]
    [HarmonyPostfix]
    public static void TryInteractWith(RimWorld.Pawn_InteractionsTracker __instance, bool __result, Pawn recipient)
    {
        if (__result)
        {
            if(__instance.)
        }
    }
}