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

namespace GenderAcceptance
{
    public class Thought_HomophobicVsGay : Thought_SituationalSocial
    {
        public override float OpinionOffset()
        {
            if (ThoughtUtility.ThoughtNullified(this.pawn, this.def))
            {
                return 0f;
            }

            if (this.otherPawn.story.traits.HasTrait(TraitDefOf.Gay) || this.otherPawn.story.traits.HasTrait(TraitDefOf.Bisexual))
            {
                return -25f;
            }

            return 0f;
        }
    }

    public class ThoughtWorker_HomophobicVsGay : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (p.story.traits.HasTrait(TraitDef.Named("Homophobic")) &&
                (otherPawn.story.traits.HasTrait(TraitDefOf.Gay) ||
                 otherPawn.story.traits.HasTrait(TraitDefOf.Bisexual) ||
                 IsInSameSexRelationship(otherPawn)))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }

        private bool IsInSameSexRelationship(Pawn pawn)
        {
            return pawn.relations.DirectRelations.Any(rel =>
                (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Spouse) &&
                pawn.gender == rel.otherPawn.gender);
        }
    }


    public class Thought_HeterophobicVsStraight : Thought_SituationalSocial
    {
        public override float OpinionOffset()
        {
            if (ThoughtUtility.ThoughtNullified(this.pawn, this.def))
            {
                return 0f;
            }

            if (!this.otherPawn.story.traits.HasTrait(TraitDefOf.Gay) && !this.otherPawn.story.traits.HasTrait(TraitDefOf.Bisexual) && !this.otherPawn.story.traits.HasTrait(TraitDefOf.Asexual))
            {
                return -25f;
            }

            return 0f;
        }
    }

    public class ThoughtWorker_HeterophobicVsStraight : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (p.story.traits.HasTrait(TraitDef.Named("Heterophobic"))
                && !otherPawn.story.traits.HasTrait(TraitDefOf.Gay)
                && !otherPawn.story.traits.HasTrait(TraitDefOf.Bisexual)
                && !otherPawn.story.traits.HasTrait(TraitDefOf.Asexual))
            {
                if (!IsInSameSexRelationship(otherPawn))
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return ThoughtState.Inactive;
        }

        private bool IsInSameSexRelationship(Pawn pawn)
        {
            return pawn.relations.DirectRelations.Any(rel =>
                (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Spouse) &&
                pawn.gender == rel.otherPawn.gender);
        }
    }

    public class ThoughtWorker_PreceptSameSexCouples : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            int sameSexCoupleCount = CountSameSexCouples(p.Map);
            int stage = CalculateStageBasedOnCount(sameSexCoupleCount);

            if (stage >= 0)

                if (stage >= 0)
                {
                    return ThoughtState.ActiveAtStage(stage + 1);
                }
            return ThoughtState.ActiveAtStage(0);
        }

        public static int CountSameSexCouples(Map map)
        {
            int count = 0;
            List<Pawn> colonists = map.mapPawns.FreeColonists;

            foreach (Pawn pawn in colonists)
            {
                if (pawn.Dead) continue;

                foreach (DirectPawnRelation relation in pawn.relations.DirectRelations)
                {
                    if ((relation.def == PawnRelationDefOf.Lover || relation.def == PawnRelationDefOf.Spouse) &&
                        pawn.gender == relation.otherPawn.gender &&
                        colonists.Contains(relation.otherPawn) &&
                        !relation.otherPawn.Dead)
                    {
                        count++;
                    }
                }
            }
            return count / 2;
        }

        private int CalculateStageBasedOnCount(int count)
        {
            return Math.Min(count - 1, 4);
        }
    }

    public class ThoughtWorker_PositiveViewOnSameSexCouples : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            var adoredPrecept = DefDatabase<PreceptDef>.GetNamed("SameSexCouples_Adored", false);
            if (adoredPrecept != null && pawn.Ideo.HasPrecept(adoredPrecept))
            {
                if (IsInSameSexRelationship(otherPawn))
                {
                    if (!IsInSameSexRelationshipWith(pawn, otherPawn))
                    {
                        return ThoughtState.ActiveAtStage(0);
                    }
                }
            }
            return ThoughtState.Inactive;
        }
        private bool IsInSameSexRelationship(Pawn pawn)
        {
            return pawn.relations.DirectRelations.Any(rel =>
                (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Spouse) &&
                pawn.gender == rel.otherPawn.gender);
        }

        private bool IsInSameSexRelationshipWith(Pawn pawn, Pawn otherPawn)
        {
            return pawn.relations.DirectRelations.Any(rel =>
                (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Spouse) &&
                pawn.gender == rel.otherPawn.gender && rel.otherPawn == otherPawn);
        }
    }
    public class ThoughtWorker_NegativeViewOnSameSexCouples : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            var despisedPrecept = DefDatabase<PreceptDef>.GetNamed("SameSexCouples_Despised", false);
            if (despisedPrecept != null && pawn.Ideo.HasPrecept(despisedPrecept))
            {
                if (IsInSameSexRelationship(otherPawn))
                {
                    if (!IsInSameSexRelationshipWith(pawn, otherPawn))
                    {
                        return ThoughtState.ActiveAtStage(0);
                    }
                }
            }
            return ThoughtState.Inactive;
        }
        private bool IsInSameSexRelationship(Pawn pawn)
        {
            return pawn.relations.DirectRelations.Any(rel =>
                (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Spouse) &&
                pawn.gender == rel.otherPawn.gender);
        }

        private bool IsInSameSexRelationshipWith(Pawn pawn, Pawn otherPawn)
        {
            return pawn.relations.DirectRelations.Any(rel =>
                (rel.def == PawnRelationDefOf.Lover || rel.def == PawnRelationDefOf.Spouse) &&
                pawn.gender == rel.otherPawn.gender && rel.otherPawn == otherPawn);
        }
    }

    public class MyMod : Mod
    {
        public MyMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.my.mod.uniqueid");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(RelationsUtility), "AttractedToGender")]
    public static class Patch_AttractedToGender
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Gender gender, ref bool __result)
        {
            var adoredPrecept = DefDatabase<PreceptDef>.GetNamed("SameSexCouples_Adored", false);
            var despisedPrecept = DefDatabase<PreceptDef>.GetNamed("SameSexCouples_Despised", false);

            bool adoresSameSex = pawn.Ideo?.HasPrecept(adoredPrecept) ?? false;
            bool despisesSameSex = pawn.Ideo?.HasPrecept(despisedPrecept) ?? false;

            bool isGay = pawn.story.traits.HasTrait(TraitDefOf.Gay);
            bool isBisexual = pawn.story.traits.HasTrait(TraitDefOf.Bisexual);
            bool isAsexual = pawn.story.traits.HasTrait(TraitDefOf.Asexual);

            if (isAsexual)
            {
                __result = false;
                return;
            }
            if (isBisexual)
            {
                __result = true;
                return;
            }
            if (adoresSameSex)
            {
                __result = true;
                return;
            }
            if (despisesSameSex && isGay)
            {
                __result = true;
                return;
            }
            __result = (isGay && pawn.gender == gender) || (!isGay && pawn.gender != gender);
        }
    }
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
    public static class Patch_InteractionWorker_RomanceAttempt_SuccessChance
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            const float reductionFactor = 0.1f;
            var adoredPrecept = DefDatabase<PreceptDef>.GetNamed("SameSexCouples_Adored", false);
            var despisedPrecept = DefDatabase<PreceptDef>.GetNamed("SameSexCouples_Despised", false);

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