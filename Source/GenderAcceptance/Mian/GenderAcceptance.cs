using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Patches.Mod_Integration;
using HarmonyLib;
using UnityEngine;
using Verse;
using Dysphoria = GenderAcceptance.Mian.Patches.Mod_Integration.Dysphoria;
using SimpleTrans = GenderAcceptance.Mian.Patches.Mod_Integration.SimpleTrans;

namespace GenderAcceptance.Mian;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        Helper.Log("Transphobia? More like trans-dimensional timey wimey shi-");

        TransDependencies.Setup();

        var harmony = new Harmony("rimworld.mian.genderacceptance");
        harmony.PatchAll();

        if (ModsConfig.IsActive("divinederivative.romance")) WayBetterRomance.Patch(harmony);

        if (ModsConfig.IsActive("lovelydovey.sex.witheuterpe")) IntimacyLovin.Patch(harmony);

        if (ModsConfig.IsActive("runaway.simpletrans")) SimpleTrans.Patch(harmony);

        if (ModsConfig.IsActive("cammy.identity.gender")) Dysphoria.Patch(harmony);
    }
}

public class GASettings : ModSettings
{
    public static GASettings Instance;
    public bool enableLogging;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableLogging, "enableLogging");
        base.ExposeData();
    }
}

public class GenderAcceptance : Mod
{
    public GenderAcceptance(ModContentPack content) : base(content)
    {
        GASettings.Instance = GetSettings<GASettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("GA.EnableLoggingExplanation".Translate(),
            ref GASettings.Instance.enableLogging);
        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "GA.ModName".Translate();
    }
}