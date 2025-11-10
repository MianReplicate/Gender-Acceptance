using Verse;

namespace GenderAcceptance.Mian.Verbs;

public class Transvestigate : Verb
{
    public new ThingWithComps EquipmentSource = null;

    public new VerbProperties verbProps = new()
    {
        range = 30f
    };

    protected override bool TryCastShot()
    {
        return true;
    }
}