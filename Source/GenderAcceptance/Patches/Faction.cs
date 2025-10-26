using HarmonyLib;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Patches;

[HarmonyPatch(typeof(RimWorld.Faction))]
public class Faction
{
    [HarmonyPatch(nameof(RimWorld.Faction.Notify_PawnJoined))]
    [HarmonyPostfix]
    public static void PawnJoined(RimWorld.Faction __instance, Pawn p)
    {
        var joinerPawn = p;
        if (joinerPawn.GetCurrentIdentity() == GenderIdentity.Cisgender)
            return;

        foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
        {
            if (pawn.Faction != __instance)
                continue;
            var transphobic = pawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false;
            if (!transphobic && pawn.GetCurrentIdentity() != GenderIdentity.Transgender)
                continue;
            var thought = ThoughtMaker.MakeThought(GADefOf.TransgenderPersonJoined, transphobic ? 1 : 0);
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
        }
    }
}