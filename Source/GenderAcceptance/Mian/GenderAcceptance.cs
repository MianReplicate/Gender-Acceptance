using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GenderAcceptance.Mian.Patches.Mod_Integration;
using HarmonyLib;
using RimWorld;
using Verse;
using Simple_Trans;
using Pawn_RelationsTracker = RimWorld.Pawn_RelationsTracker;
using RelationsUtility = RimWorld.RelationsUtility;

// TODO: add credits (divineDerivative)!
//TODO: transphobic cultures wont like gender affirming parties
// TODO: add lang files
// TODO: ideally also support RJW but, we gotta write a PR first for SimpleTrans
// TODO: chasers get intimacy need by trans ppl in all sorts of social interactions
namespace GenderAcceptance.Mian
{
    public static class Helper
    {
        // Transphobic people see trans people as their AGAB, therefore a straight transphobic man could be attracted to a trans man based on this code
        public static IEnumerable<CodeInstruction> ReplaceAttractGenderWithPerceivedGender(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                // otherPawn.gender => Helper.AttractedToPerson(pawn, otherPawn)
                if (codes[i].LoadsField(AccessTools.Field(typeof(Pawn), nameof(Pawn.gender)))
                    && codes[i + 1].Calls(AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.AttractedToGender))))
                {
                    codes[i] = CodeInstruction.Call(typeof(Helper), nameof(AttractedToPerson));
                    codes.RemoveAt(i + 1);
                }
            }
            
            return codes.AsEnumerable();
        }

        public static float ChaserFactor(Pawn pawn, Pawn target)
        {
            if (ChaserSeesFetish(pawn, target))
                return 2f;
            return 0f;
        }
        
        // does a chaser find their fetish??
        public static bool ChaserSeesFetish(Pawn initiator, Pawn recipient)
        {
            if ((initiator.story?.traits?.HasTrait(GADefOf.Chaser) ?? false) && recipient.GetCurrentIdentity() == GenderIdentity.Transgender)
                return AttractedToPerson(initiator, recipient);
            return false;
        }

        public static bool IsTransphobic(this Pawn pawn, bool includePrecept=true)
        {
            return (pawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false) || 
                   (includePrecept && pawn.GetCurrentIdentity() == GenderIdentity.Cisgender 
                    && (pawn.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false));
        }

        public static Gender GetPerceivedGender(Pawn initiator, Pawn recipient)
        {
            if (recipient.GetCurrentIdentity() != GenderIdentity.Transgender)
                return recipient.gender;

            if (recipient.gender != Gender.Male && recipient.gender != Gender.Female)
                return recipient.gender; // idk what to do otherwise if im being honest. e.g. nonbinary people
            
            return initiator.IsTransphobic() ? recipient.gender == Gender.Male ? Gender.Female : Gender.Male : recipient.gender;
        }

        public static bool AttractedToPerson(Pawn initiator, Pawn recipient)
        {
            return RelationsUtility.AttractedToGender(initiator, GetPerceivedGender(initiator, recipient));
        }

        public static bool AttractedToEachOther(Pawn pawn1, Pawn pawn2)
        {
            return AttractedToPerson(pawn1, pawn2) && AttractedToPerson(pawn2, pawn1);
        }
        
        public static int CountGenderIndividuals(Map map, GenderIdentity gender)
        {
            int count = 0;
            List<Pawn> colonists = map.mapPawns.FreeColonists;

            foreach (Pawn pawn in colonists)
            {
                if (pawn.Dead || pawn.GetCurrentIdentity() != gender) continue;

                count++;
            }
            return count;
        }
        public static GenderIdentity GetCurrentIdentity(this Pawn pawn)
        {
            if (pawn.health?.hediffSet?.HasHediff(SimpleTransPregnancyUtility.transDef) ?? false)
            {
                return GenderIdentity.Transgender;
            }
            if (pawn.health?.hediffSet?.HasHediff(SimpleTransPregnancyUtility.cisDef) ?? false)
            {
                return GenderIdentity.Cisgender;
            }
            return GenderIdentity.Cisgender;
        }   
    }

    public class InteractionWorker_PurposeMisgender : InteractionWorker
    {
        public virtual float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.IsTransphobic() && recipient.GetCurrentIdentity() == GenderIdentity.Transgender)
            {
                return 1 * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
            }
            return 0.0f;
        }
    }
    
    public class InteractionWorker_Misgender : InteractionWorker
    {
        public virtual float RandomSelectionWeight(Pawn initiator, Pawn recipient)
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
                initiator.needs.mood.thoughts.memories.TryGainMemory(thought);
            }
        }
    }
    
    public class ThoughtWorker_Transphobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if ((p.IsTransphobic(false) &&
                 otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender) || (
                    p.GetCurrentIdentity() == GenderIdentity.Transgender && (otherPawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false)))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
    
    public class ThoughtWorker_InternalTransphobia : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.GetCurrentIdentity() == GenderIdentity.Transgender && (p.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false))
            {
                var count = Helper.CountGenderIndividuals(p.Map, GenderIdentity.Transgender);
                return count <= 2 ? ThoughtState.ActiveAtStage(0) : ThoughtState.ActiveAtStage(1);
            }
            return ThoughtState.Inactive;
        }
    }

    public class ThoughtWorker_Similarity : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (p.GetCurrentIdentity() == GenderIdentity.Transgender && otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
    
    public class ThoughtWorker_Cisphobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (((p.story?.traits?.HasTrait(GADefOf.Cisphobic) ?? false)
                && otherPawn.GetCurrentIdentity() == GenderIdentity.Cisgender) || (
                p.GetCurrentIdentity() == GenderIdentity.Cisgender && (otherPawn.story?.traits?.HasTrait(GADefOf.Cisphobic) ?? false)))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }

    public class ThoughtWorker_PreceptTransgender : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            int transgenderCount = Helper.CountGenderIndividuals(p.Map, GenderIdentity.Transgender);
            int stage = Math.Min(transgenderCount - 1, 4);

            if (stage >= 0)
                return ThoughtState.ActiveAtStage(stage + 1);
            return ThoughtState.ActiveAtStage(0);
        }
    }

    public class ThoughtWorker_PositiveViewOnTransgender : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            if (pawn.Ideo?.HasPrecept(GADefOf.Transgender_Adored) ?? false)
            {
                if (otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender)
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return ThoughtState.Inactive;
        }
    }
    public class ThoughtWorker_NegativeViewOnTransgender : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            if (pawn.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false)
            {
                if (otherPawn.GetCurrentIdentity() == GenderIdentity.Transgender)
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return ThoughtState.Inactive;
        }
    }
    
    public class ThoughtWorker_Chaser_Need : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var chaserNeed = (Chaser_Need) p.needs.TryGetNeed(GADefOf.Chaser_Need);
            if (chaserNeed == null)
                return ThoughtState.Inactive;
            switch (chaserNeed.CurCategory)
            {
                case ChaserCategory.JustHadIntimacy:
                    return ThoughtState.ActiveAtStage(0);
                case ChaserCategory.Neutral:
                    return ThoughtState.Inactive;
                case ChaserCategory.LongWhile:
                    return ThoughtState.ActiveAtStage(1);
                case ChaserCategory.ExtremelyLongWhile:
                    return ThoughtState.ActiveAtStage(2);
                case ChaserCategory.Aching:
                    return ThoughtState.ActiveAtStage(3);
                default:
                    throw new NotImplementedException();
            }
        }
    }


    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony.DEBUG = true;
            
            var harmony = new Harmony("rimworld.mian.genderacceptance");
            harmony.PatchAll();

            if (ModsConfig.IsActive("divinederivative.romance"))
            {
                WayBetterRomance.Patch(harmony);
            }
            
            if (ModsConfig.IsActive("lovelydovey.sex.witheuterpe"))
            {
                IntimacyLovin.Patch(harmony);
            }
        }
    }
}