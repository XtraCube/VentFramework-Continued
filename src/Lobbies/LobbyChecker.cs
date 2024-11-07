using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InnerNet;
using UnityEngine.Networking;
using VentLib.Lobbies.Patches;
using VentLib.Logging;
using VentLib.Version;
using VentLib.Version.BuiltIn;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VentLib.Lobbies;

public class LobbyChecker
{
    private static StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LobbyChecker));
    //private static string LobbyEndpoint = "https://testing-lotus.eps.lol/lobbies";
    //private static string LobbyUpdateEndpoint = "https://testing-lotus.eps.lol/update-lobby";
    private const string LobbyEndpoint = "https://lobbies.lotusau.top/lobbies";
    private const string LobbyUpdateEndpoint = "https://lobbies.lotusau.top/update-lobby";
    private static readonly HttpClient Client = new();
    private static Dictionary<int, ModdedLobby> _moddedLobbies = new();

    private static readonly Regex SpecialCharacterRegex = new("[^A-Za-z-]*");

    // ReSharper disable once InconsistentNaming
    internal static IEnumerator POSTModdedLobby(int gameId, string host) {
        UnityWebRequest PostLobby = UnityWebRequest.Post(LobbyEndpoint, "");
        Version.Version version = VersionControl.Instance.Version ?? new NoVersion();
        PostLobby.SetRequestHeader("game-id", gameId.ToString());
        PostLobby.SetRequestHeader("game-code", GameCode.IntToGameNameV2(gameId));
        PostLobby.SetRequestHeader("version", version.ToSimpleName());
        PostLobby.SetRequestHeader("mod-name", Vents.AssemblyNames[Vents.RootAssemby]);
        PostLobby.SetRequestHeader("game-host", SpecialCharacterRegex.Replace(host.Replace(" ", "-"), ""));
        PostLobby.SetRequestHeader("region", ServerManager.Instance.CurrentRegion.Name);

        yield return PostLobby.SendWebRequest();

        if (PostLobby.result != UnityWebRequest.Result.Success) {
            log.Exception($"Error while posting lobby, returned {PostLobby.responseCode}", PostLobby.error);
        }
        yield return null;
    }

    internal static void UpdateModdedLobby(int gameId, LobbyStatus lobbyStatus)
    {
        HttpRequestMessage requestMessage = new();
        requestMessage.RequestUri = new Uri(LobbyUpdateEndpoint);
        requestMessage.Method = HttpMethod.Post;
        requestMessage.Headers.Add("game-id", gameId.ToString());
        requestMessage.Headers.Add("status", lobbyStatus.ServerString());
        Client.SendAsync(requestMessage);
    }

    // ReSharper disable once InconsistentNaming
    
    // this may be used in the future so i won't remove it, 
    /*internal static void GETModdedLobbies() 
    {
        Task<HttpResponseMessage> response = Client.GetAsync(LobbyEndpoint);
        SyncTaskWaiter<HttpResponseMessage> waiter = new(response);
        Async.Schedule(() => WaitForResponse(waiter, 0), 0.25f);
    }

    private static void WaitForResponse(SyncTaskWaiter<HttpResponseMessage> response, int times)
    {
        if (times > 20) log.Fatal("Failed to get modded lobbies");
        else if (!response.Finished)
            Async.Schedule(() => WaitForResponse(response, times + 1), 1f);
        else HandleResponse(response.Response);
    }}*/
    
    internal static void HandleResponse(Task<HttpResponseMessage>? response)
    {
        if (response != null)
        {
            StreamReader reader = new(response.Result.Content.ReadAsStream());
            string result = reader.ReadToEnd();
            reader.Close();
            log.Log(LogLevel.Fatal, $"Response from lobby server: {result}", "ModdedLobbyCheck");
            _moddedLobbies = JsonSerializer.Deserialize<Dictionary<int, ModdedLobby>>(result)!;
        }

        LobbyListingsPatch.ModdedGames.ForEach(game =>
        {
            var button = game.Item2!;
            if (!_moddedLobbies.TryGetValue(game.Item1, out ModdedLobby? lobby))
            {
                button.LanguageText.text = "Vanilla";
                return;
            }

            button.LanguageText.text = lobby.Mod;
            button.NameText.text = $"{lobby.Host}'s Lobby";
        });
    }
}