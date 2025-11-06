using System.Collections.Generic;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class Transvestigate : InteractionWorker
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
        
        initiator.AttemptTransvestigate(recipient);
    }
}