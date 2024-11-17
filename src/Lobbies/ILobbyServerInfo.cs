using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VentLib.Lobbies;

public interface ILobbyServerInfo
{
    /// <summary>
    /// The endpoint to send a request when a lobby is made.
    /// </summary>
    /// <returns>The link to send a request to.</returns>
    string CreateEndpoint();
    /// <summary>
    /// The endpoint to update playercount and current lobby status.
    /// </summary>
    /// <returns>The link to send a request to.</returns>
    string UpdatePlayerStatusEndpoint();
    /// <summary>
    /// The endpoint to send the new map.
    /// </summary>
    /// <returns>The link to send a request to.</returns>
    string UpdateMapEndpoint();
    /// <summary>
    /// A key-value dictionary to denote names and values to send with your endpoint alongside the default information.
    /// </summary>
    /// <returns>A key,value dictionary. (Key: Header Name, Value: Header Value)</returns>
    Dictionary<string, string> AddCustomHeaders(LobbyUpdateType updateType);
}

public enum LobbyUpdateType
{
    Creation,
    Player,
    Map
}