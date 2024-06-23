using System;
using UnityEngine.UI;
using VentLib.Options.Enum;
using VentLib.Options.Events;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI.Options;
public class TextOption: GameOption
{
    internal UnityOptional<StringOption> Behaviour = new();

    internal void HideMembers()
    {
        Behaviour.IfPresent(behaviour => behaviour.gameObject.SetActive(false));
        Children.ForEach(child => {
            switch (child.OptionType) {
                case OptionType.String:
                    (child as TextOption).HideMembers();
                    break;
                case OptionType.Bool:
                    (child as BoolOption).HideMembers();
                    break;
                case OptionType.Int:
                case OptionType.Float:
                    (child as FloatOption).HideMembers();
                    break;
                case OptionType.Player:
                    break;
                default:
                    (child as UndefinedOption).HideMembers();
                    break;
            }
        });
    }

    public void Increment()
    {
        Optional<object> oldValue = Value.Map(v => v.Value);

        Behaviour.IfPresent(b => {
            b.Value = b.oldValue = SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
            b.ValueText.text = GetValueText();
        });
        Behaviour.IfNotPresent(() => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
        });

        object newValue = GetValue();

        OptionValueIncrementEvent incrementEvent = new(this, oldValue, newValue);
        EventHandlers.ForEach(eh => eh(incrementEvent));
    }

    public void Decrement()
    {
        Optional<object> oldValue = Value.Map(v => v.Value);
        
        Behaviour.IfPresent(b => {
            b.Value = b.oldValue = SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) - 1, true), false);
            b.ValueText.text = GetValueText();
        });
        Behaviour.IfNotPresent(() => {
            SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);
        });
        
        object newValue = GetValue();
        
        OptionValueDecrementEvent decrementEvent = new(this, oldValue, newValue);
        EventHandlers.ForEach(eh => eh(decrementEvent));
    }

    internal void BindPlusMinusButtons()
    {
        Behaviour.IfPresent(b => {
            PassiveButton plusButton = b.transform.FindChild("PlusButton").GetComponent<PassiveButton>();
            PassiveButton minusButton = b.transform.FindChild("MinusButton").GetComponent<PassiveButton>();

            plusButton.OnClick = new Button.ButtonClickedEvent();
            plusButton.OnClick.AddListener((Action)Increment);

            minusButton.OnClick = new Button.ButtonClickedEvent();
            minusButton.OnClick.AddListener((Action)Decrement);
        });
    }
}