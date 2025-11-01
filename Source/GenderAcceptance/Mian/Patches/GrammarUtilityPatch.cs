using HarmonyLib;
using Verse.Grammar;

namespace GenderAcceptance.Mian.Patches;


[HarmonyPatch(typeof(GrammarUtility))]
public class GrammarUtilityPatch
{
    // [HarmonyPatch(nameof(GrammarUtility.RulesForPawn))]
}