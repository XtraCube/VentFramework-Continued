using System;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Options.Enum;
using VentLib.Options.Events;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI.Options;
public class BoolOption: GameOption
{
    internal UnityOptional<ToggleOption> Behaviour = new();

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

        SetValue(EnforceIndexConstraint(Index.OrElse(DefaultIndex) + 1, true), false);

        object newValue = GetValue();
        Behaviour.IfPresent(b => b.CheckMark.enabled = (bool)newValue);

        OptionValueIncrementEvent incrementEvent = new(this, oldValue, newValue);
        EventHandlers.ForEach(eh => eh(incrementEvent));
    }

    public void Decrement() => Increment();

    internal void BindPlusMinusButtons()
    {
        Behaviour.IfPresent(b => {
            PassiveButton button = b.gameObject.transform.FindChild("Toggle").GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnMouseOut = new Button.ButtonClickedEvent();
            button.OnMouseOver = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((Action)Increment);
            SpriteRenderer activeSprite = button.gameObject.transform.FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>();
            button.OnMouseOut.AddListener((Action)(() => activeSprite.color = Color.white));
            button.OnMouseOver.AddListener((Action)(() => activeSprite.color = Color.cyan));
        });
    }
}