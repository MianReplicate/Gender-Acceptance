using System.Collections.Generic;
using RimWorld;
using Verse;

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
    public static void KnowledgeLearned(Pawn pawn, Pawn otherPawn)
    {
        var list = believedToBeTransgender.TryGetValue(pawn, new());
        list.AddUnique(otherPawn);
        SetBelievedToBeTrannies(pawn, list);
    }
    public static bool BelievesIsTrans(this Pawn pawn, Pawn otherPawn)
    {
        return pawn.GetBelievedToBeTrannies(false)?.Contains(otherPawn) ?? false;
    }
}