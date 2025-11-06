using System;
using System.Collections.Generic;
using System.Linq;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace GenderAcceptance.Mian;

public static class TransKnowledge
{
    private static Dictionary<string, string> defaultRules =
    new(){
        {"didSex", "False"},
        {"mismatchedGenitalia", "False"},
        {"transvestigate", "False"}
    };
    
    private static readonly Dictionary<Pawn, List<Pawn>> believedToBeTransgender = new Dictionary<Pawn, List<Pawn>>();

    public static void SetBelievedToBeTrannies(Pawn pawn, List<Pawn> pawns)
    {
        believedToBeTransgender.SetOrAdd(pawn, pawns);
    }
    
    public static List<Pawn> GetBelievedToBeTrannies(this Pawn pawn, bool cleanReferences)
    {
        if (believedToBeTransgender.TryGetValue(pawn, out var pawns)){
            // if(cleanReferences)
                // pawns.RemoveWhere(pawnRef => pawnRef.Dead);
            return pawns;
        }
        return null;
    }
    public static void KnowledgeLearned(Pawn pawn, Pawn otherPawn, bool hardLearned, Dictionary<string, string> rules=null)
    {
        var list = believedToBeTransgender.TryGetValue(pawn, new());
        if (list.Contains(otherPawn))
            return;
        if (rules != null && !rules.All(element => defaultRules.ContainsKey(element.Key)))
        {
            Helper.Error("Invalid rules given!");
            return;
        }
        list.Add(otherPawn);
        SetBelievedToBeTrannies(pawn, list);

        if (!hardLearned)
        {
            var request = new GrammarRequest();

            if (rules == null)
                rules = defaultRules;
            else
                rules.AddRange(defaultRules.Where(rule => !rules.ContainsKey(rule.Key)).ToDictionary(pair => pair.Key, pair => pair.Value));
            request.Includes.Add(GADefOf.Suspicions_About_Trans);
            request.Constants.AddRange(rules);
            request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", pawn, request.Constants));
            request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", otherPawn, request.Constants));
            
            var text = GrammarResolver.Resolve(
                    GADefOf.Suspicions_About_Trans.FirstRuleKeyword,
                    request, "extraSentencePack",
                    false,
                    GADefOf.Suspicions_About_Trans.FirstUntranslatedRuleKeyword);
            
            List<RulePackDef> extraRulePacks = new();
            
            extraRulePacks.Add(GADefOf.Found_Out_About_Gender_Identity);
            if (GenderUtility.DoesChaserSeeTranny(pawn, otherPawn))
                extraRulePacks.Add(GADefOf.Chaser_Found_Out);

            foreach (var grammarPack in extraRulePacks)
            {
                request.Clear();
                request.Includes.Add(grammarPack);
                request.Constants.AddRange(rules);
                request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", pawn, request.Constants));
                request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", otherPawn, request.Constants));
                
                text = $"{text}\n\n{
                    GrammarResolver.Resolve(
                        grammarPack.FirstRuleKeyword, 
                        request, "extraSentencePack", 
                        false, 
                        grammarPack.FirstUntranslatedRuleKeyword)
                }";
            }

            Find.LetterStack.ReceiveLetter("GA.PawnBelievesOtherPawnIsTransLabel".Translate(pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT")), text, LetterDefOf.NeutralEvent, new LookTargets(pawn, otherPawn)); 
        }
    }
    public static bool BelievesIsTrans(this Pawn pawn, Pawn otherPawn)
    {
        return pawn.GetBelievedToBeTrannies(false)?.Contains(otherPawn) ?? false;
    }

    public static void AttemptTransvestigate(this Pawn initiator, Pawn recipient, float normalChance=0.05f, float appearanceChance=1f)
    {
        if((!recipient.LooksCis() && TransDependencies.TransLibrary.FeaturesAppearances() && Rand.Chance(appearanceChance)) || Rand.Chance(normalChance))
            TransKnowledge.KnowledgeLearned(initiator, recipient, false, new ()
            {
                {"transvestigate", "True"}
            });
    }
}