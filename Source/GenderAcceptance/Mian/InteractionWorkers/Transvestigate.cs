using System.Collections.Generic;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class InteractionWorker_Transvestigate : InteractionWorker
{
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
        
        if(!recipient.LooksCis() && TransDependencies.TransLibrary.FeaturesAppearances())
            TransKnowledge.KnowledgeLearned(initiator, recipient, false);
        else if (Rand.Chance(0.05f))
        {
            TransKnowledge.KnowledgeLearned(initiator, recipient, false);
        }
    }
}