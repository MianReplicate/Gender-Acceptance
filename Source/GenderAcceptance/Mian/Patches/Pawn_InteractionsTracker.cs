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
            ((Chaser_Need)___pawn.needs?.TryGetNeed(GADefOf.Chaser_Need))?.GainNeedFromInteraction();

        var smallTalk = DefDatabase<InteractionDef>.GetNamedSilentFail("Rimpsyche_Smalltalk");
        var conversation = DefDatabase<InteractionDef>.GetNamedSilentFail("Rimpsyche_Conversation");
        var chitchat = InteractionDefOf.Chitchat;
        
        
        if (intDef == smallTalk || intDef == conversation || intDef == chitchat)
        {
            var multiplier = intDef == conversation ? 2f : 1f;
            
            ___pawn.AttemptTransvestigate(recipient, 0.001f * multiplier, 0.005f * multiplier);

            var transgenders = ___pawn.GetTransgenderKnowledges(false)
                .Where(knowledge => knowledge.BelievesTheyAreTrans() && knowledge.Pawn != recipient).ToList();
            if (transgenders.Any())
            {
                var randomCount = Rand.RangeInclusive(0, transgenders.Count);
                for (var i = 0; i < randomCount; i++)
                {
                    var transphobic = ___pawn.GetTransphobicStatus(transgenders[i].Pawn);
                    var revealChance = 0.05f * multiplier;

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
                        revealChance *= ___pawn.CultureOpinionOnTrans() == CultureViewOnTrans.Adored ? 5f :
                            ___pawn.CultureOpinionOnTrans() == CultureViewOnTrans.Exalted ? 10f : 1f;
                    }

                    if (Rand.Chance(revealChance))
                    {
                        var initKnowledge = ___pawn.GetKnowledgeOnPawn(transgenders[i].Pawn);
                        var recipientKnowledge = recipient.GetKnowledgeOnPawn(transgenders[i].Pawn);

                        if (initKnowledge.cameOut)
                            recipientKnowledge.cameOut = true;
                        if (initKnowledge.transvestigate)
                            recipientKnowledge.transvestigate = true;
                        if (initKnowledge.sex)
                            recipientKnowledge.sex = true;

                        if (!recipientKnowledge.playedNotification)
                        {
                            recipientKnowledge.playedNotification = true;
                            var message = new Message(
                                "GA.FoundOutThroughChat".Translate(___pawn.Named("TELLER"), recipient.Named("RECEIVER"),
                                    transgenders[i].Pawn.Named("GOSSIPED")),
                                MessageTypeDefOf.NeutralEvent,
                                new LookTargets(___pawn, recipient, transgenders[i].Pawn));
                            Messages.Message(message);
                        }

                        TransKnowledgeManager.OnKnowledgeLearned(recipient, transgenders[i].Pawn);
                    }
                }
            }
        }
    }
}