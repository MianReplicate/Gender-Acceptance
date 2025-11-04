using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GenderAcceptance.Mian;

public static class Helper
{
        public static bool CheckSocialFightStart(float chance, Pawn initiator, Pawn recipient)
  {
    if (!DebugSettings.enableRandomMentalStates || recipient.needs.mood == null || TutorSystem.TutorialMode || !DebugSettings.alwaysSocialFight && (double) Rand.Value >= (double) SocialFightChance(chance, initiator, recipient))
      return false;
    StartSocialFight(initiator, recipient);
    return true;
  }

  private static void StartSocialFight(Pawn initiator, Pawn otherPawn, string messageKey = "MessageSocialFight")
  {
    if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(otherPawn))
      Messages.Message((string) messageKey.Translate((NamedArgument) initiator.LabelShort, (NamedArgument) otherPawn.LabelShort, initiator.Named("PAWN1"), otherPawn.Named("PAWN2")), (LookTargets) (Thing) initiator, MessageTypeDefOf.ThreatSmall);
    initiator.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, otherPawn: otherPawn);
    otherPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, otherPawn: initiator);
    TaleRecorder.RecordTale(TaleDefOf.SocialFight, (object) initiator, (object) otherPawn);
  }

  private static bool SocialFightPossible(Pawn initiator, Pawn otherPawn)
  {
    if (!initiator.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike || !SocialInteractionUtility.HasAnyVerbForSocialFight(initiator) || !SocialInteractionUtility.HasAnyVerbForSocialFight(otherPawn) || initiator.WorkTagIsDisabled(WorkTags.Violent) || otherPawn.Downed || initiator.Downed || initiator.IsPrisoner && !otherPawn.IsPrisoner || initiator.IsSlave && !otherPawn.IsSlave)
      return false;
    DevelopmentalStage developmentalStage = initiator.ageTracker.CurLifeStage.developmentalStage;
    return developmentalStage != DevelopmentalStage.Baby && (Mathf.Abs(initiator.ageTracker.AgeBiologicalYears - otherPawn.ageTracker.AgeBiologicalYears) <= 6 || developmentalStage != DevelopmentalStage.Child) && (developmentalStage != DevelopmentalStage.Adult || otherPawn.ageTracker.AgeBiologicalYears >= 13) && (initiator.genes == null || (double) initiator.genes.SocialFightChanceFactor > 0.0) && (otherPawn.genes == null || (double) otherPawn.genes.SocialFightChanceFactor > 0.0);
  }

  private static float SocialFightChance(float chance, Pawn initiator, Pawn otherPawn)
  {
    if (!SocialFightPossible(initiator, otherPawn))
      return 0.0f;
    float num1 = chance * Mathf.InverseLerp(0.3f, 1f, initiator.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation)) * Mathf.InverseLerp(0.3f, 1f, initiator.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
    List<Hediff> hediffs = initiator.health.hediffSet.hediffs;
    for (int index = 0; index < hediffs.Count; ++index)
    {
      if (hediffs[index].CurStage != null)
        num1 *= hediffs[index].CurStage.socialFightChanceFactor;
    }
    float x1 = (float) initiator.relations.OpinionOf(initiator);
    float num2 = (double) x1 >= 0.0 ? num1 * GenMath.LerpDouble(0.0f, 100f, 1f, 0.6f, x1) : num1 * GenMath.LerpDouble(-100f, 0.0f, 4f, 1f, x1);
    if (initiator.RaceProps.Humanlike)
    {
      List<Trait> allTraits = initiator.story.traits.allTraits;
      for (int index = 0; index < allTraits.Count; ++index)
      {
        if (!allTraits[index].Suppressed)
          num2 *= allTraits[index].CurrentData.socialFightChanceFactor;
      }
    }
    int x2 = Mathf.Abs(initiator.ageTracker.AgeBiologicalYears - initiator.ageTracker.AgeBiologicalYears);
    if (x2 > 10)
    {
      if (x2 > 50)
        x2 = 50;
      num2 *= GenMath.LerpDouble(10f, 50f, 1f, 0.25f, (float) x2);
    }
    if (initiator.IsSlave)
      num2 *= 0.5f;
    if (initiator.genes != null)
      num2 *= initiator.genes.SocialFightChanceFactor;
    if (initiator.genes != null)
      num2 *= initiator.genes.SocialFightChanceFactor;
    return Mathf.Clamp01(num2);
  }
    public static void Log(string text)
    {
        Verse.Log.Message("[Topic of Gender] " + text);
    }

    public static void Error(string text)
    {
        Verse.Log.Error("[Topic of Gender] " + text);
    }
}