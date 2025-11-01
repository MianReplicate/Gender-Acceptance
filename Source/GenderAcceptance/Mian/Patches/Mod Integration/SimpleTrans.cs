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
}