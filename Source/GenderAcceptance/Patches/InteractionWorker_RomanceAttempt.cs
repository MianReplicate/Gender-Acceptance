using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using GenderAcceptance.Patches.Mod_Integration;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Patches;

[HarmonyPatch(typeof(RimWorld.InteractionWorker_RomanceAttempt))]
public class InteractionWorker_RomanceAttempt
{
    //Adds a chaser factor to the romance success chance tooltip
    [HarmonyPatch(nameof(RimWorld.InteractionWorker_RomanceAttempt.RomanceFactors))]
    [HarmonyTranspiler]
    [HarmonyAfter("rimworld.divineDerivative.romance")]
        public static IEnumerable<CodeInstruction> AddChaserFactorTooltip(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo methodToLookFor = ModsConfig.IsActive("divinederivative.romance") 
                ? WayBetterRomance.GetSexualityFactor()
                : AccessTools.Method(typeof(RimWorld.Pawn_RelationsTracker), nameof(RimWorld.Pawn_RelationsTracker.PrettinessFactor));
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
                    //stringBuilder.AppendLine(RomanceFactorLine("WBR.HookupChanceSexuality".Translate(), num);
                    yield return new(OpCodes.Ldloc_0);
                    yield return new(OpCodes.Ldstr, "GA.HookupChanceChaser");
                    // yield return CodeInstruction.Call(typeof(Translator), nameof(Translator.Translate), [typeof(string)]);
                    // yield return CodeInstruction.Call(typeof(TaggedString), "op_Implicit", [typeof(TaggedString)]);
                    yield return new(OpCodes.Ldloc, num);
                    yield return CodeInstruction.Call(typeof(RimWorld.InteractionWorker_RomanceAttempt), "RomanceFactorLine");
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
}