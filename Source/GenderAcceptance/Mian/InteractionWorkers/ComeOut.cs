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
        if (recipient.BelievesIsTrans(initiator))
            return 0f;
        
        var spouseRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient) ? 1.75f : 1f;
        var loversRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Lover, recipient) ? 1.5f : 1f;
        var parentsRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Parent, recipient) ? 1.25f : 1f;
        var inTransphobicEnvironment = initiator.CultureOpinionOnTrans() == CultureViewOnTrans.Despised ? 0.05f : 
                initiator.CultureOpinionOnTrans() == CultureViewOnTrans.Adored ? 1.75f : 1f;

        var opinion = Mathf.Clamp(initiator.relations.OpinionOf(recipient), 0, 100) / 100;
        return opinion * spouseRelation * loversRelation * parentsRelation * inTransphobicEnvironment;
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
            {"isPositive", isPositive.ToString()}
        };

        var packs = new List<RulePackDef>()
        {
            GADefOf.Coming_Out
        };
        
        TransKnowledge.KnowledgeLearned(
            recipient, 
            initiator, 
            true, 
            isPositive ? LetterDefOf.PositiveEvent : LetterDefOf.NeutralEvent, 
            "GA.ComeOutLabel",
            packs,
            constants);
        
        initiator.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.CameOutPositive : GADefOf.CameOutNegative, recipient);
        
        recipient.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.FoundOutPawnIsTransMoodPositive : GADefOf.FoundOutPawnIsTransMoodNegative, recipient);
    }
}