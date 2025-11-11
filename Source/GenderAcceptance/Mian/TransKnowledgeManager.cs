using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace GenderAcceptance.Mian;

public class TransKnowledgeTracker : IExposable
{
    private Pawn pawn;
    public bool playedNotification;
    public bool sex;
    public bool transvestigate;
    public bool cameOut;

    public TransKnowledgeTracker()
    {
    }

    public TransKnowledgeTracker(Pawn pawn)
    {
        this.pawn = pawn;
    }

    public Pawn Pawn => pawn;

    public void ExposeData()
    {
        Scribe_References.Look(ref pawn, "GAPawn");

        Scribe_Values.Look(ref sex, "GASex");
        Scribe_Values.Look(ref transvestigate, "GATransvestigate");
        Scribe_Values.Look(ref cameOut, "GACameOut");
        Scribe_Values.Look(ref playedNotification, "GAPlayedNotification");
    }

    public bool BelievesTheyAreTrans()
    {
        return transvestigate || sex || cameOut;
    }
}

public static class TransKnowledgeManager
{
    public const string DEFAULT_LETTER_LABEL = "GA.PawnBelievesOtherPawnIsTransLabel";

    private static readonly Dictionary<string, string> defaultConstants =
        new()
        {
            { "didSex", "False" },
            { "cameOut", "False" },
            { "mismatchedGenitalia", "False" },
            { "transvestigate", "False" },
            { "hasAppearance", "False" },
            { "isPositive", "False" }
        };

    private static readonly Dictionary<Pawn, List<TransKnowledgeTracker>> believedToBeTransgender = new();

    public static void SetTransKnowledges(this Pawn pawn, List<TransKnowledgeTracker> knowledges)
    {
        believedToBeTransgender[pawn] = knowledges;
    }

    public static List<TransKnowledgeTracker> GetModifiableTransgenderKnowledge(this Pawn pawn, bool cleanReferences,
        bool createIfMissing = true)
    {
        believedToBeTransgender.TryGetValue(pawn, out var pawns);
        if (pawns == null && createIfMissing)
        {
            pawns = new List<TransKnowledgeTracker>();
            believedToBeTransgender[pawn] = pawns;
        }

        if (cleanReferences)
            pawns?.RemoveAll(tracker => tracker.Pawn.Discarded);
        return pawns;
    }

    public static ReadOnlyCollection<TransKnowledgeTracker> GetTransgenderKnowledges(this Pawn pawn,
        bool cleanReferences)
    {
        return GetModifiableTransgenderKnowledge(pawn, cleanReferences).AsReadOnly();
    }

    public static void OnKnowledgeLearned(Pawn pawn, Pawn otherPawn, LetterDef letter = null,
        string letterLabel = DEFAULT_LETTER_LABEL, List<RulePackDef> extraPacks = null,
        Dictionary<string, string> constants = null, List<Rule> rules = null)
    {
        if (!pawn.RaceProps.Humanlike)
            return;
        if (pawn == otherPawn)
            return;

        if (constants != null && !constants.All(element => defaultConstants.ContainsKey(element.Key)))
        {
            Helper.Error("Invalid constants given!");
            return;
        }

        var knowledge = pawn.GetKnowledgeOnPawn(otherPawn);
        if (!knowledge.BelievesTheyAreTrans())
            return;

        if (!knowledge.playedNotification)
        {
            knowledge.playedNotification = true;
            if (letter != null)
            {
                var request = new GrammarRequest();

                if (constants == null)
                    constants = defaultConstants;
                else
                    constants.AddRange(defaultConstants.Where(constant => !constants.ContainsKey(constant.Key))
                        .ToDictionary(pair => pair.Key, pair => pair.Value));

                var mainDef = GADefOf.Believes_Is_Trans;
                var text = "";

                List<RulePackDef> rulePacks = new();

                rulePacks.Add(mainDef);

                if (extraPacks != null)
                    rulePacks.AddRange(extraPacks);

                rulePacks.Add(GADefOf.Found_Out_About_Gender_Identity);
                if (GenderUtility.DoesChaserSeeTrans(pawn, otherPawn))
                    rulePacks.Add(GADefOf.Chaser_Found_Out);

                foreach (var grammarPack in rulePacks)
                {
                    request.Clear();
                    request.Includes.Add(grammarPack);
                    if (rules != null)
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

                Find.LetterStack.ReceiveLetter(
                    letterLabel.Translate(pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT")), text, letter,
                    new LookTargets(pawn, otherPawn));
            }
        }

        if (constants == null || !constants.ContainsKey("isPositive") || constants["isPositive"] == "False")
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
                (!DebugSettings.alwaysSocialFight && Rand.Value >=
                    (double)pawn.interactions.SocialFightChance(interactionDef, otherPawn)))
                return;
            if (pawn.jobs?.curJob?.def?.casualInterruptible ?? false)
                pawn.interactions.StartSocialFight(otherPawn, "GA.SocialFightTransphobia");
        }
    }

    public static bool BelievesIsTrans(this Pawn pawn, Pawn otherPawn)
    {
        return pawn.GetKnowledgeOnPawn(otherPawn).BelievesTheyAreTrans();
    }

    public static TransKnowledgeTracker GetKnowledgeOnPawn(this Pawn pawn, Pawn otherPawn)
    {
        if (pawn == otherPawn)
            throw new ArgumentException("Pawn cannot get trans knowledge on themselves.");
        var list = pawn.GetModifiableTransgenderKnowledge(false);
        var knowledge = list.Find(tracker => tracker.Pawn == otherPawn);
        if (knowledge == null)
        {
            knowledge = new TransKnowledgeTracker(otherPawn);
            list.Add(knowledge);
        }

        return knowledge;
    }

    public static void AttemptTransvestigate(this Pawn initiator, Pawn recipient, float normalChance = 0.01f,
        float appearanceChance = 0.01f)
    {
        if (initiator.GetKnowledgeOnPawn(recipient).transvestigate)
            return;
        if (!recipient.RaceProps.Humanlike)
            return;
        var relative = recipient.CalculateRelativeAppearanceFromIdentity();
        var appearanceRoll = Rand.Chance(appearanceChance - relative / 5 *
            ((initiator.story?.traits?.HasTrait(GADefOf.Chaser) ?? false) && relative < 0 ? 1.5f : 1));
        var normalRoll = Rand.Chance(normalChance);
        if (appearanceRoll || normalRoll)
        {
            var rules = new List<Rule>();
            if (appearanceRoll)
                rules.Add(new Rule_String("RECIPIENT_gendered",
                    recipient.GetOppositeGender().GetGenderedAppearance().GetGenderNoun()));
            initiator.GetKnowledgeOnPawn(recipient).transvestigate = true;
            OnKnowledgeLearned(
                initiator,
                recipient,
                LetterDefOf.NeutralEvent,
                constants: new Dictionary<string, string>
                {
                    { "transvestigate", "True" },
                    { "hasAppearance", appearanceRoll.ToString() }
                },
                rules: rules);
        }
    }
}