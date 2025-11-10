using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BetterRomance;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches.Mod_Integration;

public static class WayBetterRomance
{
    public static void Patch(Harmony harmony)
    {
        // The following patches replace ldfld gender lines with a call to get perceived gender instead so that the transphobia trait will work properly
        // (transphobic people don't believe trans people are the gender they say they are)

        // Adds the chaser factor tooltip to the hookup menu
        harmony.Patch(typeof(HookupUtility).GetMethod(nameof(HookupUtility.HookupFactors)),
            transpiler: typeof(WayBetterRomance).GetMethod(nameof(AddChaserFactorToHookup)));
    }

    public static IEnumerable<CodeInstruction> AddChaserFactorToHookup(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var methodToLookFor = GetSexualityFactor();
        var newLabel = generator.DefineLabel();
        var oldLabel = new Label();
        var num = generator.DeclareLocal(typeof(float));
        var startFound = false;

        foreach (var code in instructions)
        {
            if (startFound && code.opcode == OpCodes.Brfalse_S)
            {
                oldLabel = (Label)code.operand;
                code.operand = newLabel;
            }

            yield return code;

            if (startFound && code.opcode == OpCodes.Pop)
            {
                //num = GenderUtility.ChaserFactor(romanceTarget, romancer);
                yield return new CodeInstruction(OpCodes.Ldarg_1).WithLabels(newLabel);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(GenderUtility), nameof(GenderUtility.ChaserFactor));
                yield return new CodeInstruction(OpCodes.Stloc, num);
                //if (num != 0f)
                yield return new CodeInstruction(OpCodes.Ldloc, num);
                yield return new CodeInstruction(OpCodes.Ldc_R4, 0f); // default value is 0f
                yield return new CodeInstruction(OpCodes.Beq_S, oldLabel);
                //stringBuilder.AppendLine(HookupFactorLine("GA.HookupChanceChaser".Translate(), num);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldstr, "GA.HookupChanceChaser");
                yield return CodeInstruction.Call(typeof(Translator), nameof(Translator.Translate), [typeof(string)]);
                yield return CodeInstruction.Call(typeof(TaggedString), "op_Implicit", [typeof(TaggedString)]);
                yield return new CodeInstruction(OpCodes.Ldloc, num);
                yield return CodeInstruction.Call(typeof(HookupUtility), "HookupFactorLine");
                yield return new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine), [typeof(string)]));
                yield return new CodeInstruction(OpCodes.Pop);
                startFound = false;
            }

            //We want to insert our stuff after this method
            if (code.Calls(methodToLookFor)) startFound = true;
        }
    }

    public static MethodInfo GetSexualityFactor()
    {
        return AccessTools.Method(typeof(RomanceUtilities), nameof(RomanceUtilities.SexualityFactor));
    }
}