namespace VentLib.Options.Interfaces;

public interface IMainSettingEvent: IControllerEvent
{
    IMainSettingTab? Source();
}