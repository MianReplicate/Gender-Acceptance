using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GenderAcceptance.Mian.Dependencies;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Grammar;

namespace GenderAcceptance.Mian;

public class TransKnowledge : IExposable
{
    public bool sex;
    public bool transvestigate;
    public bool cameOut;
    public void ExposeData()
    {
        Scribe_Values.Look(ref sex, "GASex");
        Scribe_Values.Look(ref transvestigate, "GATransvestigate");
        Scribe_Values.Look(ref cameOut, "GACameOut");
    }

    // public bool IsSuspiciousOfThem()
    // {
        // return transvestigate || BelievesTheyAreTrans();
    // }
    public bool BelievesTheyAreTrans()
    {
        return transvestigate || sex || cameOut;
    }
}

public static class TransKnowledgeManager
{
    public const string DEFAULT_LETTER_LABEL = "GA.PawnBelievesOtherPawnIsTransLabel";
    
    private static Dictionary<string, string> defaultConstants =
    new(){
        {"didSex", "False"},
        {"cameOut", "False"},
        {"mismatchedGenitalia", "False"},
        {"transvestigate", "False"},
        {"hasAppearance", "False"},
        {"isPositive", "False"}
    };
    
    private static readonly Dictionary<Pawn, Dictionary<Pawn, TransKnowledge>> believedToBeTransgender = new Dictionary<Pawn, Dictionary<Pawn, TransKnowledge>>();

    // public static void SetBelievedToBeTrans(Pawn pawn, List<Pawn> pawns)
    // {
    //     believedToBeTransgender.SetOrAdd(pawn, pawns);
    // }
    public static void SetTransKnowledges(this Pawn pawn, Dictionary<Pawn, TransKnowledge> knowledges)
    {
        believedToBeTransgender[pawn] = knowledges;
    }
    public static Dictionary<Pawn, TransKnowledge> GetModifiableKnownTransgenders(this Pawn pawn, bool cleanReferences)
    {
        believedToBeTransgender.TryGetValue(pawn, out var pawns);
        if (pawns == null)
        {
            pawns = new();
            believedToBeTransgender[pawn] = pawns;
        }

        if (cleanReferences)
            pawns.RemoveAll(transPawn => transPawn.Key.Discarded);
        return pawns;
    }
    public static ReadOnlyDictionary<Pawn, TransKnowledge> GetKnownTransgenders(this Pawn pawn, bool cleanReferences)
    {
        return new ReadOnlyDictionary<Pawn, TransKnowledge>(GetModifiableKnownTransgenders(pawn, cleanReferences));
    }
    public static void OnKnowledgeLearned(Pawn pawn, Pawn otherPawn, LetterDef letter=null, string letterLabel=DEFAULT_LETTER_LABEL, List<RulePackDef> extraPacks=null, Dictionary<string, string> constants=null, List<Rule> rules=null)
    {
        if (!pawn.RaceProps.Humanlike)
            return;
        if (pawn == otherPawn)
            return;
        // if (!triggerIfCultureImpact && pawn.CultureOpinionOnTrans() != CultureViewOnTrans.Neutral)
            // return; // unless hard learned, cultures will not assume. This will make the game less annoying hopefully.
        
        // var list = believedToBeTransgender.TryGetValue(pawn, new());
        // if (list.Contains(otherPawn))
            // return;
        if (constants != null && !constants.All(element => defaultConstants.ContainsKey(element.Key)))
        {
            Helper.Error("Invalid constants given!");
            return;
        }
        // list.Add(otherPawn);
        // SetBelievedToBeTrans(pawn, list);

        var knowledge = pawn.GetKnowledgeOnPawn(otherPawn);
        if (!knowledge.BelievesTheyAreTrans())
            return;
        
        if (letter != null)
        {
            var request = new GrammarRequest();

            if (constants == null)
                constants = defaultConstants;
            else
                constants.AddRange(defaultConstants.Where(constant => !constants.ContainsKey(constant.Key)).ToDictionary(pair => pair.Key, pair => pair.Value));

            var mainDef = GADefOf.Believes_Is_Trans;
            var text = "";
            
            List<RulePackDef> rulePacks = new();
            
            rulePacks.Add(mainDef);
            
            if(extraPacks != null)
                rulePacks.AddRange(extraPacks);
            
            rulePacks.Add(GADefOf.Found_Out_About_Gender_Identity);
            if (GenderUtility.DoesChaserSeeTrans(pawn, otherPawn))
                rulePacks.Add(GADefOf.Chaser_Found_Out);

            foreach (var grammarPack in rulePacks)
            {
                request.Clear();
                request.Includes.Add(grammarPack);
                if(rules != null)
                    request.Rules.AddRange(rules);
                request.Constants.AddRange(constants);
                request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", pawn, request.Constants));
                request.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", otherPawn, request.Constants));
                
                text += (grammarPack == mainDef ? "" : "\n\n") +
                                                      GrammarResolver.Resolve(
                                                          grammarPack.FirstRuleKeyword, 
                                                          request, "extraSentencePack", 
                                                          false, 
                                                          grammarPack.FirstUntranslatedRuleKeyword);   
            }

            Find.LetterStack.ReceiveLetter(letterLabel.Translate(pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT")), text, letter, new LookTargets(pawn, otherPawn));
        }
        
        if (constants == null || (!constants.ContainsKey("isPositive") || constants["isPositive"] == "False"))
        {
            var transphobia = pawn.GetTransphobicStatus(otherPawn);
            var interactionDef = new InteractionDef
            {
                socialFightBaseChance = 0.35f *
                                        (transphobia.GenerallyTransphobic ? 1f : 0) *
                                        (transphobia.ChaserAttributeCounts ? 0.1f : 1) *
                                        (transphobia.HasTransphobicTrait ? 2 : 1) *
                                        (transphobia.TransphobicPreceptCounts ? 1.25f : 1) *
                                        NegativeInteractionUtility.NegativeInteractionChanceFactor(pawn, otherPawn)
            };

            if (!DebugSettings.enableRandomMentalStates || pawn.needs.mood == null || TutorSystem.TutorialMode ||
                !DebugSettings.alwaysSocialFight && (double)Rand.Value >=
                (double)pawn.interactions.SocialFightChance(interactionDef, otherPawn))
                return;
            if(pawn.jobs?.curJob?.def?.casualInterruptible ?? false)
                pawn.interactions.StartSocialFight(otherPawn, "GA.SocialFightTransphobia");
        }
    }
    // public static bool SuspiciousIsTrans(this Pawn pawn, Pawn otherPawn)
    // {
    //     return pawn.GetKnowledgeOnPawn(otherPawn).IsSuspiciousOfThem();
    // }
    public static bool BelievesIsTrans(this Pawn pawn, Pawn otherPawn)
    {
        return pawn.GetKnowledgeOnPawn(otherPawn).BelievesTheyAreTrans();
    }
    public static TransKnowledge GetKnowledgeOnPawn(this Pawn pawn, Pawn otherPawn)
    {
        var dict = pawn.GetModifiableKnownTransgenders(false);
        dict.TryGetValue(otherPawn, out var knowledge);
        if (knowledge == null)
        {
            knowledge = new TransKnowledge();
            dict.SetOrAdd(otherPawn, knowledge);
        }

        return knowledge;
    }

    public static void AttemptTransvestigate(this Pawn initiator, Pawn recipient, float normalChance=0.01f, float appearanceChance=0.01f)
    {
        if (initiator.GetKnowledgeOnPawn(recipient).transvestigate)
            return;
        if (!recipient.RaceProps.Humanlike)
            return;
        var relative = recipient.CalculateRelativeAppearanceFromIdentity();
        var appearanceRoll = Rand.Chance(appearanceChance - (relative / 5) * ((initiator.story?.traits?.HasTrait(GADefOf.Chaser) ?? false) && relative < 0 ? 1.5f : 1));
        var normalRoll = Rand.Chance(normalChance);
        if (appearanceRoll || normalRoll)
        {
            var rules = new List<Rule>();
            if (appearanceRoll)
                rules.Add(new Rule_String("RECIPIENT_gendered", recipient.GetOppositeGender().GetGenderedAppearance().GetGenderNoun()));
            initiator.GetKnowledgeOnPawn(recipient).transvestigate = true;
            OnKnowledgeLearned(
                initiator, 
                recipient,
                LetterDefOf.NeutralEvent, 
                constants: new()
            {
                { "transvestigate", "True" },
                { "hasAppearance", appearanceRoll.ToString()}
            }, 
                rules: rules);
        }
    }
}