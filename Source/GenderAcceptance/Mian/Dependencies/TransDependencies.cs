using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

/// <summary>
/// If creating a transgender mod with support for this mod in mind, please make sure your mod can provide at least the method, GetCurrentIdentity
/// </summary>
public interface ITransDependency
{
    
    /// <summary>
    /// Retrieves whether the pawn is transgender or cisgender
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The pawn's gender identity</returns>
    public GenderIdentity GetCurrentIdentity(Pawn pawn);
    
    /// <summary>
    /// Determines whether the pawn's genitalia matches up with their gender identity
    /// </summary>
    /// <param name="pawn">THe pawn to check</param>
    /// <returns>Whether genitalia matches up with the pawn's gender identity or not</returns>
    public bool HasMatchingGenitalia(Pawn pawn);
    
    /// <summary>
    /// Checks whether the culture is transphobic, accepting or neutral
    /// </summary>
    /// <param name="pawn">THe pawn to check</param>
    /// <returns>Whether the pawn is in a culture that is transphobic, accepting or neutral</returns>
    public CultureViewOnTrans CultureOpinionOnTrans(Pawn pawn);
    
    /// <summary>
    /// Whether the pawn looks cis to other pawns.
    /// This can be used anywhere when a pawn interacts with another pawn
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>Whether the pawn appears cis or not</returns>
    public bool LooksCis(Pawn pawn);

    public bool FeaturesAppearances();
}

public static class TransDependencies
{
    /// <summary>
    /// Do not USE this manually. Use GenderUtility as its calls are more useful and accurate.
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
        } else if (detectedPackages.Empty())
        {
            Helper.Error("You have none of the transgender mods required downloaded! Please choose one to download: " + transLibraries.Keys.ToStringList(", "));
        }
    }
}