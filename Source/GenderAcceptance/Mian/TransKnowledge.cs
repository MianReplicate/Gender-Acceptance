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
    private static Dictionary<string, string> defaultConstants =
    new(){
        {"didSex", "False"},
        {"mismatchedGenitalia", "False"},
        {"transvestigate", "False"},
        {"hasAppearance", "False"}
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
    public static void KnowledgeLearned(Pawn pawn, Pawn otherPawn, bool hardLearned, Dictionary<string, string> constants=null, List<Rule> rules=null)
    {
        var list = believedToBeTransgender.TryGetValue(pawn, new());
        if (list.Contains(otherPawn))
            return;
        if (constants != null && !constants.All(element => defaultConstants.ContainsKey(element.Key)))
        {
            Helper.Error("Invalid constants given!");
            return;
        }
        list.Add(otherPawn);
        SetBelievedToBeTrannies(pawn, list);

        if (!hardLearned)
        {
            var request = new GrammarRequest();

            if (constants == null)
                constants = defaultConstants;
            else
                constants.AddRange(defaultConstants.Where(constant => !constants.ContainsKey(constant.Key)).ToDictionary(pair => pair.Key, pair => pair.Value));

            request.Includes.Add(GADefOf.Suspicions_About_Trans);
            if(rules != null)
                request.Rules.AddRange(rules);
            request.Constants.AddRange(constants);
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
                if(rules != null)
                    request.Rules.AddRange(rules);
                request.Constants.AddRange(constants);
                request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", pawn, request.Constants));
                request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", otherPawn, request.Constants));
                
                text = $"{text}{
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

    public static void AttemptTransvestigate(this Pawn initiator, Pawn recipient, float normalChance=0.025f, float appearanceChance=0.25f)
    {
        if (initiator.BelievesIsTrans(recipient))
            return;
        if (!recipient.RaceProps.Humanlike)
            return;
        var appearanceRoll = Rand.Chance(appearanceChance - (recipient.CalculateRelativeAppearanceFromIdentity() / 10) * (initiator.story?.traits?.HasTrait(GADefOf.Chaser) ?? false ? 1.5f : 1));
        var normalRoll = Rand.Chance(normalChance);
        if (appearanceRoll || normalRoll)
        {
            var rules = new List<Rule>();
            if (appearanceRoll)
                rules.Add(new Rule_String("RECIPIENT_gendered", recipient.GetOppositeGender().GetGenderedAppearance().GetGenderNoun()));
            KnowledgeLearned(initiator, recipient, false, new()
            {
                { "transvestigate", "True" },
                { "hasAppearance", appearanceRoll.ToString()}
            }, rules);
        }
    }
}