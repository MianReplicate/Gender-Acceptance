using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BetterRomance;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Patches;

[HarmonyPatch(typeof(RimWorld.InteractionWorker_RomanceAttempt))]
public class InteractionWorker_RomanceAttempt
{
        //Adds a sexuality factor to the romance success chance tooltip
    //Adjust tooltip for psychic bonding
    [HarmonyPatch(nameof(RimWorld.InteractionWorker_RomanceAttempt.RomanceFactors))]
    public static class InteractionWorker_RomanceAttempt_RomanceFactors
    {
        //If psychic bonding will result, only show that on the tooltip
        public static bool Prefix(Pawn romancer, Pawn romanceTarget, ref string __result)
        {
            Gene_PsychicBonding initiatorGene = romancer.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
#if v1_4
            if (initiatorGene is not null && InteractionWorker_RomanceAttempt.CanCreatePsychicBondBetween_NewTemp(romancer, romanceTarget))
#else
            if (initiatorGene is not null && RimWorld.InteractionWorker_RomanceAttempt.CanCreatePsychicBondBetween(romancer, romanceTarget))
#endif
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine((string)InfoHelper.RomanceFactorLine.Invoke(null, [initiatorGene.LabelCap, 1f]));
                __result = stringBuilder.ToString();
                return false;
            }
            return true;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            MethodInfo PrettinessFactor = AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.PrettinessFactor));
            Label newLabel = ilg.DefineLabel();
            Label oldLabel = new();
            LocalBuilder num = ilg.DeclareLocal(typeof(float));
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
                    //num = RomanceUtilities.SexualityFactor(romanceTarget, romancer);
                    yield return new CodeInstruction(OpCodes.Ldarg_1).WithLabels(newLabel);
                    yield return new(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(RomanceUtilities), nameof(RomanceUtilities.SexualityFactor));
                    yield return new(OpCodes.Stloc, num);
                    //if (num != 1f)
                    yield return new(OpCodes.Ldloc, num);
                    yield return new(OpCodes.Ldc_R4, 1f);
                    yield return new(OpCodes.Beq_S, oldLabel);
                    //stringBuilder.AppendLine(RomanceFactorLine("WBR.HookupChanceSexuality".Translate(), num);
                    yield return new(OpCodes.Ldloc_0);
                    yield return new(OpCodes.Ldstr, "GA.HookupChanceChaser");
                    yield return CodeInstruction.Call(typeof(Translator), nameof(Translator.Translate), [typeof(string)]);
                    yield return CodeInstruction.Call(typeof(TaggedString), "op_Implicit", [typeof(TaggedString)]);
                    yield return new(OpCodes.Ldloc, num);
                    yield return CodeInstruction.Call(typeof(InteractionWorker_RomanceAttempt), "RomanceFactorLine");
                    yield return CodeInstruction.Call(typeof(StringBuilder), nameof(StringBuilder.AppendLine), [typeof(string)]);
                    yield return new(OpCodes.Pop);

                    startFound = false;
                }
                //We want to insert our stuff after the beauty line
                if (code.Calls(PrettinessFactor))
                {
                    startFound = true;
                }
            }
        }
    }
}