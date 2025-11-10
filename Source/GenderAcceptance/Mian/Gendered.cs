using System;
using Verse;

namespace GenderAcceptance.Mian;

public enum Gendered
{
    None,
    Masculine,
    Feminine,
    Androgynous
}

public static class GenderedUtil
{
    public static string GetGenderNoun(this Gendered gendered)
    {
        switch (gendered)
        {
            case Gendered.Androgynous:
                return "GA.Androgynous".Translate();
            case Gendered.Masculine:
                return "GA.Masculine".Translate();
            case Gendered.Feminine:
                return "GA.Feminine".Translate();
            default:
                throw new ArgumentException();
        }
    }
}