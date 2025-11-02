using System.Collections.Generic;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace GenderAcceptance.Mian;

public static class TransKnowledge
{
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
    public static void KnowledgeLearned(Pawn pawn, Pawn otherPawn, bool hardLearned)
    {
        var list = believedToBeTransgender.TryGetValue(pawn, new());
        if (list.Contains(otherPawn))
            return;
        list.Add(otherPawn);
        SetBelievedToBeTrannies(pawn, list);

        if (!hardLearned)
        {
            var request = new GrammarRequest();
            var text = "GA.PawnBelievesOtherPawnIsTrans".Translate(pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT"));

            List<RulePackDef> rulePacks = new();
            
            rulePacks.Add(GADefOf.Suspicions_About_Trans);
            if (pawn.IsTrannyphobic())
                rulePacks.Add(GADefOf.Transphobe_Found_Out);
            if (GenderUtility.DoesChaserSeeTranny(pawn, otherPawn))
                rulePacks.Add(GADefOf.Chaser_Found_Out);

            foreach (var grammarPack in rulePacks)
            {
                request.Clear();
                request.Includes.Add(grammarPack);
                request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", pawn, request.Constants));
                request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", otherPawn, request.Constants));
                
                text = text + " " + 
                       GrammarResolver.Resolve(
                           grammarPack.FirstRuleKeyword, 
                           request, "extraSentencePack", 
                           false, 
                           grammarPack.FirstUntranslatedRuleKeyword);
            }

            Find.LetterStack.ReceiveLetter("GA.PawnBelievesOtherPawnIsTransLabel".Translate(pawn.Named("INITIATOR")), text, LetterDefOf.NeutralEvent, new LookTargets(pawn, otherPawn));
        }
        else
        {
            var isPositive = GenderUtility.DoesChaserSeeTranny(pawn, otherPawn) || !pawn.IsTrannyphobic();
            pawn.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.FoundOutPawnIsTransMoodPositive : GADefOf.FoundOutPawnIsTransMoodNegative, otherPawn);

            if (!isPositive)
                Helper.CheckSocialFightStart(Mathf.Abs(Mathf.Clamp((pawn.relations.OpinionOf(otherPawn) - 100), -100, 0) / 100f), pawn, otherPawn);
        }
    }
    public static bool BelievesIsTrans(this Pawn pawn, Pawn otherPawn)
    {
        return pawn.GetBelievedToBeTrannies(false)?.Contains(otherPawn) ?? false;
    }

    public static void AttemptTransvestigate(this Pawn initiator, Pawn recipient, float normalChance=0.05f, float appearanceChance=1f)
    {
        if(!recipient.LooksCis() && TransDependencies.TransLibrary.FeaturesAppearances() && Rand.Chance(appearanceChance))
            TransKnowledge.KnowledgeLearned(initiator, recipient, false);
        else if (Rand.Chance(normalChance))
        {
            TransKnowledge.KnowledgeLearned(initiator, recipient, false);
        }
    }
}