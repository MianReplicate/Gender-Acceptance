using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace GenderAcceptance.Mian.Patches.Mod_Integration;

public static class IntimacyLovin
{
    public static void Patch(Harmony harmony)
    {
        // The following patches replace ldfld gender lines with a call to get perceived gender instead so that the transphobia trait will work properly
        // (transphobic people don't believe trans people are the gender they say they are)

        harmony.Patch(
            typeof(InteractionWorker_Seduce).GetMethod(nameof(InteractionWorker_Seduce.RandomSelectionWeight)),
            postfix: typeof(IntimacyLovin).GetMethod(nameof(AddChaserRandomSelectionFactor)));
        harmony.Patch(typeof(SexUtilities).GetMethod(nameof(SexUtilities.SexDisposition)),
            postfix: typeof(IntimacyLovin).GetMethod(nameof(AddChaserDispositionFactor)));
    }

    public static void AddChaserRandomSelectionFactor(Pawn initiator, Pawn recipient, ref float __result)
    {
        //Adjust with chaser rating
        if (GenderUtility.DoesChaserSeeTrans(initiator, recipient))
            __result *= 2f;
    }

    public static void AddChaserDispositionFactor(Pawn initiator, Pawn recipient, ref float __result)
    {
        //Adjust with chaser rating
        if (GenderUtility.DoesChaserSeeTrans(initiator, recipient))
            __result *= 2f;
    }

    // public static IEnumerable<CodeInstruction> ReplaceAttractGenderWithPerceivedGender(IEnumerable<CodeInstruction> instructions)
    // {
    //     var codes = new List<CodeInstruction>(instructions);
    //     for (var i = 0; i < codes.Count; i++)
    //     {
    //         // otherPawn.gender => Helper.GetPerceivedGender(pawn, otherPawn)
    //         if (codes[i].LoadsField(AccessTools.Field(typeof(Pawn), nameof(Pawn.gender)))
    //             && codes[i + 1].Calls(AccessTools.Method(typeof(CommonChecks), nameof(CommonChecks.AttractedToGender))))
    //         {
    //             codes[i] = CodeInstruction.Call(typeof(Helper), nameof(Helper.GetPerceivedGender));
    //             codes.Insert(i - 1, new CodeInstruction(OpCodes.Dup));
    //                 
    //             i += 1; // skips past the AttractedToGender call
    //         }
    //     }
    //         
    //     return codes.AsEnumerable();
    // }
}