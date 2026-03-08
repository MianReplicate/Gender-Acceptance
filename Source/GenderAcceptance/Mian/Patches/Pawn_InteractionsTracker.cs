using System.Linq;
using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Needs;
using GenderAcceptance.Mian.Utilities;
using HarmonyLib;
using RimWorld;
using Verse;
using GenderUtility = GenderAcceptance.Mian.Utilities.GenderUtility;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.Pawn_InteractionsTracker))]
public static class Pawn_InteractionsTracker
{
    [HarmonyPatch(nameof(RimWorld.Pawn_InteractionsTracker.TryInteractWith))]
    [HarmonyPostfix]
    public static void TryInteractWith(Pawn ___pawn, bool __result, Pawn recipient, InteractionDef intDef)
    {
        if (!__result || !recipient.RaceProps.Humanlike || !___pawn.RaceProps.Humanlike)
            return;
        if (___pawn.FindsExtraordinarilyAttractive(recipient))
            ((Need_Chaser)___pawn.needs?.TryGetNeed(GADefOf.Need_Chaser))?.GainNeedFromInteraction();

        var smallTalk = DefDatabase<InteractionDef>.GetNamedSilentFail("Rimpsyche_Smalltalk");
        var conversation = DefDatabase<InteractionDef>.GetNamedSilentFail("Rimpsyche_Conversation");
        var chitchat = InteractionDefOf.Chitchat;
        
        
        if (intDef == smallTalk || intDef == conversation || intDef == chitchat)
        {
            var multiplier = intDef == conversation ? 2f : 1f;

            var transgenders = ___pawn.GetTransgenderKnowledges(false)
                .Where(knowledge => knowledge.BelievesTheyAreTrans() && knowledge.Pawn != recipient).ToList();
            if (transgenders.Any())
            {  
                var randomPerson = transgenders.RandomElement();
                    var transphobic = ___pawn.GetTransphobicStatus(randomPerson.Pawn);
                    var revealChance = GASettings.Instance.baseRandomOuttingChance * multiplier;

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
                        Ideo ideo = GASettings.Instance.colonyIdeologyOverPawnIdeology ? recipient.Faction?.ideos?.PrimaryIdeo : ___pawn.Ideo;
                        revealChance *= GASettings.Instance.nonTransphobicPeopleNeverOut ? 0f : ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Adored ? 5f :
                            ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Exalted ? 10f : ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Despised ? 0.5f : ideo?.CultureOpinionOnTrans() == CultureViewOnTrans.Abhorrent ? 0.1f : 1f;
                    }

                    if (Rand.Chance(revealChance))
                    {
                        var initKnowledge = ___pawn.GetKnowledgeOnPawn(randomPerson.Pawn);
                        var recipientKnowledge = recipient.GetKnowledgeOnPawn(randomPerson.Pawn);

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
                                (transphobic.GenerallyTransphobic ? "GA.IntentionallyFoundOutThroughChat" : "GA.AccidentallyFoundOutThroughChat").Translate(___pawn.Named("TELLER"), recipient.Named("RECEIVER"),
                                    randomPerson.Pawn.Named("GOSSIPED")),
                                MessageTypeDefOf.NeutralEvent,
                                new LookTargets(___pawn, recipient, randomPerson.Pawn));
                            Messages.Message(message);
                        }

                        TransKnowledgeManager.OnKnowledgeLearned(recipient, randomPerson.Pawn);
                    }
            }
        }
    }
}