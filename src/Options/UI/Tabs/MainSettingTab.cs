using VentLib.Utilities.Extensions;
using VentLib.Options.UI.Options;
using System.Collections.Generic;
using VentLib.Options.Enum;
using System.Linq;
using VentLib.Options.Interfaces;

namespace VentLib.Options.UI.Tabs;

public class MainSettingTab: IMainSettingTab
{
    private List<GameOption> Options { get; }
    public string ButtonText { get; }
    public string AreaDescription { get; }
    public MainSettingTab(string buttonText, string areaDescription)
    {
        ButtonText = buttonText;
        AreaDescription = areaDescription;
        Options = new();
    }

    public virtual void AddOption(GameOption option)
    {
        if (Options.Contains(option)) return;
        Options.Add(option);
    }

    public virtual void RemoveOption(GameOption option) => Options.Remove(option);

    public virtual void ClearOptions() => Options.Clear();

    public virtual List<GameOption> PreRender() => Options.SelectMany(opt => opt.GetDisplayedMembers()).ToList();

    public virtual float StartHeight() => 2.21f; // Vanilla AU starts at 0.713f. 2.21 is like the very top of the options.
    
    public virtual List<GameOption> GetOptions() => Options;

    public virtual void Activate()
    {
        
    }
    public virtual void Deactivate()
    {
        GetOptions().ForEach(child => {
            switch (child.OptionType) {
                case OptionType.String:
                    (child as TextOption)!.HideMembers();
                    break;
                case OptionType.Bool:
                    (child as BoolOption)!.HideMembers();
                    break;
                case OptionType.Int:
                case OptionType.Float:
                    (child as FloatOption)!.HideMembers();
                    break;
                default:
                    (child as UndefinedOption)!.HideMembers();
                    break;
            }
        });
    }
}