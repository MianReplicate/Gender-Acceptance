using RimWorld;
using Verse;

namespace GenderAcceptance.Mian.GameComps;

public class VersionTracker : GameComponent
{
    public string lastLaunchedVersion = "0.0";
    
    public static VersionTracker Instance => Current.Game.GetComponent<VersionTracker>();

    public VersionTracker(Game game)
    {
        
    }

    public override void StartedNewGame() => lastLaunchedVersion = Constants.Version;

    public override void LoadedGame()
    {
        if (!lastLaunchedVersion.Equals(Constants.Version))
        {
            lastLaunchedVersion = Constants.Version;
            
            if(Constants.WarnOnVersion.Contains(lastLaunchedVersion))
                Find.LetterStack.ReceiveLetter("GA.VersionUpdate".Translate(), "GA.VersionUpdateMain".Translate(), LetterDefOf.NegativeEvent);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastLaunchedVersion, "lastLaunchedVersion", "0.0");
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            Helper.Log("Last loaded TOG version: " + lastLaunchedVersion);
    }
}