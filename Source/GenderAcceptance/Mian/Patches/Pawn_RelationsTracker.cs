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
    public static void AddChaserFactor(Pawn otherPawn, ref float __result, Pawn ___pawn)
    {
        if (GenderUtility.DoesChaserSeeTrans(___pawn, otherPawn))
            __result *= 2;
    }

    [HarmonyPatch(nameof(RimWorld.Pawn_RelationsTracker.AddDirectRelation))]
    [HarmonyPostfix]
    public static void OnDirectRelationAdd(PawnRelationDef def, Pawn otherPawn, Pawn ___pawn,
        RimWorld.Pawn_RelationsTracker __instance)
    {
        if (___pawn.GetCurrentIdentity() != GenderIdentity.Transgender)
            return;

        if (__instance.DirectRelationExists(def, otherPawn) && (otherPawn.RaceProps?.Humanlike ?? false) &&
            (___pawn.RaceProps?.Humanlike ?? false))
        {
            var baseChance = 0.05f;
            if (def.familyByBloodRelation)
                baseChance *= 2f;
            baseChance *= def.importance / 150f;
            baseChance *= def.opinionOffset / 25f;

            if (Rand.Chance(baseChance))
            {
                Helper.Debug("Pawn " + ___pawn.Name + " is out to " + otherPawn.Name + " due to new relation: " +
                             def.defName);
                otherPawn.GetKnowledgeOnPawn(___pawn).cameOut = true;
                otherPawn.GetKnowledgeOnPawn(___pawn).playedNotification = true;
            }
        }
    }
}