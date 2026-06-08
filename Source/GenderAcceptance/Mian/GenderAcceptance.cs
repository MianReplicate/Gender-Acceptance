using System;
using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Patches.Mod_Integration;
using GenderAcceptance.Mian.Utilities;
using HarmonyLib;
using Multiplayer.API;
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
        
        var harmony = new Harmony("rimworld.mian.genderacceptance");
        harmony.PatchAll();

        if (ModsConfig.IsActive("divinederivative.romance"))
        {
            WayBetterRomance.Patch(harmony);
            Constants.WBREnabled = true;
        }

        if (ModsConfig.IsActive("lovelydovey.sex.witheuterpe")) IntimacyLovin.Patch(harmony);

        if (ModsConfig.IsActive("runaway.simpletrans")) SimpleTrans.Patch(harmony);

        if (ModsConfig.IsActive("cammy.identity.gender")) Dysphoria.Patch(harmony);

        // Multiplayer code
        if (!MP.enabled)
            return;
        
        MP.RegisterAll();
    }
}

public class GASettings : ModSettings
{
    public static GASettings Instance;
    public float baseRandomOuttingChance;
    public bool colonyIdeologyOverPawnIdeology;
    public bool nonTransphobicPeopleNeverOut;
    public bool enableLogging;
    
    public const float DefaultBaseRandomOuttingChance = 0.01f;
    public const bool DefaultColonyIdeologyOverPawnIdeology = true;
    public const bool DefaultNonTransphobicPeopleNeverOut = false;
    public const bool DefaultEnableLogging = false;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref enableLogging, "enableLogging", DefaultEnableLogging, true);
        Scribe_Values.Look(ref baseRandomOuttingChance, "baseRandomOuttingChance", DefaultBaseRandomOuttingChance, true);
        Scribe_Values.Look(ref colonyIdeologyOverPawnIdeology, "colonyIdeologyOverPawnIdeology", DefaultColonyIdeologyOverPawnIdeology, true);
        Scribe_Values.Look(ref nonTransphobicPeopleNeverOut, "nonTransphobicPeopleNeverOut", DefaultNonTransphobicPeopleNeverOut, true);
        base.ExposeData();
    }
}

public class GenderAcceptance : Mod
{
    public GenderAcceptance(ModContentPack content) : base(content)
    {
        GASettings.Instance = GetSettings<GASettings>();
        Constants.Version = content.ModMetaData.ModVersion;
        
        LongEventHandler.QueueLongEvent(TransDependencies.Startup, "GA.LongEvent.StartingTransLibraries", false, null);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled("GA.EnableLoggingExplanation".Translate(),
            ref GASettings.Instance.enableLogging);
        
        GASettings.Instance.baseRandomOuttingChance = listingStandard.SliderLabeled(
                                                       "GA.BaseRandomOuttingChance".Translate(
                                                           GASettings.DefaultBaseRandomOuttingChance * 100f,
                                                           GASettings.Instance.baseRandomOuttingChance * 100f),
                                                       GASettings.Instance.baseRandomOuttingChance * 100f, 0f, 100f, tooltip: "GA.BaseRandomOuttingChanceTip".Translate()) /
                                                   100f;
        listingStandard.CheckboxLabeled("GA.ColonyIdeologyOverPawnIdeology".Translate(),
            ref GASettings.Instance.colonyIdeologyOverPawnIdeology,
            tooltip: "GA.ColonyIdeologyOverPawnIdeologyTip".Translate());      
        listingStandard.CheckboxLabeled("GA.NonTransphobicPeopleNeverOut".Translate(),
            ref GASettings.Instance.nonTransphobicPeopleNeverOut,
            tooltip: "GA.NonTransphobicPeopleNeverOutTip".Translate());
        if (listingStandard.ButtonText("GA.ResetSettings".Translate()))
        {
            GASettings.Instance.enableLogging = GASettings.DefaultEnableLogging;
            GASettings.Instance.baseRandomOuttingChance = GASettings.DefaultBaseRandomOuttingChance;
            GASettings.Instance.colonyIdeologyOverPawnIdeology = GASettings.DefaultColonyIdeologyOverPawnIdeology;
            GASettings.Instance.nonTransphobicPeopleNeverOut = GASettings.DefaultNonTransphobicPeopleNeverOut;
        }
        
        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "GA.ModName".Translate();
    }
}