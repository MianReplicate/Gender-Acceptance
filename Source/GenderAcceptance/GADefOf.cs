using RimWorld;
using Verse;

namespace GenderAcceptance;

[DefOf]
public static class GADefOf
{
    public static TraitDef Chaser;
    public static TraitDef Transphobic;
    public static TraitDef Cisphobic;

    [MayRequireIdeology] public static PreceptDef Transgender_Adored;
    [MayRequireIdeology] public static PreceptDef Transgender_Despised;
    [MayRequireIdeology] public static IssueDef Transgender;

    [MayRequireIdeology] public static ThoughtDef AmountOfTransgender_Disliked;
    [MayRequireIdeology] public static ThoughtDef NegativeViewOnTransgender;
    [MayRequireIdeology] public static ThoughtDef AmountOfTransgender_Liked;
    [MayRequireIdeology] public static ThoughtDef PositiveViewOnTransgender;
    [MayRequireIdeology] public static ThoughtDef Internal_Transphobia;

    public static ThoughtDef Cisphobia;
    public static ThoughtDef Transphobia;
        
    public static ThoughtDef Similar;
    // public static ThoughtDef Chaser_Transgender_Spotted;
        
    static GADefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(GADefOf));
    }
}
