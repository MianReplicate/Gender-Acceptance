using HarmonyLib;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.Faction))]
public class Faction
{
    [HarmonyPatch(nameof(RimWorld.Faction.Notify_PawnJoined))]
    [HarmonyPostfix]
    public static void PawnJoined(RimWorld.Faction __instance, Pawn p)
    {
        var joinerPawn = p;

        foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
        {
            if (pawn.Faction != __instance)
                continue;
            if (p.BelievesIsTrans(joinerPawn))
            {
                var transphobic = pawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false;
                if (!transphobic && pawn.GetCurrentIdentity() != GenderIdentity.Transgender)
                    continue;
                pawn.needs.mood.thoughts.memories.TryGainMemory(transphobic ? GADefOf.Transgender_Person_Joined_Negative : GADefOf.Transgender_Person_Joined_Positive, joinerPawn);
            }
            else
            {
                var cisphobic = pawn.story?.traits?.HasTrait(GADefOf.Cisphobic) ?? false;
                if (!cisphobic)
                    continue;
                pawn.needs.mood.thoughts.memories.TryGainMemory(GADefOf.Cisgender_Person_Joined, joinerPawn);
            }
        }
    }
}