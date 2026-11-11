using Multiplayer.API;

namespace GenderAcceptance.Mian;

public class MultiplayerSupport
{
    public static void Add()
    {
        // Multiplayer code
        if (!MP.enabled)
            return;
        
        MP.RegisterAll();
    }
}