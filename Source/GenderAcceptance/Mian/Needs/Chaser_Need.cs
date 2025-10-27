using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GenderAcceptance.Mian.Needs;

public enum ChaserCategory : byte
{
  Inactive,
  JustHadIntimacy,
  Neutral,
  LongWhile,
  ExtremelyLongWhile,
  Aching,
}

public class Chaser_Need : Need
{
      private static readonly float[] Thresholds = new float[4]
  {
    0.8f,
    0.5f,
    0.25f,
    0.1f
  };

  public override bool ShowOnNeedList => !this.Disabled;

  public override int GUIChangeArrow => this.IsFrozen ? 0 : !this.GainingNeed ? -1 : 1;
  private bool GainingNeed => Find.TickManager.TicksGame < this.lastGainTick + 15;

  private int lastGainTick = -999;
  public ChaserCategory CurCategory
  {
    get
    {
      if (Disabled)
        return ChaserCategory.Inactive;
      if ((double) this.CurLevel > (double) Chaser_Need.Thresholds[0])
        return ChaserCategory.JustHadIntimacy;
      if ((double) this.CurLevel > (double) Chaser_Need.Thresholds[1])
        return ChaserCategory.Neutral;
      if ((double) this.CurLevel > (double) Chaser_Need.Thresholds[2])
        return ChaserCategory.LongWhile;
      if ((double) this.CurLevel > (double) Chaser_Need.Thresholds[3])
        return ChaserCategory.ExtremelyLongWhile;
      return  ChaserCategory.Aching;
    }
  }
  
  private float FallPerInterval
  {
    get
    {
      switch (this.CurCategory)
      {
        case ChaserCategory.Aching:
          return 0.0001f;
        case ChaserCategory.ExtremelyLongWhile:
          return 0.0003f;
        case ChaserCategory.LongWhile:
          return 0.0006f;
        case ChaserCategory.Neutral:
          return 0.00105f;
        case ChaserCategory.JustHadIntimacy:
          return 0.0015f;
        default:
          throw new InvalidOperationException();
      }
    }
  }

  private bool Disabled => this.pawn.Dead || (!this.pawn.story?.traits?.HasTrait(GADefOf.Chaser) ?? false);

  public Chaser_Need(Pawn pawn)
    : base(pawn)
  {
    this.threshPercents = new List<float>((IEnumerable<float>) Chaser_Need.Thresholds);
  }

  public override void SetInitialLevel() => this.CurLevel = Rand.Range(0.7f, 1f);
  
  private void GainNeed(float amount)
  {
    if ((double) amount <= 0.0 || this.curLevelInt >= 1)
      return;
    this.curLevelInt += amount;
    this.lastGainTick = Find.TickManager.TicksGame;
  }

  public void GainNeedFromInteraction()
  {
    GainNeed(Rand.Range(0.1f, 0.3f));
  }

  public void GainNeedFromSex()
  {
    GainNeed(Rand.Range(1f, 1.5f));
  }

  public override void NeedInterval()
  {
    if (this.Disabled)
    {
      this.CurLevel = 1f;
    }
    else
    {
      if (this.IsFrozen)
        return;
      this.CurLevel -= this.FallPerInterval;
    }
  }
}