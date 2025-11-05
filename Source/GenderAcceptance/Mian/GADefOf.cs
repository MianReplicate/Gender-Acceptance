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

    [MayRequireIdeology] public static PreceptDef Transgender_Despised;
    [MayRequireIdeology] public static PreceptDef Transgender_Adored;

    public static RulePackDef Suspicions_About_Trans;
    public static RulePackDef Chaser_Found_Out;
    public static RulePackDef Transphobe_Found_Out;
    public static RulePackDef Coming_Out_Positive_Pack;
    public static RulePackDef Coming_Out_Negative_Pack;

    public static ThoughtDef CameOutNegative;
    public static ThoughtDef CameOutPositive;
    public static ThoughtDef FoundOutPawnIsTransMoodPositive;
    public static ThoughtDef FoundOutPawnIsTransMoodNegative;

    public static ThoughtDef Transgender_Person_Joined_Positive;
    public static ThoughtDef Transgender_Person_Joined_Negative;
    public static ThoughtDef Cisgender_Person_Joined;
    public static ThoughtDef Cisphobia;
    public static ThoughtDef Transphobia;
        
    public static ThoughtDef Similar;
    public static ThoughtDef Chaser_Need_Thought;
    
    public static ThoughtDef Accidental_Misgender;

    public static ThoughtDef Dehumanized;

    public static InteractionDef Transvestigate;

    static GADefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(GADefOf));

        if (!ModsConfig.IsActive("cammy.identity.gender"))
            Transgender_Despised = DefDatabase<PreceptDef>.GetNamed("Transgender_Despised");
    }
}
