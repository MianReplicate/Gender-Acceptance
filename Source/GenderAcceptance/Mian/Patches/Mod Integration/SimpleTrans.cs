using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Patches.Mod_Integration;

public static class SimpleTrans
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(typeof(RitualRoleAssignments).GetMethod(nameof(RitualRoleAssignments.CanEverSpectate)),
            prefix: typeof(SimpleTrans).GetMethod(nameof(CanEverSpectate)));
        harmony.Patch(typeof(RitualOutcomeEffectWorker_GenderAffirmParty).GetMethod(nameof(RitualOutcomeEffectWorker_GenderAffirmParty.Apply)),
            prefix: typeof(SimpleTrans).GetMethod(nameof(ApplyKnowledgeEffects)));
    }
    
    public static bool CanEverSpectate(RitualRoleAssignments __instance, Pawn pawn, ref bool __result)
    {
        if (__instance.Ritual.behavior is RitualBehaviorWorker_GenderAffirmParty && pawn.IsTrannyphobic())
        {
            __result = false;
            return false;
        }

        return true;
    }
    
    public static void ApplyKnowledgeEffects(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
    {
        Pawn celebrant = jobRitual.assignments?.FirstAssignedPawn("Celebrant");
        if (celebrant == null)
            return;
        foreach (var keyValuePair in totalPresence)
        {
            var pawn = keyValuePair.Key;
            TransKnowledge.KnowledgeLearned(pawn, celebrant, true);
        }
    }
}