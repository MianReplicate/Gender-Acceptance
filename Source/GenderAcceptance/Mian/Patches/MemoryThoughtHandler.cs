using GenderAcceptance.Mian.Needs;
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
    public static void ChaserAndTrannyFucked(RimWorld.MemoryThoughtHandler __instance, Thought_Memory newThought, Pawn otherPawn)
    {
        if (newThought.def == ThoughtDefOf.GotSomeLovin)
        {
            if(!otherPawn.HasMatchingGenitalia())
                TransKnowledge.KnowledgeLearned(__instance.pawn, otherPawn, false);
            
            if (GenderUtility.DoesChaserSeeTranny(__instance.pawn, otherPawn))
            {
                ((Chaser_Need) __instance.pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeedFromSex();
                otherPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(GADefOf.Dehumanized, __instance.pawn);   
            }
        }
    }
}