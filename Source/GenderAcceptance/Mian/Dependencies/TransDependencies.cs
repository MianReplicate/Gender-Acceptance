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
    /// Determines whether the pawn is in a culture with transphobia or not.
    /// </summary>
    /// <param name="pawn">THe pawn to check</param>
    /// <returns>Whether the pawn is in a culture with transphobia or not</returns>
    public bool IsInCultureWithTransphobia(Pawn pawn);
    
    /// <summary>
    /// Whether the pawn looks cis to other pawns.
    /// This is used in InteractionDefs and other places of the code to determine whether someone thinks a pawn is trans or cis
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>Whether the pawn appears cis or not</returns>
    public bool LooksCis(Pawn pawn);
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
        }
    }
}