using BetterRomance;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.LovePartnerRelationUtility))]
public class LovePartnerRelationUtility
{
     [HarmonyPatch(typeof(RimWorld.LovePartnerRelationUtility), nameof(RimWorld.LovePartnerRelationUtility.LovePartnerRelationGenerationChance))]
     [HarmonyPostfix]
     public static void Postfix(Pawn generated, Pawn other, ref float __result)
        {
            //Adjust with chaser rating
            if(GenderUtility.DoesChaserSeeTrans(generated, other))
                __result *= 2f;
        }
}