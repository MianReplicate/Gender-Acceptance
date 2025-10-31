using System.Collections.Generic;
using System.Linq;
using GenderAcceptance.Mian.Patches.Mod_Integration;
using HarmonyLib;
using Verse;
using Simple_Trans;
using RelationsUtility = RimWorld.RelationsUtility;

namespace GenderAcceptance.Mian;
    public static class Helper
    {
        public static void Log(string text)
        {
            Verse.Log.Message("[Gender Acceptance] "+ text);
        }
        
        // Transphobic people see trans people as their AGAB, therefore a straight transphobic man could be attracted to a trans man based on this code
        public static IEnumerable<CodeInstruction> ReplaceAttractGenderWithPerceivedGender(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                // otherPawn.gender => Helper.AttractedToPerson(pawn, otherPawn)
                if (codes[i].LoadsField(AccessTools.Field(typeof(Pawn), nameof(Pawn.gender)))
                    && codes[i + 1].Calls(AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.AttractedToGender))))
                {
                    codes[i] = CodeInstruction.Call(typeof(Helper), nameof(AttractedToPerson));
                    codes.RemoveAt(i + 1);
                }
            }
            
            return codes.AsEnumerable();
        }

        public static float ChaserFactor(Pawn pawn, Pawn target)
        {
            if (ChaserSeesFetish(pawn, target))
                return 2f;
            return 0f;
        }
        
        // does a chaser find their fetish??
        public static bool ChaserSeesFetish(Pawn initiator, Pawn recipient)
        {
            if ((initiator.story?.traits?.HasTrait(GADefOf.Chaser) ?? false) && recipient.GetCurrentIdentity() == GenderIdentity.Transgender)
                return AttractedToPerson(initiator, recipient);
            return false;
        }

        public static bool IsTransphobic(this Pawn pawn, bool includePrecept=true)
        {
            return (pawn.story?.traits?.HasTrait(GADefOf.Transphobic) ?? false) || 
                   (includePrecept && pawn.GetCurrentIdentity() == GenderIdentity.Cisgender 
                    && (pawn.Ideo?.HasPrecept(GADefOf.Transgender_Despised) ?? false));
        }

        public static Gender GetPerceivedGender(Pawn initiator, Pawn recipient)
        {
            if (recipient.GetCurrentIdentity() != GenderIdentity.Transgender)
                return recipient.gender;

            if (recipient.gender != Gender.Male && recipient.gender != Gender.Female)
                return recipient.gender; // idk what to do otherwise if im being honest. e.g. nonbinary people
            
            return initiator.IsTransphobic() ? recipient.gender == Gender.Male ? Gender.Female : Gender.Male : recipient.gender;
        }

        public static bool AttractedToPerson(Pawn initiator, Pawn recipient)
        {
            return RelationsUtility.AttractedToGender(initiator, GetPerceivedGender(initiator, recipient));
        }

        public static bool AttractedToEachOther(Pawn pawn1, Pawn pawn2)
        {
            return AttractedToPerson(pawn1, pawn2) && AttractedToPerson(pawn2, pawn1);
        }
        
        public static int CountGenderIndividuals(Map map, GenderIdentity gender)
        {
            int count = 0;
            List<Pawn> colonists = map.mapPawns.FreeColonists;

            foreach (Pawn pawn in colonists)
            {
                if (pawn.Dead || pawn.GetCurrentIdentity() != gender) continue;

                count++;
            }
            return count;
        }
        public static GenderIdentity GetCurrentIdentity(this Pawn pawn)
        {
            if (pawn.health?.hediffSet?.HasHediff(SimpleTransPregnancyUtility.transDef) ?? false)
            {
                return GenderIdentity.Transgender;
            }
            if (pawn.health?.hediffSet?.HasHediff(SimpleTransPregnancyUtility.cisDef) ?? false)
            {
                return GenderIdentity.Cisgender;
            }
            return GenderIdentity.Cisgender;
        }   
    }

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
            }
            
            if (ModsConfig.IsActive("lovelydovey.sex.witheuterpe"))
            {
                IntimacyLovin.Patch(harmony);
            }
        }
    }