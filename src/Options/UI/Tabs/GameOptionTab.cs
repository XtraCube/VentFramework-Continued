using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Interfaces;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace VentLib.Options.UI.Tabs;

public class GameOptionTab : IGameOptionTab
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GameOptionTab));
    private List<GameOption> Options { get; } = new();
    public string name;
    private Func<Sprite> spriteSupplier;

    private UnityOptional<Sprite> sprite = UnityOptional<Sprite>.Null();
    private UnityOptional<GameObject> tabButton = UnityOptional<GameObject>.Null();
    
    private UnityOptional<GameObject> relatedMenu = UnityOptional<GameObject>.Null();
    private UnityOptional<GameOptionsMenu> innerMenu = UnityOptional<GameOptionsMenu>.Null();

    private OrderedSet<Action<IGameOptionTab>> callbacks = new();

    public GameOptionTab(string name, Func<Sprite> spriteSupplier)
    {
        this.name = name;
        this.spriteSupplier = spriteSupplier;
    }

    public void AddEventListener(Action<IGameOptionTab> callback) => callbacks.Add(callback);

    public virtual void AddOption(GameOption option)
    {
        if (Options.Contains(option)) return;
        Options.Add(option);
    }

    public virtual void RemoveOption(GameOption option) => Options.Remove(option);

    public void HandleClick()
    {
        callbacks.ForEach(cb => cb(this));
    }

    public virtual StringOption InitializeOption(StringOption sourceBehavior)
    {
        if (!innerMenu.Exists()) throw new ArgumentException("Cannot Initialize Behaviour because menu does not exist");
        return Object.Instantiate(sourceBehavior, innerMenu.Get().transform);
    }

    public virtual void Setup(RolesSettingsMenu menu)
    {
        
    }

    public void SetPosition(Vector2 position)
    {
        tabButton.IfPresent(btn => btn.transform.localPosition = position);
    }

    public List<GameOption> PreRender() => Options.SelectMany(opt => opt.GetDisplayedMembers()).ToList();

    public Optional<Vector3> GetPosition() => tabButton.Map(btn => btn.transform.localPosition);
    
    public List<GameOption> GetOptions() => Options;

    public void Activate()
    {
        log.Info($"Activated Tab \"{name}\"", "TabSwitch");
        GetTabHighlight().IfPresent(highlight => highlight.enabled = true);
        relatedMenu.IfPresent(menu => menu.SetActive(true));
    }

    public void Deactivate()
    {
        log.Debug($"Deactivated Tab \"{name}\"", "TabSwitch");
        GetTabHighlight().IfPresent(highlight => highlight.enabled = false);
        relatedMenu.IfPresent(menu => menu.SetActive(false));
    }

    public void Show()
    {
        tabButton.IfPresent(button => button.SetActive(true));
    }

    public void Hide()
    {
        tabButton.IfPresent(button => button.SetActive(false));
    }


    private UnityOptional<SpriteRenderer> GetTabRenderer() => tabButton.UnityMap(button => button.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>());
    
    private UnityOptional<SpriteRenderer> GetTabHighlight() => tabButton.UnityMap(button => button.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>());
}