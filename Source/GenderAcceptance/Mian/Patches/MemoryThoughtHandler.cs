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
        if ((__instance.pawn.story?.traits?.HasTrait(GADefOf.Chaser) ?? false) &&
            newThought.def == ThoughtDefOf.GotSomeLovin && otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender)
        {
            ((Chaser_Need) __instance.pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeed(1f);
        }
    }
}