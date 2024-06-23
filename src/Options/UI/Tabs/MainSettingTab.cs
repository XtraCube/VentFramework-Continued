using System.Collections.Generic;
using System.Linq;
using VentLib.Utilities.Extensions;

namespace VentLib.Options.UI.Tabs;

public class MainSettingTab
{
    private List<GameOption> Options { get; }
    public string buttonText;
    public string areaDescription;
    public MainSettingTab(string buttonText, string areaDescription)
    {
        this.buttonText = buttonText;
        this.areaDescription = areaDescription;
        Options = new();
    }

    public virtual void AddOption(GameOption option)
    {
        if (Options.Contains(option)) return;
        Options.Add(option);
    }

    public virtual void RemoveOption(GameOption option) => Options.Remove(option);

    public virtual void ClearOptions() => Options.Clear();

    public List<GameOption> PreRender() => Options.SelectMany(opt => opt.GetDisplayedMembers()).ToList();

    public virtual float StartHeight() => 2.21f; // Vanilla AU starts around 0.8 i think. 2.21 is like the very top of the options.
    
    public List<GameOption> GetOptions() => Options;
}