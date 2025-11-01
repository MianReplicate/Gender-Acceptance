using System.Linq;
using GenderAcceptance.Mian.Dependencies;
using GenderAcceptance.Mian.Patches.Mod_Integration;
using HarmonyLib;
using RimWorld;
using Verse;

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

            if (ModsConfig.IsActive("divinederivative.romance"))
            {
                WayBetterRomance.Patch(harmony);
            }
            
            if (ModsConfig.IsActive("lovelydovey.sex.witheuterpe"))
            {
                IntimacyLovin.Patch(harmony);
            }
            
            if (ModsConfig.IsActive("runaway.simpletrans"))
            {
                Patches.Mod_Integration.SimpleTrans.Patch(harmony);
            }
        }
    }