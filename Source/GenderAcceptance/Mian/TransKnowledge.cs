using System.Collections.Generic;
using Verse;

namespace GenderAcceptance.Mian;

public static class TransKnowledge
{
    private static readonly Dictionary<Pawn, List<Pawn>> knownTransgenders = new Dictionary<Pawn, List<Pawn>>();

    public static void SetKnownTrannies(Pawn pawn, List<Pawn> pawns)
    {
        knownTransgenders.SetOrAdd(pawn, pawns);
    }
    
    public static List<Pawn> GetKnownTrannies(this Pawn pawn, bool cleanReferences)
    {
        if (knownTransgenders.TryGetValue(pawn, out var pawns)){
            if(cleanReferences)
                pawns.RemoveWhere(pawnRef => pawnRef.GetCurrentIdentity() != GenderIdentity.Transgender);
            return pawns;
        }
        return null;
    }

    public static void KnowledgeLearned(Pawn pawn, Pawn otherPawn)
    {
        if (otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender){
            var list = knownTransgenders.TryGetValue(pawn, new());
            list.AddUnique(otherPawn);
            SetKnownTrannies(pawn, list);
        }
    }

    public static bool KnowsIsTrans(this Pawn pawn, Pawn otherPawn)
    {
        return pawn.GetKnownTrannies(false)?.Contains(otherPawn) ?? false;
    }
}