using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian;

public class BodyTypeGenderedDef : Def
{
    public BodyTypeDef bodyType;

    public int genderPoints;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string configError in base.ConfigErrors())
            yield return configError;
        if (bodyType == null)
            yield return "no body type def set";
    }

    public static BodyTypeGenderedDef FromBodyType(BodyTypeDef bodyType) => DefDatabase<BodyTypeGenderedDef>.AllDefs
        .Where(def => def.bodyType == bodyType).FirstOrFallback();
    public static BodyTypeGenderedDef Named(string defName) => DefDatabase<BodyTypeGenderedDef>.GetNamed(defName);
}