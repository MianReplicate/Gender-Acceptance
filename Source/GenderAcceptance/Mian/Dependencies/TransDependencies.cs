using System;
using System.Collections.Generic;
using System.Linq;
using GenderAcceptance.Mian.Utilities;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

/// <summary>
///     If creating a transgender mod with support for this mod in mind, please make sure your mod can provide at least the
///     method, GetCurrentIdentity
/// </summary>
public interface ITransDependency
{
    /// <summary>
    ///     Retrieves whether the pawn is transgender or cisgender
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The pawn's trans status</returns>
    public GenderIdentity GetCurrentIdentity(Pawn pawn);
    
    /// <summary>
    ///     Gets the pawn's gender identity (not their sex)
    /// </summary>
    /// <param name="pawn">The pawn</param>
    /// <returns>The pawn's gender identity</returns>
    public ActualGender GetActualGender(Pawn pawn);

    /// <summary>
    ///     Determines whether the pawn's genitalia matches up with their gender identity
    /// </summary>
    /// <param name="pawn">THe pawn to check</param>
    /// <returns>Whether genitalia matches up with the pawn's gender identity or not</returns>
    public bool AppearsToHaveMatchingGenitalia(Pawn pawn);

    /// <summary>
    ///     Checks whether the culture is transphobic, accepting or neutral
    /// </summary>
    /// <param name="ideo">The culture to check</param>
    /// <returns>Whether the culture is transphobic, accepting or neutral</returns>
    public CultureViewOnTrans CultureOpinionOnTrans(Ideo ideo);

    /// <summary>
    ///     Calculates how gendered a pawn is depending on the trans mod used.
    ///     The higher the points, the more masculine. The closer to zero, the more androgynous. If below 0, they are feminine.
    /// </summary>
    /// <param name="pawn">The pawn to check</param>
    /// <returns>The gendered points for the pawn</returns>
    public float GetGenderedPoints(Pawn pawn);
}

public static class TransDependencies
{
    /// <summary>
    ///     Do not USE this manually. Use GenderUtility as its calls are more useful and accurate.
    /// </summary>
    public static ITransDependency TransLibrary;

    private static readonly Dictionary<string, Type> TransLibraries = new()
    {
        { "lovelydovey.sex.withrosaline:cammy.identity.gender", typeof(DysGenderWorks) },
        { "cammy.identity.gender", typeof(Dysphoria) },
        { "lovelydovey.sex.withrosaline", typeof(GenderWorks) },
        { "runaway.simpletrans", typeof(SimpleTrans) }
    };

    /// <summary>
    ///     Adds an additional trans dependency. Use if you are making a trans mod and are trying to add compatibility with this mod!
    /// </summary>
    /// <param name="id">The identification of the mod</param>
    /// <param name="type">The class type, this should implement ITransDependency</param>
    public static void AddAdditionalTransLibraryToSetup(string id, Type type)
    {
        if (!type.IsAssignableFrom(typeof(ITransDependency)))
            throw new ArgumentException($"{type} does not implement ITransDependency!");
        TransLibraries.Add(id, type);
    }

    public static void Setup()
    {
        var detectedPackages = new List<string>();
        var packagedIDs = new List<string>();

        foreach (var (mainID, libraryType) in TransLibraries)
        {
            var allIDs = mainID.Split(":");

            if (allIDs.ContainsAny(packagedIDs.Contains))
                continue;

            if (allIDs.All(ModsConfig.IsActive))
            {
                detectedPackages.Add(mainID);
                packagedIDs.AddRange(allIDs);
                if (TransLibrary != null)
                    continue;
                TransLibrary = (ITransDependency)Activator.CreateInstance(libraryType);
            }
        }

        if (detectedPackages.Count > 1)
            Helper.Error("You have multiple transgender mods! Please choose one to keep and remove the rest: " +
                         string.Join(", ", detectedPackages));
        else if (detectedPackages.Empty())
            Helper.Error("You have none of the transgender mods required downloaded! Please choose one to download: " +
                         string.Join(", ", TransLibraries.Keys));

        Helper.Log("Applying library: " + TransLibrary.GetType().Name);
    }
}