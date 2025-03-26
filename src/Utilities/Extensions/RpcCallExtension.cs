namespace VentLib.Utilities.Extensions;

/// <summary>
/// A class holding extensions for the <see cref="RpcCalls"/> enum.
/// </summary>
public static class RpcCallExtension
{
    /// <summary>
    /// Returns the name of the specific <see cref="RpcCalls"/>
    /// </summary>
    /// <param name="rpcCalls">The <see cref="RpcCalls"/> you want the name of.</param>
    /// <returns></returns>
    public static string Name(this RpcCalls rpcCalls)
    {
        if ((byte)rpcCalls == 100) return "Mass";
        return rpcCalls switch
        {
            RpcCalls.PlayAnimation => "PlayAnimation",
            RpcCalls.CompleteTask => "CompleteTask",
            RpcCalls.SyncSettings => "SyncSettings",
            RpcCalls.SetInfected => "SetInfected",
            RpcCalls.Exiled => "Exiled",
            RpcCalls.CheckName => "CheckName",
            RpcCalls.SetName => "SetName",
            RpcCalls.CheckColor => "CheckColor",
            RpcCalls.SetColor => "SetColor",
            RpcCalls.SetHat_Deprecated => "SetHat_Deprecated",
            RpcCalls.SetSkin_Deprecated => "SetSkin_Deprecated",
            RpcCalls.ReportDeadBody => "ReportDeadBody",
            RpcCalls.MurderPlayer => "MurderPlayer",
            RpcCalls.SendChat => "SendChat",
            RpcCalls.StartMeeting => "StartMeeting",
            RpcCalls.SetScanner => "SetScanner",
            RpcCalls.SendChatNote => "SendChatNote",
            RpcCalls.SetPet_Deprecated => "SetPet_Deprecated",
            RpcCalls.SetStartCounter => "SetStartCounter",
            RpcCalls.EnterVent => "EnterVent",
            RpcCalls.ExitVent => "ExitVent",
            RpcCalls.SnapTo => "SnapTo",
            RpcCalls.CloseMeeting => "CloseMeeting",
            RpcCalls.VotingComplete => "VotingComplete",
            RpcCalls.CastVote => "CastVote",
            RpcCalls.ClearVote => "ClearVote",
            RpcCalls.AddVote => "AddVote",
            RpcCalls.CloseDoorsOfType => "CloseDoorsOfType",
            // RpcCalls.RepairSystem => "RepairSystem",
            RpcCalls.SetTasks => "SetTasks",
            RpcCalls.ClimbLadder => "ClimbLadder",
            RpcCalls.UsePlatform => "UsePlatform",
            RpcCalls.SendQuickChat => "SendQuickChat",
            RpcCalls.BootFromVent => "BootFromVent",
            RpcCalls.UpdateSystem => "UpdateSystem",
            RpcCalls.SetVisor_Deprecated => "SetVisor_Deprecated",
            RpcCalls.SetNamePlate_Deprecated => "SetNamePlate_Deprecated",
            RpcCalls.SetLevel => "SetLevel",
            RpcCalls.SetHatStr => "SetHatStr",
            RpcCalls.SetSkinStr => "SetSkinStr",
            RpcCalls.SetPetStr => "SetPetStr",
            RpcCalls.SetVisorStr => "SetVisorStr",
            RpcCalls.SetNamePlateStr => "SetNamePlateStr",
            RpcCalls.SetRole => "SetRole",
            RpcCalls.ProtectPlayer => "ProtectPlayer",
            RpcCalls.Shapeshift => "Shapeshift",
            RpcCalls.CheckMurder => "CheckMurder",
            RpcCalls.CheckProtect => "CheckProtect",
            RpcCalls.Pet => "Pet",
            RpcCalls.CancelPet => "CancelPet",
            RpcCalls.UseZipline => "UseZipline",
            RpcCalls.TriggerSpores => "TriggerSpores",
            RpcCalls.CheckSpore => "CheckSpore",
            RpcCalls.CheckShapeshift => "CheckShapeshift",
            RpcCalls.RejectShapeshift => "RejectShapeshift",
            // skip to 60
            RpcCalls.LobbyTimeExpiring => "LobbyTimeExpiring",
            RpcCalls.ExtendLobbyTimer => "ExtendLobbyTimer",
            RpcCalls.CheckVanish => "CheckVanish",
            RpcCalls.StartVanish => "StartVanish",
            RpcCalls.CheckAppear => "CheckAppear",
            RpcCalls.StartAppear => "StartAppear",
            _ => rpcCalls.ToString()
        };
    }
}