using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using Verse.Noise;
using UnityEngine;
using System.Reflection;
using Simple_Trans;

namespace GenderAcceptance
{
    public static class Helper
    {
        public static int CountGenderIndividuals(Map map, GenderIdentity gender)
        {
            int count = 0;
            List<Pawn> colonists = map.mapPawns.FreeColonists;

            foreach (Pawn pawn in colonists)
            {
                if (pawn.Dead || Helper.GetCurrentIdentity(pawn) != gender) continue;

                count++;
            }
            return count / 2;
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

    public class Thought_TransphobicVsTransgender : Thought_SituationalSocial
    {
        public override float OpinionOffset()
        {
            if (ThoughtUtility.ThoughtNullified(this.pawn, this.def))
            {
                return 0f;
            }

            if (Helper.GetCurrentIdentity(this.otherPawn) == GenderIdentity.Transgender)
            {
                return -25f;
            }

            return 0f;
        }
    }

    public class ThoughtWorker_TransphobicVsTransgender : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (p.story.traits.HasTrait(TraitDef.Named("Transphobic")) &&
                (Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Transgender))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
    
    public class Thought_CisphobicVsCisgender : Thought_SituationalSocial
    {
        public override float OpinionOffset()
        {
            if (ThoughtUtility.ThoughtNullified(this.pawn, this.def))
            {
                return 0f;
            }

            if (Helper.GetCurrentIdentity(this.otherPawn) == GenderIdentity.Cisgender)
            {
                return -25f;
            }

            return 0f;
        }
    }

    public class ThoughtWorker_CisphobicVsCisgender : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (p.story.traits.HasTrait(TraitDef.Named("Cisphobic"))
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
            int transgenderCount = Helper.CountGenderIndividuals(p.Map, GenderIdentity.Transgender);
            int stage = CalculateStageBasedOnCount(transgenderCount);

            if (stage >= 0)

                if (stage >= 0)
                {
                    return ThoughtState.ActiveAtStage(stage + 1);
                }
            return ThoughtState.ActiveAtStage(0);
        }

        private int CalculateStageBasedOnCount(int count)
        {
            return Math.Min(count - 1, 4);
        }
    }

    public class ThoughtWorker_PositiveViewOnTransgenderIndividuals : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            var adoredPrecept = DefDatabase<PreceptDef>.GetNamed("Transgender_Adored", false);
            if (adoredPrecept != null && pawn.Ideo.HasPrecept(adoredPrecept))
            {
                if (Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Transgender)
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return ThoughtState.Inactive;
        }
    }
    public class ThoughtWorker_NegativeViewOnTransgenderIndividuals : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            var despisedPrecept = DefDatabase<PreceptDef>.GetNamed("Transgender_Despised", false);
            if (despisedPrecept != null && pawn.Ideo.HasPrecept(despisedPrecept))
            {
                if (Helper.GetCurrentIdentity(otherPawn) == GenderIdentity.Transgender && !pawn.relations.DirectRelations.Any(rel => (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Spouse)))
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return ThoughtState.Inactive;
        }
    }

    public class MyMod : Mod
    {
        public MyMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("netdot.mian.genderacceptance");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
    public static class Patch_InteractionWorker_RomanceAttempt_SuccessChance
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            const float reductionFactor = 0.1f;
            var adoredPrecept = DefDatabase<PreceptDef>.GetNamed("Transgender_Adored", false);
            var despisedPrecept = DefDatabase<PreceptDef>.GetNamed("Transgender_Despised", false);
            
            bool adoresSameSex = initiator.Ideo?.HasPrecept(adoredPrecept) ?? false;
            bool despisesSameSex = initiator.Ideo?.HasPrecept(despisedPrecept) ?? false;
            bool isStraight = !initiator.story.traits.HasTrait(TraitDefOf.Gay) && initiator.gender == recipient.gender;
            bool isGay = initiator.story.traits.HasTrait(TraitDefOf.Gay) && initiator.gender != recipient.gender;

            if ((adoresSameSex && isStraight) || (despisesSameSex && isGay))
            {
                float compatibilityFactor = initiator.relations.CompatibilityWith(recipient);
                float opinionFactor = initiator.relations.OpinionOf(recipient) / 100.0f; 

                float dynamicChance = Mathf.Clamp01(compatibilityFactor * opinionFactor);

                dynamicChance *= reductionFactor;

                __result = Mathf.Max(__result, dynamicChance);
            }
        }
    }
}