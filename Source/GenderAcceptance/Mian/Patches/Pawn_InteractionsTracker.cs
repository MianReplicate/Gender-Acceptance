using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Needs;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.Pawn_InteractionsTracker))]
public class Pawn_InteractionsTracker
{
    [HarmonyPatch(nameof(RimWorld.Pawn_InteractionsTracker.TryInteractWith))]
    [HarmonyPostfix]
    public static void TryInteractWith(Pawn ___pawn, bool __result, Pawn recipient)
    {
        if (!__result || !recipient.RaceProps.Humanlike)
            return;
        if (GenderUtility.DoesChaserSeeTranny(___pawn, recipient))
            ((Chaser_Need) ___pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeedFromInteraction();
        ___pawn.AttemptTransvestigate(recipient, 0.01f, 0.05f);
    }
}