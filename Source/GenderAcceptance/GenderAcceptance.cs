using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GenderAcceptance.Patches;
using GenderAcceptance.Patches.Mod_Integration;
using HarmonyLib;
using RimWorld;
using Verse;
using Simple_Trans;
using Pawn_RelationsTracker = RimWorld.Pawn_RelationsTracker;
using RelationsUtility = RimWorld.RelationsUtility;

//TODO: transphobic cultures wont like gender affirming parties
// TODO: swap opinion increase for chasers to increasing romance/sex factors
// TODO: add lang files
namespace GenderAcceptance
{
    public static class Helper
    {
        public static IEnumerable<CodeInstruction> ReplaceGenderAttractionCalls(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].LoadsField(AccessTools.Field(typeof(Pawn), nameof(Pawn.gender)))
                    && codes[i + 1].Calls(AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.AttractedToGender))))
                {
                
                    codes.RemoveRange(i, 2);
                    codes.InsertRange(i, [
                        CodeInstruction.Call(typeof(Helper), nameof(AttractedToPerson)),
                    ]);
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
        
        // used for chasers
        public static bool IsGay(this Pawn pawn)
        {
            var traits = pawn.story.traits;
            var homoAce = DefDatabase<TraitDef>.GetNamedSilentFail("HomoAce");
            return traits.HasTrait(TraitDefOf.Gay) || (homoAce != null && traits.HasTrait(homoAce));
        }
        public static bool IsStraight(this Pawn pawn)
        {
            var traits = pawn.story.traits;
            var heteroAce = DefDatabase<TraitDef>.GetNamedSilentFail("HeteroAce");
            var straight = DefDatabase<TraitDef>.GetNamedSilentFail("Straight");
            return ((straight != null && traits.HasTrait(straight)) || (heteroAce != null && traits.HasTrait(heteroAce)) 
                                                                    || (!pawn.IsGay() && !traits.HasTrait(TraitDefOf.Bisexual) && !traits.HasTrait(TraitDefOf.Asexual)));
        }
        // does a chaser find their fetish??
        public static bool ChaserSeesFetish(Pawn initiator, Pawn recipient)
        {
            // initiator cannot have asexual traits
            if (initiator.story.traits.HasTrait(GADefOf.Chaser) && GetCurrentIdentity(recipient) == GenderIdentity.Transgender)
            {
                var genderPerceivedByChaser = GetPerceivedGender(initiator, recipient);
                if (initiator.IsGay() && genderPerceivedByChaser == initiator.gender)
                    return true;
                if (initiator.IsStraight() && genderPerceivedByChaser != initiator.gender)
                    return true;
            }
            return false;
        }

        public static bool IsTransphobic(Pawn pawn)
        {
            return pawn.story.traits.HasTrait(GADefOf.Transphobic) || 
                   (GetCurrentIdentity(pawn) == GenderIdentity.Cisgender 
                    && (pawn.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false));
        }

        public static Gender GetPerceivedGender(Pawn initiator, Pawn recipient)
        {
            if (GetCurrentIdentity(recipient) != GenderIdentity.Transgender)
                return recipient.gender;

            if (recipient.gender != Gender.Male && recipient.gender != Gender.Female)
                return recipient.gender; // idk what to do otherwise if im being honest. e.g. nonbinary people
            
            return IsTransphobic(initiator) ? recipient.gender == Gender.Male ? Gender.Female : Gender.Male : recipient.gender;
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
                if (pawn.Dead || Helper.GetCurrentIdentity(pawn) != gender) continue;

                count++;
            }
            return count;
        }
        public static GenderIdentity GetCurrentIdentity(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(SimpleTransPregnancyUtility.transDef))
            {
                return GenderIdentity.Transgender;
            }
            if (pawn.health.hediffSet.HasHediff(SimpleTransPregnancyUtility.cisDef))
            {
                return GenderIdentity.Cisgender;
            }
            return GenderIdentity.Cisgender;
        }   
    }
    
    public class ThoughtWorker_Transphobia : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if ((p.story.traits.HasTrait(GADefOf.Transphobic) &&
                Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Transgender) || (
                    Helper.GetCurrentIdentity(p) == GenderIdentity.Transgender && otherPawn.story.traits.HasTrait(GADefOf.Transphobic)))
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
            if (Helper.GetCurrentIdentity(p) == GenderIdentity.Transgender && (p.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false))
            {
                var count = Helper.CountGenderIndividuals(p.Map, GenderIdentity.Transgender);
                return count <= 2 ? ThoughtState.ActiveAtStage(0) : ThoughtState.ActiveAtStage(1);
            }
            return ThoughtState.Inactive;
        }
    }
    
    public class ThoughtWorker_Chaser : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (Helper.ChaserSeesFetish(p, otherPawn))
                return ThoughtState.ActiveAtStage(0);
            return ThoughtState.Inactive;
        }
    }

    public class ThoughtWorker_Similarity : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (Helper.GetCurrentIdentity(p) == GenderIdentity.Transgender && Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Transgender)
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
            if (p.story.traits.HasTrait(GADefOf.Cisphobic)
                && Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Cisgender)
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
            if(Helper.GetCurrentIdentity(p) == GenderIdentity.Transgender) return ThoughtState.ActiveAtStage(0);
            
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
                if (Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Transgender)
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
                if (Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Transgender)
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return ThoughtState.Inactive;
        }
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            var harmony = new Harmony("rimworld.mian.genderacceptance");
            harmony.PatchAll();

            if (ModsConfig.IsActive("divinederivative.romance"))
            {
                WayBetterRomance.Patch(harmony);
            }
        }
    }
}