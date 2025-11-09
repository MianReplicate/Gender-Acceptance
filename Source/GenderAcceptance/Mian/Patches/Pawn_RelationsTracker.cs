using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.Pawn_RelationsTracker))]
public static class Pawn_RelationsTracker
{
    // 200x the factor for sexxx!
    [HarmonyPatch(nameof(RimWorld.Pawn_RelationsTracker.SecondaryLovinChanceFactor))]
    [HarmonyPostfix]
    public static void AddChaserFactor(Pawn otherPawn, ref float __result, ref RimWorld.Pawn_RelationsTracker __instance, Pawn ___pawn)
    {
        if (GenderUtility.DoesChaserSeeTrans(___pawn, otherPawn))
            __result *= 2;
    }
}