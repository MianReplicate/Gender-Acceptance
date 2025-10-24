using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LoveyDoveySexWithEuterpe;
using Verse;

namespace GenderAcceptance.Patches.Mod_Integration;

public static class IntimacyLovin
{
    public static void Patch(Harmony harmony)
    {
        // The following patches replace ldfld gender lines with a call to get perceived gender instead so that the transphobia trait will work properly
        // (transphobic people don't believe trans people are the gender they say they are)
        
        harmony.Patch(typeof(CommonChecks).GetMethod(nameof(CommonChecks.AreMutuallyAttracted)),
            transpiler: typeof(Helper).GetMethod(nameof(ReplaceAttractGenderWithPerceivedGender)));
    }
    
    public static IEnumerable<CodeInstruction> ReplaceAttractGenderWithPerceivedGender(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++)
        {
            // otherPawn.gender => Helper.GetPerceivedGender(pawn, otherPawn)
            if (codes[i].LoadsField(AccessTools.Field(typeof(Pawn), nameof(Pawn.gender)))
                && codes[i + 1].Calls(AccessTools.Method(typeof(CommonChecks), nameof(CommonChecks.AttractedToGender))))
            {
                codes[i] = CodeInstruction.Call(typeof(Helper), nameof(Helper.GetPerceivedGender));
                codes.Insert(i - 1, new CodeInstruction(OpCodes.Dup));
                    
                i += 1; // skips past the AttractedToGender call
            }
        }
            
        return codes.AsEnumerable();
    }
}