using BetterRomance;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Patches;

[HarmonyPatch(typeof(RimWorld.LovePartnerRelationUtility))]
public class LovePartnerRelationUtility
{
     [HarmonyPatch(typeof(RimWorld.LovePartnerRelationUtility), nameof(RimWorld.LovePartnerRelationUtility.LovePartnerRelationGenerationChance))]
     [HarmonyPostfix]
     public static void Postfix(Pawn generated, Pawn other, ref float __result)
        {
            //Adjust with chaser rating
            float chaserFactor = Helper.ChaserSeesFetish(generated, other) ? 2f: 0;
            __result *= chaserFactor;
        }
}