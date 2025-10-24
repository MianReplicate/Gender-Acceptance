using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BetterRomance;
using BetterRomance.HarmonyPatches;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderAcceptance.Patches.Mod_Integration;

public static class WayBetterRomance
{
    public static void Patch(Harmony harmony)
    {
        // The following patches replace ldfld gender lines with a call to get perceived gender instead so that the transphobia trait will work properly
        // (transphobic people don't believe trans people are the gender they say they are)
        
        harmony.Patch(typeof(SexualityUtility).GetMethod(nameof(SexualityUtility.CouldWeBeLovers)),
            transpiler: typeof(WayBetterRomance).GetMethod(nameof(Helper.ReplaceAttractGenderWithPerceivedGender)));

        harmony.Patch(typeof(SexualityUtility).GetMethod(nameof(SexualityUtility.WouldConsiderMarriage)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceAttractGenderWithPerceivedGender)));
        
        harmony.Patch(typeof(SexualityUtility).GetMethod(nameof(SexualityUtility.CouldWeBeMarried)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceAttractGenderWithPerceivedGender)));

        harmony.Patch(typeof(HookupUtility).GetMethod(nameof(HookupUtility.HookupOption)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceAttractGenderWithPerceivedGender)));
        
        harmony.Patch(typeof(HookupUtility).GetMethod(nameof(HookupUtility.HookupEligiblePair)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceAttractGenderWithPerceivedGender)));

        harmony.Patch(
            typeof(InteractionWorker_Breakup_RandomSelectionWeight).GetMethod(
                nameof(InteractionWorker_Breakup_RandomSelectionWeight.Postfix)),
            transpiler: typeof(Helper).GetMethod(nameof(Helper.ReplaceAttractGenderWithPerceivedGender)));
            
        // Adds the chaser factor tooltip to the hookup menu
        harmony.Patch(typeof(HookupUtility).GetMethod(nameof(HookupUtility.HookupFactors)),
            transpiler: typeof(Helper).GetMethod(nameof(AddChaserFactorToHookup)));
    }

    public static IEnumerable<CodeInstruction> AddChaserFactorToHookup(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        MethodInfo methodToLookFor = GetSexualityFactor();
            Label newLabel = generator.DefineLabel();
            Label oldLabel = new Label();
            LocalBuilder num = generator.DeclareLocal(typeof(float));
            bool startFound = false;

            foreach (CodeInstruction code in instructions)
            {
                if (startFound && code.Branches(out _))
                {
                    oldLabel = (Label)code.operand;
                    code.operand = newLabel;
                }

                yield return code;

                if (startFound && code.opcode == OpCodes.Pop)
                {
                    //num = Helper.ChaserFactor(romanceTarget, romancer);
                    yield return new CodeInstruction(OpCodes.Ldarg_1).WithLabels(newLabel);
                    yield return new(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(Helper), nameof(Helper.ChaserFactor));
                    yield return new(OpCodes.Stloc, num);
                    //if (num != 0f)
                    yield return new(OpCodes.Ldloc, num);
                    yield return new(OpCodes.Ldc_R4, 0f); // default value is 0f
                    yield return new(OpCodes.Beq_S, oldLabel);
                    //stringBuilder.AppendLine(HookupFactorLine("GA.HookupChanceChaser".Translate(), num);
                    yield return new(OpCodes.Ldloc_0);
                    yield return new(OpCodes.Ldstr, "GA.HookupChanceChaser");
                    // yield return CodeInstruction.Call(typeof(Translator), nameof(Translator.Translate), [typeof(string)]);
                    yield return CodeInstruction.Call(typeof(TaggedString), "op_Implicit", [typeof(TaggedString)]);
                    yield return new(OpCodes.Ldloc, num);
                    yield return CodeInstruction.Call(typeof(HookupUtility), "HookupFactorLine");
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine), [typeof(string)]));
                    yield return new(OpCodes.Pop);
                    startFound = false;
                }
                //We want to insert our stuff after this method
                if (code.Calls(methodToLookFor))
                {
                    startFound = true;
                }
            }
    }

    public static MethodInfo GetSexualityFactor()
    {
        return AccessTools.Method(typeof(RomanceUtilities), nameof(RomanceUtilities.SexualityFactor));
    }
}