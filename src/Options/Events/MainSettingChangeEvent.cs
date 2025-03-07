using VentLib.Options.Interfaces;

namespace VentLib.Options.Events;

public class MainSettingChangeEvent: MainSettingEvent
{
    public readonly IMainSettingTab? Original;
    
    public MainSettingChangeEvent(IMainSettingTab? oldTab, IMainSettingTab? newTab) : base(newTab)
    {
        Original = oldTab;
    }
}