using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Options.Events;
using VentLib.Options.Interfaces;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace VentLib.Options.UI;

public class GameOption : Option
{
    public Color Color = Color.white;
    public bool IsHeader;
    public bool IsTitle;
    
    public IGameOptionTab? Tab
    {
        get => tab;
        set
        {
            tab?.RemoveOption(this);
            tab = value;
            tab?.AddOption(this);
        }
    }

    private IGameOptionTab? tab;

    public int Level => Parent.Map(p => ((GameOption)p).Level).OrElse(0) + 1;

    public string Name(bool colorize = true)
    {
        if (!colorize || Color == Color.white) return base.Name();
        return Color.Colorize(base.Name());
    }

    internal List<GameOption> GetDisplayedMembers()
    {
        return new[] { this }.Concat(Children.GetConditionally(GetValue())
            .Cast<GameOption>()
            .SelectMany(child => child.GetDisplayedMembers()))
            .ToList();
    }
}