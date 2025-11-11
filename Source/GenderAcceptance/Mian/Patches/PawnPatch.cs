using System.Text.RegularExpressions;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(Pawn))]
public class PawnPatch
{
    [HarmonyPatch(nameof(Pawn.MainDesc))]
    [HarmonyBefore("lovelydovey.sex.withrosaline")]
    [HarmonyPostfix]
    public static void MainDescPatch(Pawn __instance, bool writeGender, ref string __result)
    {
        if (writeGender && (__instance?.RaceProps?.Humanlike ?? false))
        {
            var prefix = __instance.GetGenderedAppearance().GetGenderNoun() + " " +
                         __instance.GetCurrentIdentity().ToString().ToLower();

            __result = char.ToUpper(prefix[0]) + prefix.Substring(1) + " " + __result.ToLower();
        }
    }
}