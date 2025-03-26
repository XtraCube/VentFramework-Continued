using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using InnerNet;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using PButton = PassiveButton;

namespace VentLib.Lobbies.Patches;

[HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.HandleList))]
internal class LobbyListingsPatch
{
    private static StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LobbyListingsPatch));
    internal static List<(int, MatchMakerGameButton?)> ModdedGames = new();

    internal static void Prefix(FindAGameManager __instance)
    {
        // seems to be down, so we'll just not call this...
        LobbyChecker.GETModdedLobbies();
    }
    
    internal static void Postfix(FindAGameManager __instance, [HarmonyArgument(0)] InnerNetClient.TotalGameData totalGames, [HarmonyArgument(1)] HttpMatchmakerManager.FindGamesListFilteredResponse response)
    {
        // Useless currently... but will show Lotus Lobbies of the server eventually... someday.
        ModdedGames.Clear();
    }
}