using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VentLib.Logging.Default;
using VentLib.Options.Game.Impl;
using VentLib.Options.Game.Interfaces;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace VentLib.Options.Game.Tabs;

public sealed class VanillaMainTab : VanillaTab
{
    public static VanillaMainTab Instance = null!;
    private static IVanillaMenuRenderer _renderer = new MainMenuRenderer();
    
    // private UnityOptional<SpriteRenderer> highlight = UnityOptional<SpriteRenderer>.Null();
    private UnityOptional<GameOptionsMenu> innerMenu = UnityOptional<GameOptionsMenu>.Null();

    public VanillaMainTab()
    {
        Instance = this;
    }

    public static void SetRenderer(IVanillaMenuRenderer renderer)
    {
        _renderer = renderer;
    }
    
    public override StringOption InitializeOption(StringOption sourceBehavior)
    {
        if (!innerMenu.Exists()) throw new ArgumentException("Cannot Initialize Behaviour because menu does not exist");
        return Object.Instantiate(sourceBehavior, innerMenu.Get().transform);
    }

    public override void Setup(MenuInitialized initialized)
    {
        TabButton = UnityOptional<GameObject>.NonNull(initialized.GameTab);
        RelatedMenu = UnityOptional<GameObject>.NonNull(initialized.GameSettingMenu.GameSettingsTab.gameObject);
        // highlight = UnityOptional<SpriteRenderer>.Of(initialized.GameSettingMenu.GameSettingsHightlight);
        innerMenu = UnityOptional<GameOptionsMenu>.Of(initialized.GameSettingMenu.GameSettingsTab);

        var button = initialized.GameTab.GetComponentInChildren<PassiveButton>();
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        button.OnClick.AddListener((Action)HandleClick);
    }

    public override List<GameOption> PreRender()
    {
        var returnList = new List<GameOption>();
        if (!innerMenu.Exists()) return returnList;

        List<GameOption> options = GetOptions().SelectMany(opt => opt.GetDisplayedMembers()).ToList();
        
        if (options.Count == 0) return returnList;
        var menu = innerMenu.Get();
        
        options.ForEach(opt => GameOptionController.ValidateOptionBehaviour(opt, false));
        _renderer.Render(menu, options, menu.Children.ToArray().Skip(1), GameOptionController.RenderOptions);
        
        return new List<GameOption>();
    }

    // could not find a glpyh or anything for this so we'll just print to console for now...
    protected override void SetGlyphEnabled(bool enabled) => NoDepLogger.Fatal("VanillaManTab.SetGlyphEnabled is being called when it should not be.");
}