using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public abstract class TransDependency : ITransDependency
{
    public abstract GenderIdentity GetCurrentIdentity(Pawn pawn);
    public abstract bool HasMismatchingGenitalia(Pawn pawn);
    
    public virtual bool IsInCultureWithTransphobia(Pawn pawn)
    {
        return pawn.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false;
    }
}