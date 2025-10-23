using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace GenderAcceptance.Patches;

[HarmonyPatch(typeof(RimWorld.ThoughtWorker_Man))]
public class ThoughtWorker_Man
{
    // The method is protected so I cant do nameof :(
    [HarmonyPatch("CurrentSocialStateInternal")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> GetGender(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].LoadsField(AccessTools.Field(typeof(Pawn), nameof(Pawn.gender))))
            {
                var label = codes[i - 1].labels[0];
                codes.RemoveRange(i - 1, 2);
                codes.InsertRange(i - 1, [
                    new CodeInstruction(OpCodes.Ldarg_1).WithLabels(label),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    CodeInstruction.Call(typeof(Helper), nameof(Helper.GetPerceivedGender)),
                ]);
            }
        }

        return codes.AsEnumerable();
    }
}