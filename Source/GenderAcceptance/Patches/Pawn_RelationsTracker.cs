using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderAcceptance.Patches;

[HarmonyPatch(typeof(RimWorld.Pawn_RelationsTracker))]
public static class Pawn_RelationsTracker
{
    // Transphobic people perceive trans people as their assigned gender, so this patch replaces the checks for gender with the definitive version :sunglasses:
    [HarmonyPatch(nameof(RimWorld.Pawn_RelationsTracker.SecondaryLovinChanceFactor))]
    [HarmonyTranspiler]
    [HarmonyPriority(Priority.Last)]
    public static IEnumerable<CodeInstruction> TransphobiaPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);
        var failed = false;
        
        codeMatcher.Start()
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld,
                AccessTools.Field(typeof(TraitDefOf), nameof(TraitDefOf.Gay))))
            .ThrowIfInvalid(
                "Hmm, someone removed the gay field? This might be rather problematic depending on what they did.")
            .OnError((matcher, error) =>
            {
                // maybe try replacing all attracted to gender checks in case someone did that already
                failed = true;
                return false;
            })
            .Advance(2);
        
        var branchLabel = (Label) codeMatcher.Operand;

        codeMatcher.Start()
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld,
                AccessTools.Field(typeof(TraitDefOf), nameof(TraitDefOf.Asexual))))
            .ThrowIfInvalid("Someone already removed the asexual field...")
            .OnError((matcher, error) =>
            {
                // maybe try replacing all attracted to gender checks in case someone did that already
                failed = true;
                return false;
            })
            .Advance(-4);
        
        codeMatcher.RemoveSearchForward(instruction => 
            instruction.opcode == OpCodes.Bne_Un_S && (Label) instruction.operand == branchLabel);
        codeMatcher.RemoveInstruction(); // remove the instruction that was matched
        
        // now let's insert our code!

        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0));
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1));
        codeMatcher.InsertAndAdvance(CodeInstruction.Call(typeof(Helper), nameof(Helper.AttractedToPerson)));
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue_S, branchLabel));

        return codeMatcher.InstructionEnumeration();
    }
}