using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

public interface ITransDependency
{
    public GenderIdentity GetCurrentIdentity(Pawn pawn);
    public bool HasMismatchingGenitalia(Pawn pawn);

    public bool IsInCultureWithTransphobia(Pawn pawn);
}

public static class TransDependencies
{
    /// <summary>
    /// Do not USE this manually. Use Helper as its calls are more useful and accurate.
    /// </summary>
    public static ITransDependency TransLibrary;
    
    private static Dictionary<string, Type> transLibraries = new()
    {
        {"cammy.identity.gender", typeof(Dysphoria)},
        {"lovelydovey.sex.withrosaline", typeof(GenderWorks)},
        {"runaway.simpletrans", typeof(SimpleTrans)}
    };

    public static void Setup()
    {
        var detectedPackages = new List<string>();
        
        foreach (var (id, libraryType) in transLibraries)
        {
            if (ModsConfig.IsActive(id))
            {
                detectedPackages.Add(id);
                if (TransLibrary != null)
                    continue;
                TransLibrary = (ITransDependency) Activator.CreateInstance(libraryType);
            }
        }

        if (detectedPackages.Count > 1)
        {
            Helper.Error("You have multiple transgender mods! Please choose one to keep and remove the rest: " + detectedPackages.ToStringList(", "));
        }
    }
}