using VentLib.Options.Interfaces;

namespace VentLib.Options.Events;

public class MainSettingEvent: IMainSettingEvent
{
    private readonly IMainSettingTab? source;

    public MainSettingEvent(IMainSettingTab? tab)
    {
        source = tab;
    }
    
    public IMainSettingTab? Source() => source;
}