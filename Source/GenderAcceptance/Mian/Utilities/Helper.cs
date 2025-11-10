using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GenderAcceptance.Mian;

public static class Helper
{
    public static void Log(string text)
    {
        Verse.Log.Message("[Topic of Gender] " + text);
    }
    public static void Debug(string text)
    {
        Verse.Log.Message("[Topic of Gender] " + text);
    }

    public static void Error(string text)
    {
        Verse.Log.Error("[Topic of Gender] " + text);
    }
}