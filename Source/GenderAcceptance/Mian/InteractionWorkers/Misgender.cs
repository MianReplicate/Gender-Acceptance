using System.Collections.Generic;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.InteractionWorkers;

public class InteractionWorker_Misgender : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (recipient.GetCurrentIdentity() == GenderIdentity.Transgender)
        {
            return 0.05f;
        }
        return 0.0f;
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

        if (!initiator.IsTransphobic())
        {
            var thought = ThoughtMaker.MakeThought(GADefOf.Accidental_Misgender, 0);
            initiator.needs.mood.thoughts.memories.TryGainMemory(thought, recipient);
        }
    }
}