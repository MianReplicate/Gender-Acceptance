using HarmonyLib;
using Identity;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Patches.Mod_Integration;

public static class Dysphoria
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(AccessTools.Method(typeof(ThoughtWorker_Precept_IsTrans_Social), "ShouldHaveThought"),
            prefix: typeof(Dysphoria).GetMethod(nameof(ReplaceIsTransSocial)));
    }

    public static bool ReplaceIsTransSocial(Pawn p, Pawn otherPawn, ref ThoughtState __result)
    {
        __result = p.BelievesIsTrans(otherPawn);
        return false;
    }
}