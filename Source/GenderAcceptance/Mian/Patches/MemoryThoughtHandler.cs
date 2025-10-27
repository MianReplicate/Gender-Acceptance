using HarmonyLib;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.MemoryThoughtHandler))]
public static class MemoryThoughtHandler
{
    [HarmonyPatch(nameof(RimWorld.MemoryThoughtHandler.TryGainMemory), [typeof(Thought_Memory), typeof(Pawn)])]
    [HarmonyPostfix]
    public static void GainTransIntimacyFromLovin(RimWorld.MemoryThoughtHandler __instance, Thought_Memory newThought, Pawn otherPawn)
    {
        if (newThought.def == ThoughtDefOf.GotSomeLovin && Helper.ChaserSeesFetish(__instance.pawn, otherPawn))
        {
            ((Chaser_Need) __instance.pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeedFromSex();
        }
    }
}