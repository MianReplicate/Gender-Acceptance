using Verse;

namespace GenderAcceptance.Mian.Verbs;

public class Transvestigate : Verb
{
    // public new float EffectiveRange = 10f;

    public new ThingWithComps EquipmentSource = null;
    
    public new VerbProperties verbProps = new VerbProperties()
    {
        range = 30f
    };
    protected override bool TryCastShot()
    {
        return true;
    }
}