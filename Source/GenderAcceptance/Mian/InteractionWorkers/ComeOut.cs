using System;
using System.Collections.Generic;
using RimWorld;
using Simple_Trans;
using UnityEngine;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class InteractionWorker_ComeOut : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (recipient.BelievesIsTrans(initiator))
            return 0f;
        
        var loversRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Lover, recipient) ? 1.5f : 1f;
        var parentsRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Parent, recipient) ? 1.25f : 1f;
        var siblingsRelation = initiator.relations.DirectRelationExists(PawnRelationDefOf.Sibling, recipient) ? 1.25f : 1f;
        var inTransphobicEnvironment = initiator.IsInCultureWithTrannyphobia() ? 0.05f : 1f;

        var opinion = Mathf.Clamp(initiator.relations.OpinionOf(recipient), 0, 100) / 100;
        return opinion * loversRelation * parentsRelation * siblingsRelation * inTransphobicEnvironment;
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
        letterLabel = (string) null;
        letterDef = (LetterDef) null;
        lookTargets = (LookTargets) null;
        
        TransKnowledge.KnowledgeLearned(recipient, initiator, true);

        var isPositive = !recipient.IsTrannyphobic();
        if (GenderUtility.DoesChaserSeeTranny(recipient, initiator))
        {
            isPositive = true;
            extraSentencePacks.Add(GADefOf.Chaser_Found_Out);
            extraSentencePacks.Add(GADefOf.Coming_Out_Positive_Pack);
        }
        else
        {
            extraSentencePacks.Add(isPositive ? GADefOf.Coming_Out_Positive_Pack : GADefOf.Coming_Out_Negative_Pack);   
        }
        
        initiator.needs.mood.thoughts.memories.TryGainMemory(isPositive ? GADefOf.CameOutPositive : GADefOf.CameOutNegative, recipient);
    }
}