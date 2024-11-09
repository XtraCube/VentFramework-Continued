using System.Linq;
using InnerNet;
using VentLib.Logging;
using VentLib.Networking;
using VentLib.Utilities.Harmony.Attributes;

namespace VentLib.Lobbies.Patches;

internal class LobbyStatusPatches
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LobbyStatusPatches));
    [QuickPrefix(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
    private static void UpdateStatusInGame(AmongUsClient __instance)
    {
        if (!__instance.AmHost) return;
        if (!NetworkRules.AllowRoomDiscovery) return;
        log.Info($"Updating Lobby Status: {LobbyStatus.InGame}", "LobbyStatus");
        LobbyChecker.UpdateModdedLobby(__instance.GameId, PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && !p.Data.Disconnected).Count(), LobbyStatus.InGame);
    }
    
    [QuickPrefix(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
    private static void UpdateStatusClosed(InnerNetClient __instance, DisconnectReasons reason)
    {
        if (reason is DisconnectReasons.NewConnection || !__instance.AmHost) return;
        if (!NetworkRules.AllowRoomDiscovery) return;
        log.Info($"Updating Lobby Status: {LobbyStatus.Closed}", "LobbyStatus");
        LobbyChecker.UpdateModdedLobby(__instance.GameId, PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && !p.Data.Disconnected).Count(), LobbyStatus.Closed);
    }
    
    [QuickPostfix(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    private static void UpdateStatusStart(LobbyBehaviour __instance)
    {   
        if (!AmongUsClient.Instance.AmHost) return;
        if (!NetworkRules.AllowRoomDiscovery) return;
        log.Info($"Updating Lobby Status: {LobbyStatus.Open}", "LobbyStatus");
        LobbyChecker.UpdateModdedLobby(AmongUsClient.Instance.GameId, PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && !p.Data.Disconnected).Count(), LobbyStatus.Open);
    }
    
    [QuickPrefix(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    private static void UpdatePlayersOnJoin(PlayerPhysics __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!NetworkRules.AllowRoomDiscovery) return;
        LobbyStatus curStatus = LobbyBehaviour.Instance == null ? LobbyStatus.InGame : LobbyStatus.Open;
        log.Info($"Updating number of players {curStatus}.", "LobbyStatus");
        
        LobbyChecker.UpdateModdedLobby(AmongUsClient.Instance.GameId, PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && !p.Data.Disconnected).Count(), curStatus);
    }

    [QuickPostfix(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    private static void UpdatePlayersOnLeave(AmongUsClient __instance, ClientData data)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!NetworkRules.AllowRoomDiscovery) return;
        LobbyStatus curStatus = LobbyBehaviour.Instance == null ? LobbyStatus.InGame : LobbyStatus.Open;
        log.Info($"Updating number of players {curStatus}.", "LobbyStatus");
        
        LobbyChecker.UpdateModdedLobby(AmongUsClient.Instance.GameId, PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && !p.Data.Disconnected).Count(), curStatus);
    }
}