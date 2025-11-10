using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(Verse.GenderUtility))]
public static class GenderUtilityPatch
{
    [HarmonyPatch(nameof(Verse.GenderUtility.GetGenderLabel))]
    [HarmonyAfter("lovelydovey.sex.withrosaline")]
    [HarmonyPostfix]
    public static void AddAppearance(Pawn pawn, ref string __result)
    {
        if (pawn?.RaceProps?.Humanlike ?? false)
        {
            __result = pawn.GetGenderedAppearance().GetGenderNoun() + " " + __result;
        }
    }
}