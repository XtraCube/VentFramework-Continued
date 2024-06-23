using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VentLib.Localization;
using VentLib.Options.Extensions;
using VentLib.Options.Interfaces;
using VentLib.Options.IO;
using VentLib.Options.UI.Options;
using VentLib.Ranges;
using VentLib.Utilities.Optionals;
// ReSharper disable MethodOverloadWithOptionalParameter

namespace VentLib.Options.UI;

public class GameOptionBuilder : IOptionBuilder<GameOptionBuilder>
{
    protected GameOption Option;
    
    public static GameOptionBuilder From(Option option, BuilderFlags flags = BuilderFlags.KeepNone) => (GameOptionBuilder)OptionHelpers.OptionToBuilder(option, typeof(GameOptionBuilder), flags);
    
    public GameOptionBuilder Name(string name)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.name = name;
        return this;
    }

    public GameOptionBuilder Key(string key)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Key = key;
        return this;
    }

    public GameOptionBuilder Description(string description)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Description = Optional<string>.NonNull(description);
        return this;
    }

    public GameOptionBuilder LocaleName(string qualifier)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.name = Localizer.Get(Assembly.GetCallingAssembly()).Translate(qualifier, translationCreationOption: TranslationCreationOption.CreateIfNull);
        Option.Key ??= qualifier;
        return this;
    }

    public GameOptionBuilder IOSettings(Func<IOSettings, IOSettings> settings)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.IOSettings = settings(Option.IOSettings);
        return this;
    }
    
    public GameOptionBuilder IOSettings(Action<IOSettings> settings)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        settings(Option.IOSettings);
        return this;
    }
    

    /// <summary>
    /// Introduces a condition for when to show sub-options. The predicate takes in an object which is the current
    /// value of the option and should return a bool with true indicating the sub options should be shown, and false indicating otherwise.
    /// </summary>
    /// <param name="subOptionPredicate">Predicate to determine if sub options should be shown</param>
    /// <returns></returns>
    public GameOptionBuilder ShowSubOptionPredicate(Func<object, bool> subOptionPredicate)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Children.SetPredicate(subOptionPredicate);
        return this;
    }

    public GameOptionBuilder Values(IEnumerable<object> values)
    {
        if (Option == null) Option = new TextOption();
        Option.Values.AddRange(values.Select(v => new OptionValue(v)));
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.String;
        return this;
    }

    public GameOptionBuilder Values(int defaultIndex, params object[] values)
    {
        if (Option == null) Option = new TextOption();
        Option.DefaultIndex = defaultIndex;
        Option.Values.AddRange(values.Select(v => new OptionValue(v)));
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.String;
        return this;
    }
    
    public GameOptionBuilder Values(int defaultIndex, Type valueType, params object[] values)
    {
        if (Option == null) Option = new TextOption();
        Option.DefaultIndex = defaultIndex;
        Option.Values.AddRange(values.Select(v => new OptionValue(v)));
        Option.ValueType = valueType;
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.String;
        return this;
    }

    public GameOptionBuilder Values(IEnumerable<OptionValue> values)
    {
        if (Option == null) Option = new TextOption();
        Option.Values.AddRange(values);
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.String;
        return this;
    }

    public GameOptionBuilder Value(Func<GameOptionValueBuilder, OptionValue> valueBuilder)
    {
        if (Option == null) Option = new TextOption();
        Option.Values.Add(valueBuilder(new GameOptionValueBuilder()));
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.String;
        return this;
    }

    public GameOptionBuilder Value(object value)
    {
        if (Option == null) Option = new TextOption();
        Option.Values.Add(new OptionValue(value));
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.String;
        return this;
    }

    /// <summary>
    /// Replaces the default + and - with a Checkmark instead. This is not compatible with any other options however.
    /// This will reset the Option's values. Any other methods that set  values will make the option switch from a Checkmark.
    /// </summary>
    /// <param name="defaultValue">Whether or not the checkmarked should be checked by default.</param>
    /// <returns></returns>
    public GameOptionBuilder AddBoolean(bool defaultValue = false)
    {
        if (Option != null) return this;
        Option = new BoolOption() {
            OptionType = Enum.OptionType.Bool,
        };
        return this.Values(defaultValue ? 1 : 0, false, true);
    }

    public GameOptionBuilder AddFloatRange(float start, float stop, float step)
    {
        return AddFloatRange(start, stop, step, 0);
    }

    public GameOptionBuilder AddIntRange(int start, int stop, int step)
    {
        return AddIntRange(start, stop, step, 0);
    }

    /// <summary>
    /// Use the Player name switcher that is used in the HNS gamemode to swap through every player.
    /// This will reset the Option's values. Any other methods that set  values will make the option switch from a Player switcher.
    /// First value is always Round-Robin. (Might be broken. Not tested.)
    /// </summary>
    /// <returns></returns>
    public GameOptionBuilder AddPlayerNames()
    {
        if (!Option.OptionType.CanOverride()) return this;
        Option = new UndefinedOption();
        Option.OptionType = Enum.OptionType.Player;
        return this;
    }

    public GameOptionBuilder Bind(Action<object> changeConsumer)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        BindEvent(ve => changeConsumer(ve.NewValue()));
        return this;
    }

    public GameOptionBuilder BindEvent(Action<IOptionEvent> changeConsumer)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.RegisterEventHandler(changeConsumer);
        return this;
    }
    
    public GameOptionBuilder BindEvent(Action<IOptionValueEvent> changeConsumer)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.RegisterEventHandler(ce =>
        {
            if (ce is IOptionValueEvent ove) changeConsumer(ove);
        });
        return this;
    }

    public GameOptionBuilder BindInt(Action<int> changeConsumer)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        BindEvent(ve => changeConsumer((int)ve.NewValue()));
        return this;
    }

    public GameOptionBuilder BindBool(Action<bool> changeConsumer)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        BindEvent(ve => changeConsumer((bool)ve.NewValue()));
        return this;
    }
    
    public GameOptionBuilder BindFloat(Action<float> changeConsumer)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        BindEvent(ve => changeConsumer((float)ve.NewValue()));
        return this;
    }
    
    public GameOptionBuilder BindString(Action<string> changeConsumer)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        BindEvent(ve => changeConsumer((string)ve.NewValue()));
        return this;
    }

    public GameOptionBuilder SubOption(Func<GameOptionBuilder, Option> subOption)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option sub = subOption(new GameOptionBuilder());
        Option.AddChild(sub);
        return this;
    }

    public GameOptionBuilder Attribute(string key, object value)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Attributes[key] = value;
        return this;
    }

    public void ClearValues()
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Values.Clear();
        Option.OptionType = Enum.OptionType.Undefined;
    }

    public void ClearAttributes()
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Attributes.Clear();
    }

    public GameOptionBuilder Clone()
    {
        return From(Option, BuilderFlags.KeepValues | BuilderFlags.KeepSubOptions | BuilderFlags.KeepSubOptionPredicate | BuilderFlags.KeepAttributes);
    }

    public TR Build<TR>() where TR : Option
    {
        return (TR) (Option) Build();
    }

    public TR BuildAndRegister<TR>(Assembly? assembly = null) where TR : Option
    {
        return (TR) (Option) BuildAndRegister();
    }
    
    public TR BuildAndRegister<TR>(OptionManager manager) where TR : Option
    {
        return (TR) (Option) BuildAndRegister(manager);
    }

    public GameOption Build()
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first before building!");
        return Option;
    }

    public GameOption BuildAndRegister(Assembly? assembly = null, OptionLoadMode loadMode = OptionLoadMode.LoadOrCreate)
    {
        assembly ??= Assembly.GetCallingAssembly();
        Option.Register(assembly, loadMode);
        return Option;
    }
    
    public GameOption BuildAndRegister(OptionManager manager, OptionLoadMode loadMode = OptionLoadMode.LoadOrCreate)
    {
        Option.Register(manager, loadMode);
        return Option;
    }
    

    public GameOptionBuilder Tab(IGameOptionTab tab)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Tab = tab;
        return this;
    }
    
    public GameOptionBuilder Color(Color color)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.Color = color;
        return this;
    }

    public GameOptionBuilder IsHeader(bool isHeader)
    {
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.IsHeader = isHeader;
        return this;
    }

    public GameOptionBuilder IsTitle(bool isTitle)
    {
        if (isTitle) Option = new UndefinedOption() { OptionType = Enum.OptionType.Title };
        if (Option == null) throw new ArgumentNullException("You need to do a ValueSetter function first!");
        Option.IsTitle = isTitle;
        return this;
    }
    
    public GameOptionBuilder AddFloatRange(float start, float stop, float step, int defaultIndex = 0, string suffix = "")
    {
        if (Option == null) Option = new FloatOption();
        var values = new FloatRangeGen(start, stop, step).AsEnumerable().Select(v => new GameOptionValueBuilder().Value(v).Suffix(suffix).Build());
        Option.DefaultIndex = defaultIndex;
        Option.Values.AddRange(values);
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.Float;
        return this;
    }

    public GameOptionBuilder AddIntRange(int start, int stop, int step = 1, int defaultIndex = 0, string suffix = "")
    {
        if (Option == null) Option = new FloatOption();
        var values = new IntRangeGen(start, stop, step).AsEnumerable()
            .Select(v => new GameOptionValueBuilder().Value(v).Suffix(suffix).Build());
        Option.DefaultIndex = defaultIndex;
        Option.Values.AddRange(values);
        if (Option.OptionType.CanOverride()) Option.OptionType = Enum.OptionType.Int;
        return this;
    }
}