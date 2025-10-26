using RimWorld;
using Verse;

namespace GenderAcceptance.Mian;

[DefOf]
public static class GADefOf
{
    public static TraitDef Chaser;
    public static TraitDef Transphobic;
    public static TraitDef Cisphobic;

    public static NeedDef Chaser_Need;

    [MayRequireIdeology] public static PreceptDef Transgender_Adored;
    [MayRequireIdeology] public static PreceptDef Transgender_Despised;
    [MayRequireIdeology] public static IssueDef Transgender;

    [MayRequireIdeology] public static ThoughtDef AmountOfTransgender_Disliked;
    [MayRequireIdeology] public static ThoughtDef NegativeViewOnTransgender;
    [MayRequireIdeology] public static ThoughtDef AmountOfTransgender_Liked;
    [MayRequireIdeology] public static ThoughtDef PositiveViewOnTransgender;
    [MayRequireIdeology] public static ThoughtDef Internal_Transphobia;

    public static ThoughtDef TransgenderPersonJoined;
    public static ThoughtDef CisgenderPersonJoined;
    public static ThoughtDef Cisphobia;
    public static ThoughtDef Transphobia;
        
    public static ThoughtDef Similar;
    public static ThoughtDef Chaser_Need_Thought;
    
    public static ThoughtDef Accidental_Misgender;

        
    static GADefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(GADefOf));
    }
}
