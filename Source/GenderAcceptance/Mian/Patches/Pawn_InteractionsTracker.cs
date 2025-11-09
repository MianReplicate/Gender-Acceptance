using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Needs;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.Pawn_InteractionsTracker))]
public class Pawn_InteractionsTracker
{
    [HarmonyPatch(nameof(RimWorld.Pawn_InteractionsTracker.TryInteractWith))]
    [HarmonyPostfix]
    public static void TryInteractWith(Pawn ___pawn, bool __result, Pawn recipient, InteractionDef intDef)
    {
        if (!__result || !recipient.RaceProps.Humanlike)
            return;
        if (GenderUtility.DoesChaserSeeTrans(___pawn, recipient))
            ((Chaser_Need) ___pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeedFromInteraction();
        ___pawn.AttemptTransvestigate(recipient, 0.01f, 0.05f);

        if (intDef == InteractionDefOf.Chitchat)
        {
            var known = recipient.GetKnownTrannies(false);
            var trannies = ___pawn.GetKnownTrannies(false);
            trannies.RemoveWhere(pawn => known.Contains(pawn));

            if (trannies.Any())
            {
                var randomCount = Rand.RangeInclusive(0, trannies.Count);
                for (int i = 0; i < randomCount; i++)
                {
                    var transphobic = ___pawn.GetTransphobicStatus(trannies[i]);
                    var revealChance = 0.05f;

                    if (transphobic.GenerallyTransphobic)
                    {
                        revealChance *= 1.25f;
                        
                        if (transphobic.ChaserAttributeCounts)
                            revealChance *= 0.5f;
                        if (transphobic.HasTransphobicTrait)
                            revealChance *= 1.25f;
                        if (transphobic.TransphobicPreceptCounts)
                            revealChance *= 5f;
                    }
                    else
                    {
                        if (___pawn.CultureOpinionOnTrans() == CultureViewOnTrans.Adored)
                            revealChance *= 5f;
                    }
                    
                    if(Rand.Chance(revealChance))
                        TransKnowledge.KnowledgeLearned(recipient, trannies[i], false);
                }   
            }
        }
    }
}