using System.Collections.Generic;
using HarmonyLib;
using Verse;
using Verse.Grammar;

namespace GenderAcceptance.Mian.Patches;


[HarmonyPatch(typeof(GrammarUtility))]
public static class GrammarUtilityPatch
{
    [HarmonyPatch(nameof(GrammarUtility.RulesForPawn), typeof(string), typeof(Pawn), typeof(Dictionary<string, string>),
        typeof(bool), typeof(bool))]
    [HarmonyPostfix]
    public static void AddExtraRules(IEnumerable<Rule> __result, Pawn pawn, string pawnSymbol) => 
        __result.AddItem(new Rule_String(pawnSymbol + "_isTransphobic", (pawn.IsTrannyphobic()).ToString()));
}