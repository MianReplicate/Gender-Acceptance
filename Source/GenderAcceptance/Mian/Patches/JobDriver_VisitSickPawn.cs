using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Simple_Trans;
using Verse;

namespace GenderAcceptance.Mian.Patches;

[HarmonyPatch(typeof(RimWorld.JobDriver_VisitSickPawn))]
public class JobDriver_VisitSickPawn
{
    // Gain intimacy for the chaser if this is a trans person
    [HarmonyPatch("MakeNewToils")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddIntimacyForChaser(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var local = generator.DeclareLocal(typeof(Chaser_Need));
        var skipLabel = generator.DefineLabel();
        var started = false;
        foreach (var code in instructions)
        {
            if (started)
            {
                started = false;
                yield return code.WithLabels(skipLabel);
            }
            else
            {
                yield return code;   
            }
            
            // right after the GainJoy method
            if (code.Calls(AccessTools.Method(typeof(Need_Joy), nameof(Need_Joy.GainJoy))))
            {
                started = true;

                // if(pawn.GetCurrentIdentity() == GenderIdentity.Transgender)
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.LoadField(typeof(RimWorld.JobDriver_VisitSickPawn), nameof(RimWorld.JobDriver_VisitSickPawn.pawn));
                yield return CodeInstruction.Call(typeof(Helper), nameof(Helper.GetCurrentIdentity));
                yield return CodeInstruction.LoadField(typeof(GenderIdentity), nameof(GenderIdentity.Transgender));
                yield return new CodeInstruction(OpCodes.Brfalse_S, skipLabel);
                
                // if(this.Patient.needs.TryGetNeed(GADefOf.Chaser_Need)) != null
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(RimWorld.JobDriver_VisitSickPawn), "get_Patient");
                yield return CodeInstruction.LoadField(typeof(Pawn), nameof(Pawn.needs));
                yield return CodeInstruction.LoadField(typeof(GADefOf), nameof(GADefOf.Chaser_Need));
                yield return CodeInstruction.Call(typeof(Pawn_NeedsTracker), nameof(Pawn_NeedsTracker.TryGetNeed));
                yield return new CodeInstruction(OpCodes.Stloc, local.LocalIndex);
                yield return new CodeInstruction(OpCodes.Ldloc, local.LocalIndex);
                yield return new CodeInstruction(OpCodes.Brfalse_S, skipLabel);
                
                // Chaser_Need.GainNeed(1 * 0.000144f * delta)
                yield return new CodeInstruction(OpCodes.Ldloc, local.LocalIndex);
                yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
                yield return new CodeInstruction(OpCodes.Ldc_R4, 0.000144f);
                yield return new CodeInstruction(OpCodes.Mul);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Conv_R4);
                yield return new CodeInstruction(OpCodes.Mul);
                yield return CodeInstruction.Call(typeof(Chaser_Need), nameof(Chaser_Need.GainNeed));
            }
        }
    }
}