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
using VentLib.Utilities.Extensions;
using VentLib.Version;
using VentLib.Version.BuiltIn;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VentLib.Lobbies;

public class LobbyChecker
{
    private static StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(LobbyChecker));

    private static List<ILobbyServerInfo> LobbyServers = new() {
        new DefaultServerInfo()
    };

    private static readonly HttpClient Client = new();
    private static Dictionary<int, ModdedLobby> _moddedLobbies = new();

    private static readonly Regex SpecialCharacterRegex = new("[^A-Za-z-]*");

    public static void AddEndpoint(ILobbyServerInfo serverInfo, bool replaceAll = false)
    {
        if (serverInfo == null) return;
        if (replaceAll) LobbyServers.Clear();
        LobbyServers.Add(serverInfo);
    }

    // ReSharper disable once InconsistentNaming
    internal static IEnumerator PostLobbyToEndpoints(int gameId, string host, int playerCount) 
    {
        List<UnityWebRequest> requests = new();
        LobbyServers.ForEach(curInfo => {
            if (curInfo.CreateEndpoint() == "") return;
            UnityWebRequest PostLobby = UnityWebRequest.Post(curInfo.CreateEndpoint(), "");
            Version.Version version = VersionControl.Instance.Version ?? new NoVersion();
            PostLobby.SetRequestHeader("game-id", gameId.ToString());
            PostLobby.SetRequestHeader("game-code", GameCode.IntToGameNameV2(gameId));
            PostLobby.SetRequestHeader("version", version.ToSimpleName());
            PostLobby.SetRequestHeader("mod-name", Vents.AssemblyNames[Vents.RootAssemby]);
            PostLobby.SetRequestHeader("game-host", SpecialCharacterRegex.Replace(host.Replace(" ", "-"), ""));
            PostLobby.SetRequestHeader("region", ServerManager.Instance.CurrentRegion.Name);
            PostLobby.SetRequestHeader("player-count", playerCount.ToString());
            PostLobby.SetRequestHeader("max-players", GameManager.Instance.LogicOptions.MaxPlayers.ToString());
            PostLobby.SetRequestHeader("map", GameManager.Instance.LogicOptions.MapId.ToString());
            curInfo.AddCustomHeaders(LobbyUpdateType.Creation).ForEach(kvp => PostLobby.SetRequestHeader(kvp.Key, kvp.Value));

            requests.Add(PostLobby);
        });

        IEnumerator<UnityWebRequest> enumerator = requests.GetEnumerator();
        while (enumerator.MoveNext())
            yield return enumerator.Current.SendWebRequest();
            if (enumerator.Current.result != UnityWebRequest.Result.Success) {
                log.Exception($"Error while posting lobby, returned {enumerator.Current.responseCode}", enumerator.Current.error);
            }
        enumerator.Dispose();
        yield return null;
    }

    internal static void UpdateLobbyStatus(int gameId, int playerCount, LobbyStatus lobbyStatus)
    {
        LobbyServers.ForEach(curInfo => {
            if (curInfo.UpdatePlayerStatusEndpoint() == "") return;
            HttpRequestMessage requestMessage = new();
            requestMessage.RequestUri = new Uri(curInfo.UpdatePlayerStatusEndpoint());
            requestMessage.Method = HttpMethod.Post;
            requestMessage.Headers.Add("game-id", gameId.ToString());
            requestMessage.Headers.Add("status", lobbyStatus.ServerString());
            requestMessage.Headers.Add("player-count", playerCount.ToString());
            curInfo.AddCustomHeaders(LobbyUpdateType.Player).ForEach(kvp => requestMessage.Headers.Add(kvp.Key, kvp.Value));
            Client.SendAsync(requestMessage);
        });
    }

    internal static void UpdateLobbyMap(int gameId, byte mapId)
    {
        LobbyServers.ForEach(curInfo => {
            if (curInfo.UpdateMapEndpoint() == "") return;
            HttpRequestMessage requestMessage = new();
            requestMessage.RequestUri = new Uri(curInfo.UpdateMapEndpoint());
            requestMessage.Method = HttpMethod.Post;
            requestMessage.Headers.Add("game-id", gameId.ToString());
            requestMessage.Headers.Add("map", mapId.ToString());
            curInfo.AddCustomHeaders(LobbyUpdateType.Map).ForEach(kvp => requestMessage.Headers.Add(kvp.Key, kvp.Value));
            Client.SendAsync(requestMessage);  
        });
    }

    internal static void GETModdedLobbies()  {}

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
    private class DefaultServerInfo: ILobbyServerInfo
    {
        private static readonly Dictionary<string, string> Empty = new();

        // Examples. these aren't currently implemented or even being listened for
        // public string CreateEndpoint() => "https://api.lotusapi.top/lobbies/create";
        // public string UpdatePlayerStatusEndpoint() => "https://api.lotusapi.top/lobbies/update";
        // public string UpdateMapEndpoint() => "https://api.lotusapi.top/lobbies/update";
        public string CreateEndpoint() => "";
        public string UpdatePlayerStatusEndpoint() => "";
        public string UpdateMapEndpoint() => "";
        public Dictionary<string, string> AddCustomHeaders(LobbyUpdateType _) => Empty;
    }
}