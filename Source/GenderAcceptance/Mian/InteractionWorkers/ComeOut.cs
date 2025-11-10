using System;
using System.Collections.Generic;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Simple_Trans;
using UnityEngine;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class ComeOut : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (initiator.GetCurrentIdentity() == GenderIdentity.Cisgender)
            return 0f;
        if (recipient.GetKnowledgeOnPawn(initiator).cameOut)
            return 0f;
        
        var spouseRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient) ? 1.75f : 1f;
        var loversRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Lover, recipient) ? 1.5f : 1f;
        var parentsRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Parent, recipient) ? 1.25f : 1f;
        var environment = 1f;

        var cultureOpinion = initiator.CultureOpinionOnTrans();
        if(cultureOpinion == CultureViewOnTrans.Despised)
            environment = 0.05f;
        else if(cultureOpinion == CultureViewOnTrans.Adored)
            environment = 1.75f;
        else if (cultureOpinion == CultureViewOnTrans.Exalted)
            environment = 2.5f;
        else if (cultureOpinion == CultureViewOnTrans.Abhorrent)
            environment = 0.01f;
        
        var opinion = Mathf.Clamp(initiator.relations.OpinionOf(recipient), 0, 100) / 100;
        return opinion * spouseRelation * loversRelation * parentsRelation * environment;
    }
        
    public override void Interacted(
        Pawn initiator,
        Pawn recipient,
        List<RulePackDef> extraSentencePacks,
        out string letterText,
        out string letterLabel,
        out LetterDef letterDef,
        out LookTargets lookTargets)
    {
        letterText = (string) null;
        letterLabel = null;
        letterDef = null;
        lookTargets = (LookTargets) null;
        
        var transphobia = recipient.GetTransphobicStatus(initiator);
        var isNegative = transphobia.GenerallyTransphobic;
        
        if (transphobia.ChaserAttributeCounts && isNegative)
            isNegative = Rand.Chance(0.1f * NegativeInteractionUtility.NegativeInteractionChanceFactor(recipient, initiator));

        var isPositive = !isNegative;
        var constants = new Dictionary<string, string>()
        {
            {"isPositive", isPositive.ToString()},
            {"cameOut", "True"}
        };

        // var packs = new List<RulePackDef>()
        // {
        //     GADefOf.Coming_Out
        // };

        recipient.GetKnowledgeOnPawn(initiator).cameOut = true;
        TransKnowledgeManager.OnKnowledgeLearned(
            recipient, 
            initiator,
            isPositive ? LetterDefOf.PositiveEvent : LetterDefOf.NeutralEvent, 
            "GA.ComeOutLabel",
            // packs,
            constants: constants);
        
        initiator.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.CameOutPositive : GADefOf.CameOutNegative, recipient);
        
        recipient.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.FoundOutPawnIsTransMoodPositive : GADefOf.FoundOutPawnIsTransMoodNegative, recipient);
    }
}