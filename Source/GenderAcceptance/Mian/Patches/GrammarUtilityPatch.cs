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
    public static IEnumerable<Rule> AddExtraRules(IEnumerable<Rule> __result, Pawn pawn, string pawnSymbol, Dictionary<string, string> constants=null)
    {
        string prefix = "";
        if (!pawnSymbol.NullOrEmpty())
            prefix = $"{prefix}{pawnSymbol}_";

        if (constants != null)
        {
            constants[prefix + "isTransphobic"] = pawn.GetTransphobicStatus().ToString();
        }

        foreach (var rule in __result)
        {
            yield return rule;
        }
        
        yield return new Rule_String(prefix + "gender", pawn.gender.GetGenderNoun());
        yield return new Rule_String(prefix + "otherGender", pawn.GetOppositeGender().GetGenderNoun());
    }
}