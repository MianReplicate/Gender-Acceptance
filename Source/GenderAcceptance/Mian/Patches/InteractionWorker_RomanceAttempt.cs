using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using GenderAcceptance.Mian.Patches.Mod_Integration;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.InteractionWorker_RomanceAttempt))]
public static class InteractionWorker_RomanceAttempt
{
    //Adds a chaser factor to the romance success chance tooltip
    [HarmonyPatch(nameof(RimWorld.InteractionWorker_RomanceAttempt.RomanceFactors))]
    [HarmonyTranspiler]
    [HarmonyAfter("rimworld.divineDerivative.romance")]
    public static IEnumerable<CodeInstruction> AddChaserFactorTooltip(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var methodToLookFor = ModsConfig.IsActive("divinederivative.romance")
            ? WayBetterRomance.GetSexualityFactor()
            : AccessTools.Method(typeof(RimWorld.Pawn_RelationsTracker),
                nameof(RimWorld.Pawn_RelationsTracker.PrettinessFactor));
        var newLabel = generator.DefineLabel();
        var oldLabel = new Label();
        var num = generator.DeclareLocal(typeof(float));
        var startFound = false;

        foreach (var code in instructions)
        {
            if (startFound && code.Branches(out _))
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
                //stringBuilder.AppendLine(RomanceFactorLine("GA.HookupChanceChaser".Translate(), num);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldstr, "GA.HookupChanceChaser");
                yield return CodeInstruction.Call(typeof(Translator), nameof(Translator.Translate), [typeof(string)]);
                yield return CodeInstruction.Call(typeof(TaggedString), "op_Implicit", [typeof(TaggedString)]);
                yield return new CodeInstruction(OpCodes.Ldloc, num);
                yield return CodeInstruction.Call(typeof(RimWorld.InteractionWorker_RomanceAttempt),
                    "RomanceFactorLine");
                yield return new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.AppendLine), [typeof(string)]));
                yield return new CodeInstruction(OpCodes.Pop);

                startFound = false;
            }

            //We want to insert our stuff after this method
            if (code.Calls(methodToLookFor)) startFound = true;
        }
    }
}