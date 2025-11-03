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
    public static void AddExtraRules(Pawn pawn, string pawnSymbol, Dictionary<string, string> constants=null)
    {
        string prefix = "";
        if (!pawnSymbol.NullOrEmpty())
            prefix = $"{prefix}{pawnSymbol}_";
        
        if(constants != null)
            constants[prefix + "isTransphobic"] = pawn.IsTrannyphobic().ToString();
    }
}