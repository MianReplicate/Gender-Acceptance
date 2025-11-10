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
            var prefix = __instance.GetGenderedAppearance().GetGenderNoun();
            Regex regex =
                new Regex("\\b" + Regex.Escape(__instance.gender.GetLabel(__instance.AnimalOrWildMan())) + "\\b",
                    RegexOptions.IgnoreCase);

            if (regex.IsMatch(__result))
            {
                __result = regex.Replace(__result, match => (ModsConfig.IsActive("lovelydovey.sex.withrosaline") ? prefix : char.ToUpper(prefix[0]) + prefix.Substring(1)) + " " + match.Value.ToLower());
            }
            else
            {
                __result = prefix + " " + __result;
            }
        }
    }
}