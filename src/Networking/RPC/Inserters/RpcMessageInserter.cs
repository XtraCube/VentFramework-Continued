using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using VentLib.Networking.Interfaces;
using VentLib.Networking.RPC.Interfaces;

namespace VentLib.Networking.RPC.Inserters;

public class RpcMessageInserter: IRpcInserter<BaseRpcMessage>
{
    public static RpcMessageInserter Instance = null!;

    public RpcMessageInserter()
    {
        Instance = this;
    }

    public void Insert(BaseRpcMessage value, MessageWriter writer)
    {
        value.SerializeRpcValues(writer);
    }
}