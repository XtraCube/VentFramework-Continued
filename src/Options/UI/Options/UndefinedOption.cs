using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Options.Enum;
using VentLib.Options.Events;
using VentLib.Options.Interfaces;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI.Options;
public class UndefinedOption: GameOption, IGameOptionInstance
{
    // This class should only be used for CategoryHeaders.
    internal UnityOptional<PlayerOption> Behaviour = UnityOptional<PlayerOption>.Null();
    internal UnityOptional<CategoryHeaderMasked> Header = UnityOptional<CategoryHeaderMasked>.Null();

    internal void HideMembers()
    {
        Behaviour.IfPresent(behaviour => behaviour.gameObject.SetActive(false));
        Header.IfPresent(header => header.gameObject.SetActive(false));
        Children.ForEach(child => {
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
                case OptionType.Player:
                    (child as UndefinedOption)!.HideMembers();
                    break;
                default:
                    (child as UndefinedOption)!.HideMembers();
                    break;
            }
        });
    }

    public void Increment()
    {
        
    }

    public void Decrement()
    {
        
    }
    
    public void UpdateOption()
    {
        
    }

    internal void BindPlusMinusButtons()
    {
        
    }

    public static UndefinedOption From(GameOption option)
    {
        UndefinedOption undefinedOption = new UndefinedOption() {
            name = option.name,
            key = option.key,
            Description = option.Description,
            IOSettings = option.IOSettings,
            Values = option.Values,
            DefaultIndex = option.DefaultIndex,
            Attributes = option.Attributes,
        };
        if (option.valueType != null) undefinedOption.valueType = option.valueType;
        option.EventHandlers.ForEach(undefinedOption.RegisterEventHandler);
        option.Children.ForEach(undefinedOption.Children.Add);
        return undefinedOption;
    }
}