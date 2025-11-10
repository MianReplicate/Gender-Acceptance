using RimWorld;
using Verse;
using Verse.AI;

namespace GenderAcceptance.Mian.JobGivers;

public class TransvestigateSpree : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        Helper.Debug("Trying to give transvestigate spree to " + pawn.Name);
        MentalStates.TransvestigateSpree transvestigateSpree = pawn.MentalState as MentalStates.TransvestigateSpree;
        if (transvestigateSpree == null || transvestigateSpree.target == null || !pawn.CanReach(transvestigateSpree.target, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
        {
            return null;
        }
        if (!SocialInteractionUtility.BestInteractableCell(pawn, transvestigateSpree.target).IsValid)
        {
            return null;
        }
        Helper.Debug("Making transvestigate job for " + pawn.Name + " with " + transvestigateSpree.target);

        return JobMaker.MakeJob(GADefOf.Transvestigate, transvestigateSpree.target);
    }
}