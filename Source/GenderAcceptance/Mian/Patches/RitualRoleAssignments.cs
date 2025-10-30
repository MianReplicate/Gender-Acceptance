using HarmonyLib;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.RitualRoleAssignments))]
public static class RitualRoleAssignments
{
    [HarmonyPatch(nameof(RimWorld.RitualRoleAssignments.CanEverSpectate))]
    [HarmonyPrefix]
    public static bool CanEverSpectate(RimWorld.RitualRoleAssignments __instance, Pawn pawn, ref bool __result)
    {
        if (__instance.Ritual.behavior is RitualBehaviorWorker_GenderAffirmParty && pawn.IsTransphobic())
        {
            __result = false;
            return false;
        }

        return true;
    }
}