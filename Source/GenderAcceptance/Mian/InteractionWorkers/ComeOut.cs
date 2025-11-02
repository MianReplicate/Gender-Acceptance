using System;
using System.Collections.Generic;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Simple_Trans;
using UnityEngine;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class InteractionWorker_ComeOut : InteractionWorker
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
        letterLabel = "GA.ComeOutLabel".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"));
        lookTargets = (LookTargets) new LookTargets(initiator, recipient);
        
        TransKnowledge.KnowledgeLearned(recipient, initiator, true);

        var isPositive = !recipient.IsTrannyphobic();
        var isChaser = GenderUtility.DoesChaserSeeTranny(recipient, initiator);

        if (isChaser && isPositive == false)
            isPositive = Rand.Bool;
        
        extraSentencePacks.Add(isPositive ? GADefOf.Coming_Out_Positive_Pack : GADefOf.Coming_Out_Negative_Pack);
        
        if(!isPositive)
            extraSentencePacks.Add(GADefOf.Transphobe_Found_Out);
        
        if(isChaser) 
            extraSentencePacks.Add(GADefOf.Chaser_Found_Out);
        
        initiator.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.CameOutPositive : GADefOf.CameOutNegative, recipient);
        
        letterDef = isPositive ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent;
    }
}