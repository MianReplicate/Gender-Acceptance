using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public abstract class TransDependency : ITransDependency
{
    public abstract GenderIdentity GetCurrentIdentity(Pawn pawn);
    public abstract bool HasMatchingGenitalia(Pawn pawn);
    
    public virtual CultureViewOnTrans CultureOpinionOnTrans(Pawn pawn)
    {
        return pawn.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false ? CultureViewOnTrans.Despised : pawn.Ideo?.HasPrecept(GADefOf.Transgender_Adored) ?? false ? CultureViewOnTrans.Adored : CultureViewOnTrans.Neutral;
    }

    public virtual bool LooksCis(Pawn pawn)
    {
        return true;
    }

    public virtual bool FeaturesAppearances()
    {
        return false;
    }
}