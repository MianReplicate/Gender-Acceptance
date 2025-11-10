using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.Needs;

public enum ChaserCategory : byte
{
    Inactive,
    JustHadIntimacy,
    Neutral,
    LongWhile,
    ExtremelyLongWhile,
    Aching
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

    private int lastGainTick = -999;

    public Chaser_Need(Pawn pawn)
        : base(pawn)
    {
        threshPercents = new List<float>(Thresholds);
    }

    public override bool ShowOnNeedList => !Disabled;

    public override int GUIChangeArrow => IsFrozen ? 0 : !GainingNeed ? -1 : 1;
    private bool GainingNeed => Find.TickManager.TicksGame < lastGainTick + 15;

    public ChaserCategory CurCategory
    {
        get
        {
            if (Disabled)
                return ChaserCategory.Inactive;
            if (CurLevel > (double)Thresholds[0])
                return ChaserCategory.JustHadIntimacy;
            if (CurLevel > (double)Thresholds[1])
                return ChaserCategory.Neutral;
            if (CurLevel > (double)Thresholds[2])
                return ChaserCategory.LongWhile;
            if (CurLevel > (double)Thresholds[3])
                return ChaserCategory.ExtremelyLongWhile;
            return ChaserCategory.Aching;
        }
    }

    private float FallPerInterval
    {
        get
        {
            switch (CurCategory)
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

    private bool Disabled => pawn.Dead || (!pawn.story?.traits?.HasTrait(GADefOf.Chaser) ?? false);

    public override void SetInitialLevel()
    {
        CurLevel = Rand.Range(0.7f, 1f);
    }

    private void GainNeed(float amount)
    {
        if (amount <= 0.0 || curLevelInt >= 1)
            return;
        curLevelInt += amount;
        lastGainTick = Find.TickManager.TicksGame;
    }

    public void GainNeedFromInteraction()
    {
        GainNeed(Rand.Range(0.1f, 0.15f));
    }

    public void GainNeedFromSex()
    {
        GainNeed(Rand.Range(1f, 1.5f));
    }

    public override void NeedInterval()
    {
        if (Disabled)
        {
            CurLevel = 1f;
        }
        else
        {
            if (IsFrozen)
                return;
            CurLevel -= FallPerInterval;
        }
    }
}