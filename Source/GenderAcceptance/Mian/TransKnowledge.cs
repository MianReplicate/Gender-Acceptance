using System.Collections.Generic;
using RimWorld;
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
        list.AddUnique(otherPawn);
        SetBelievedToBeTrannies(pawn, list);

        if (!hardLearned)
        {
            var request = new GrammarRequest();
            var text = "GA.PawnBelievesOtherPawnIsTrans".Translate(pawn.Named("PAWN"), otherPawn.Named("SUSPECTEDPAWN"));
            request.Rules.AddRange(GrammarUtility.RulesForPawn("PAWN", pawn, request.Constants));
            request.Rules.AddRange(GrammarUtility.RulesForPawn("SUSPECTEDPAWN", otherPawn, request.Constants));

            RulePackDef rulePack = null;

            if (GenderUtility.DoesChaserSeeTranny(pawn, otherPawn))
                rulePack = GADefOf.Chaser_Found_Out;
            else if (pawn.IsTrannyphobic())
                rulePack = GADefOf.Transphobe_Found_Out;
            
            if(rulePack != null)
                text = $"{text} {GrammarResolver.Resolve("entry", request, "extraSentencePack",
                    false, rulePack.FirstUntranslatedRuleKeyword)}";

            Messages.Message((string) text, MessageTypeDefOf.NeutralEvent, false);   
        }
        else
        {
            var isPositive = GenderUtility.DoesChaserSeeTranny(pawn, otherPawn) || !pawn.IsTrannyphobic();
            pawn.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.FoundOutPawnIsTransMoodPositive : GADefOf.FoundOutPawnIsTransMoodNegative, otherPawn);
        }
    }
    public static bool BelievesIsTrans(this Pawn pawn, Pawn otherPawn)
    {
        return pawn.GetBelievedToBeTrannies(false)?.Contains(otherPawn) ?? false;
    }
}