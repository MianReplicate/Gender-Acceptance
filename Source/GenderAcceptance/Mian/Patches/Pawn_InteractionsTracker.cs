using System.Linq;
using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Needs;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.Pawn_InteractionsTracker))]
public static class Pawn_InteractionsTracker
{
    [HarmonyPatch(nameof(RimWorld.Pawn_InteractionsTracker.TryInteractWith))]
    [HarmonyPostfix]
    public static void TryInteractWith(Pawn ___pawn, bool __result, Pawn recipient, InteractionDef intDef)
    {
        if (!__result || !recipient.RaceProps.Humanlike)
            return;
        if (GenderUtility.DoesChaserSeeTrans(___pawn, recipient))
            ((Chaser_Need) ___pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeedFromInteraction();
        if (intDef == InteractionDefOf.Chitchat)
        {
            ___pawn.AttemptTransvestigate(recipient, 0.001f, 0.005f);

            var transgenders = ___pawn.GetKnownTransgenders(false).ToList();
            if (transgenders.Any())
            {
                var randomCount = Rand.RangeInclusive(0, transgenders.Count);
                for (int i = 0; i < randomCount; i++)
                {
                    var transphobic = ___pawn.GetTransphobicStatus(transgenders[i].Key);
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

                    if (Rand.Chance(revealChance))
                    {
                        var initKnowledge = ___pawn.GetKnowledgeOnPawn(transgenders[i].Key);
                        var recipientKnowledge = recipient.GetKnowledgeOnPawn(transgenders[i].Key);

                        if (initKnowledge.cameOut)
                            recipientKnowledge.cameOut = true;
                        if (initKnowledge.transvestigate)
                            recipientKnowledge.transvestigate = true;
                        if (initKnowledge.sex)
                            recipientKnowledge.sex = true;
                        
                        TransKnowledgeManager.OnKnowledgeLearned(recipient, transgenders[i].Key);
                    }
                }   
            }
        }
    }
}