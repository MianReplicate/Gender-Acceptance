using System;
using System.Collections.Generic;
using System.Linq;
using GenderAcceptance.Mian.Utilities;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Dependencies;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransLibrary : Attribute
{
    public string[] PackageIds { get;  }
    public string Name { get; }
    public int Priority { get; }
    
    public TransLibrary(string[] packageIds, string name, TLPriority priority=TLPriority.Normal)
    {
        PackageIds = packageIds;
        Name = name;
        Priority = (int) priority;
    }
    
    public override object TypeId {
        get {
            return this;
        }
    }
}

public enum TLPriority
{
    First = 0,
    ExtremelyHigh = 10,
    HigherThanNormal = 50,
    Normal = 100,
    Low = 1000,
}

public static class TransDependencies
{
    internal static TransDependency TransLibrary;

    public static void Startup()
    {
        var transLibraries = LoadedModManager.RunningModsListForReading.SelectMany(content => content.assemblies.loadedAssemblies).SelectMany(a => a.GetTypes())
            .Where(t => t.HasAttribute<TransLibrary>())
            .SelectMany(t => (TransLibrary[])t.GetCustomAttributes(typeof(TransLibrary), false),
                resultSelector: ((type, compat) => new { type, compat })).ToList();

        Type libraryToUse = null;
        var highestPriority = (int)TLPriority.Low;

        foreach (var library in transLibraries.Where((arg => arg.compat.PackageIds.All(ModsConfig.IsActive))))
        {
            if (library.compat.Priority < highestPriority)
            {
                highestPriority = library.compat.Priority;
                libraryToUse = library.type;
            }
        }

        if (libraryToUse == null){
            Helper.Error(
                "You have none of the transgender packages required downloaded! Please choose out of these, which packages to utilize: " +
                string.Join(", ", transLibraries.Select(library => library.compat.Name)));
            return;
        }
        
        TransLibrary = (TransDependency) Activator.CreateInstance(libraryToUse);
        Helper.Log("Applying library: " + TransLibrary.GetType().Name);
    }
}