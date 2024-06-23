using HarmonyLib;
using VentLib.Logging;

namespace VentLib.Anticheat;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ServerUpdatePatch));
    static void Postfix(ref int __result)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
        {
            log.Trace($"IsLocalGame: {__result}");
        }
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
        {
            __result += 25;
            log.Trace($"IsOnlineGame: {__result}");
        }
    }
}
[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class IsVersionModdedPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}