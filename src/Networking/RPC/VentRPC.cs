using System.Linq;
using System.Reflection;
using Hazel;
using VentLib.Logging;
using VentLib.Networking.Helpers;
using VentLib.Networking.Interfaces;
using VentLib.Networking.RPC.Attributes;
using VentLib.Options;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

// ReSharper disable RedundantAssignment

namespace VentLib.Networking.RPC;

/// <summary>
/// A class handling the RPCs of Vent Framework.
/// </summary>
public static class VentRPC
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(VentRPC));
    
    [VentRPC(VentCall.SetControlFlag, RpcActors.Host, RpcActors.NonHosts)]
    public static void SetControlFlag(string assemblyName, int controlFlag)
    {
        log.Trace($"SetControlFlag(assemblyName={assemblyName}, controlFlag={controlFlag})");
        Assembly? assembly = AssemblyUtils.FindAssemblyFromFullName(assemblyName);
        if (assembly == null) return;
        Vents.SetControlFlag(assembly, (VentControlFlag)controlFlag);
    }

    [VentRPC(VentCall.SyncOptions, RpcActors.Host, RpcActors.NonHosts)]
    internal static void SyncOptions(BatchList<NetworkedOption> networkedOptions) => 
        networkedOptions
            .Where(no => no.GetOption() is not NullOption)
            .ForEach(no => no.GetOption().SetValue(no.GetIndex()));

    [VentRPC(VentCall.SyncSingleOption, RpcActors.Host, RpcActors.NonHosts)]
    internal static void SyncSingleOption(NetworkedOption networkedOption)
    {
        Option option = networkedOption.GetOption();
        if (option is NullOption) return;
        option.SetValue(networkedOption.GetIndex());
    }

    internal class NetworkedOption: IRpcSendable<NetworkedOption>
    {
        private Option baseOption;
        private int index;
        
        public NetworkedOption(Option baseOption, int index)
        {
            this.baseOption = baseOption;
            this.index = index;
        }

        public NetworkedOption()
        {
            // for RpcSenable.
        }
        
        public Option GetOption() => baseOption;
        public int GetIndex() => index;
        
        public NetworkedOption Read(MessageReader reader)
        {
            Option option = reader.ReadDynamic(typeof(Option));
            return new NetworkedOption(option, reader.ReadInt32()); 
        }

        public void Write(MessageWriter writer)
        {
            baseOption.Write(writer);
            writer.Write(index);
        }
    }
}

/// <summary>
/// RPCs specifically used for VentFramework.
/// </summary>
public enum VentCall: uint
{
    /// <summary>
    /// Used to share versions among clients.
    /// </summary>
    VersionCheck = 1017,
    
    /// <summary>
    /// Denies or allows RPCs of a client to prevent incompatibility.
    /// </summary>
    SetControlFlag = 1018,
    
    /// <summary>
    /// Syncs Option Managers that have the flag set.<br/>
    /// The players must be on the same version for this to happen.
    /// </summary>
    SyncOptions = 1019,
    
    /// <summary>
    /// Syncs one option. Used in lobby so we don't send every option again.
    /// </summary>
    SyncSingleOption = 1020,
}