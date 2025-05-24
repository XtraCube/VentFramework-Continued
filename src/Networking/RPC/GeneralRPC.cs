using Hazel;
using VentLib.Logging.Default;
using VentLib.Utilities.Extensions;

namespace VentLib.Networking.RPC;

public static class GeneralRPC
{
    public static void SendGameData(int clientId = -1, SendOption sendOption = SendOption.Reliable)
    {
        // MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
        // writer.StartMessage((byte)(clientId == -1 ? 5 : 6)); //0x05 GameData
        // {
        //     writer.Write(AmongUsClient.Instance.GameId);
        //     if (clientId != -1)
        //         writer.WritePacked(clientId);
        //     writer.StartMessage(1); //0x01 Data
        //     {
        //         writer.WritePacked(GameManager.Instance.NetId);
        //         GameManager.Instance.Serialize(writer, true);
        //     }
        //     writer.EndMessage();
        // }
        // writer.EndMessage();

        // AmongUsClient.Instance.SendOrDisconnect(writer);
        // writer.Recycle();
        MessageWriter? writer = null;
        int num = 0;
        
        foreach (NetworkedPlayerInfo playerInfo in GameData.Instance.AllPlayers)
        {
            if (writer == null)
            {
                writer = MessageWriter.Get(sendOption);
                writer.StartMessage((byte)(clientId == -1 ? 5 : 6));
                writer.Write(AmongUsClient.Instance.GameId);

                if (clientId != -1) writer.WritePacked(clientId);
            }
            int length = writer.Length;
            int position = writer.Position;
            writer.StartMessage(1);
            writer.WritePacked(playerInfo.NetId);
            playerInfo.Serialize(writer, false);
            writer.EndMessage();
            if (length > NetworkRules.MaxPacketSize)
            {
                if (num == 0)
                {
                    NoDepLogger.Fatal("1 Player ({0}) exceeded max packet size: {1}".Formatted(playerInfo.Object != null ? playerInfo.Object.name : playerInfo.PlayerName, 
                        NetworkRules.MaxPacketSize));
                }
                else
                {
                    writer.Length = length;
                    writer.Position = position;
                }

                num = 0;
                writer.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(writer);
                writer.Recycle();
                writer = null;
            }
            else num++;
        }

        if (writer == null) return;
        writer.EndMessage();
        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }

    public static void SendMeetingHud(int clientId = -1, bool initialState = true, SendOption sendOption = SendOption.Reliable)
    {
        MessageWriter writer = MessageWriter.Get(sendOption);
        writer.StartMessage((byte)(clientId == -1 ? 5 : 6)); //0x05 GameData
        {
            writer.Write(AmongUsClient.Instance.GameId);
            if (clientId != -1)
                writer.WritePacked(clientId);
            writer.StartMessage(1); //0x01 Data
            {
                writer.WritePacked(MeetingHud.Instance.NetId);
                MeetingHud.Instance.Serialize(writer, initialState);
            }
            writer.EndMessage();
        }
        writer.EndMessage();

        AmongUsClient.Instance.SendOrDisconnect(writer);
        writer.Recycle();
    }
}