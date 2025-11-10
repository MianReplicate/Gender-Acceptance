using System.Collections.Generic;
using GenderAcceptance.Mian.Needs;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.MemoryThoughtHandler))]
public static class MemoryThoughtHandler
{
    [HarmonyPatch(nameof(RimWorld.MemoryThoughtHandler.TryGainMemory), typeof(Thought_Memory), typeof(Pawn))]
    [HarmonyPostfix]
    public static void LovinThoughtApplied(RimWorld.MemoryThoughtHandler __instance, Thought_Memory newThought,
        Pawn otherPawn)
    {
        // this is like the universal sex thought
        if (newThought.def == ThoughtDefOf.GotSomeLovin)
        {
            Dictionary<string, string> constants =
                new()
                {
                    { "didSex", "True" }
                };
            var sus = !otherPawn.AppearsToHaveMatchingGenitalia() || Rand.Chance(0.01f);
            if (!otherPawn.AppearsToHaveMatchingGenitalia()) constants.Add("mismatchedGenitalia", "True");

            if (sus)
            {
                __instance.pawn.GetKnowledgeOnPawn(otherPawn).sex = true;
                TransKnowledgeManager.OnKnowledgeLearned(__instance.pawn, otherPawn, LetterDefOf.NeutralEvent,
                    constants: constants);
            }

            if (GenderUtility.DoesChaserSeeTrans(__instance.pawn, otherPawn))
            {
                ((Chaser_Need)__instance.pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeedFromSex();
                otherPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(GADefOf.Dehumanized, __instance.pawn);
            }
        }
    }
}