using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VentLib.Logging.Default;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;
using VentLib.Utilities.Optionals;
using Encoding = System.Text.Encoding;
using Object = UnityEngine.Object;

namespace VentLib.Options;

[LoadStatic]
public static class PresetManager
{
    internal static Preset CurrentPreset => AllPresets[_currentPreset - 1];

    private 
        static readonly string[] RandomPresetNames =
    [
        "Chaos", "Anarchy", "Mayhem", "Chill", "Vibes", "Preset", "Custom"
    ];

    private static readonly List<Preset> AllPresets;

    private static UnityOptional<GameSettingMenu> _lastInitialized = new();
    private static int _savedPresetIndex = 1;
    private static int _currentPreset = 1;
    private static int _nextPresetID;
        
    private static Option _presetOption;

    static PresetManager()
    {
        OptionManager presetOptions = OptionManager.GetManager(file:"presets.config", managerFlags:OptionManagerFlags.IgnorePreset);
        _presetOption = new OptionBuilder()
            .Key("Preset Index")
            .Description("The index of the Option Preset to use.")
            .Value(_currentPreset)
            .IOSettings(io => io.UnknownValueAction = ADEAnswer.Allow)
            .BindInt(_ => _presetOption?.Manager?.DelaySave(0))
            .Build();
        _presetOption.Register(presetOptions, OptionLoadMode.LoadOrCreate);
        _currentPreset = _presetOption.GetValue<int>();

        AllPresets = new();
        while (true)
        {
            _nextPresetID++;
            string presetPath = Path.Join(OptionManager.OptionPath, $"Preset{_nextPresetID}");
            DirectoryInfo presetDirectory = new(presetPath);
            if (presetDirectory.Exists) AllPresets.Add(new Preset(_nextPresetID));
            else break;
        }
        
        CreatePreset("Default"); // The default preset everyone will start out with.
        CreatePreset("Host"); // Used for when joining to sync over options.
        
        ChangePreset(_currentPreset);
        _savedPresetIndex = _currentPreset;
    }
    
    public static void ChangePreset(string presetName)
    {
        Preset? existingPreset = AllPresets.FirstOrDefault(p => p.Name == presetName);
        if (existingPreset != null)
        {
            _currentPreset = AllPresets.IndexOf(existingPreset) + 1;
            if (CurrentPreset.Name != "Host")
            {
                _savedPresetIndex = _currentPreset;
            }
            FinishChangingPreset();
        }
        else
        {
            NoDepLogger.Warn($"No preset named '{presetName}' exists. Creating a new one instead.");
            CreatePreset(presetName);
            ChangePreset(presetName);
        }
        NoDepLogger.Info($"Changed to Preset {CurrentPreset.Name}");
    }
    public static void ChangePreset(int presetID)
    {
        Preset? existingPreset = AllPresets.FirstOrDefault(p => p.ID == presetID);
        if (existingPreset != null)
        {
            _currentPreset = presetID;
            FinishChangingPreset();
            NoDepLogger.Info($"Changed to Preset {CurrentPreset.Name}. {presetID}");
        }
        else
        {
            NoDepLogger.Warn($"No preset with ID '{presetID}' exists");
        }
    }
    
    public static void DeletePreset(string presetName)
    {
        if (presetName == "Host") throw new InvalidOperationException("Cannot delete the 'Host' preset.");
        Preset? targetPreset = AllPresets.FirstOrDefault(p => p.Name == presetName);
        if (targetPreset == null) return;
        Async.Schedule(targetPreset.Delete, 2f);
        AllPresets.Remove(targetPreset);
        
        AllPresets.ForEach((p, i) => p.ChangeID(i + 1));
        if (_currentPreset >= AllPresets.Count) _currentPreset = AllPresets.Count - 1;
        FinishChangingPreset();
    }
    public static void DeletePreset(int presetID)
    {
        Preset? targetPreset = AllPresets.FirstOrDefault(p => p.ID == presetID);
        if (targetPreset == null) return;
        Async.Schedule(targetPreset.Delete, 2f);
        AllPresets.Remove(targetPreset);
        
        AllPresets.ForEach((p, i) => p.ChangeID(i + 1));
        if (_currentPreset >= AllPresets.Count) _currentPreset = AllPresets.Count - 1;
        FinishChangingPreset();
    }
    
    public static void CreatePreset(string presetName)
    {
        Preset? targetPreset = AllPresets.FirstOrDefault(p => p.Name == presetName);
        if (targetPreset != null)
        {
            NoDepLogger.Warn($"Preset named '{presetName}' already exists! Not creating a new one.");
            return;
        }
        AllPresets.Add(new Preset(_nextPresetID, presetName));
        _nextPresetID++;
    }

    public static void SwitchFromHost()
    {
        if (CurrentPreset.Name != "Host") return;
        _currentPreset = _savedPresetIndex;
        FinishChangingPreset();
    }
    
    internal static void EditSettingsMenu(GameSettingMenu menu)
    {
        _lastInitialized = UnityOptional<GameSettingMenu>.NonNull(menu);
        
        menu.FindChild<Transform>("What Is This?").localPosition -= new Vector3(0, 0.15f, 0);
        
        TextMeshPro curPresetText = menu.FindChild<TextMeshPro>("GameSettingsLabel");
        curPresetText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        curPresetText.transform.localPosition = new Vector3(-2.85f, 1.45f, -3f);
        curPresetText.horizontalAlignment = HorizontalAlignmentOptions.Center;
        curPresetText.text = CurrentPreset.Name;
        
        TextMeshPro presetLabel = Object.Instantiate(curPresetText, menu.transform);
        presetLabel.transform.localPosition = new Vector3(-2.85f, 1.85f, -3f);
        presetLabel.fontSizeMax = 2;
        presetLabel.text = "Preset:";

        PassiveButton buttonTemp = Object.Instantiate(menu.transform.FindChild("CloseButton").GetComponent<PassiveButton>(), menu.transform);
        buttonTemp.activeSprites = null;
        buttonTemp.inactiveSprites = null;
        
        PassiveButton upArrow = Object.Instantiate(buttonTemp, menu.transform);
        upArrow.name = "UpArrow";
        upArrow.transform.localPosition = new Vector3(-4.1f, 1.7f, -25f);
        upArrow.transform.rotation = new(0f, 0f, .7071f, .7071f);
        upArrow.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        {
            SpriteRenderer inactiveSprite = upArrow.FindChild<SpriteRenderer>("Inactive", true);
            inactiveSprite.sprite = AssetLoader.LoadSprite("presets.arrow.png", 200, true);
            inactiveSprite.gameObject.SetActive(true);
            upArrow.OnClick.RemoveAllListeners();
            upArrow.OnMouseOut.RemoveAllListeners();
            upArrow.OnMouseOver.RemoveAllListeners();
            upArrow.OnClick.m_PersistentCalls.Clear();
            upArrow.OnClick.AddListener((Action)(() => CyclePresets(menu, -1)));
            upArrow.OnMouseOver.AddListener((Action)(() => inactiveSprite.color = Color.cyan));
            upArrow.OnMouseOut.AddListener((Action)(() => inactiveSprite.color = Color.white));
        }
        
        PassiveButton downArrow = Object.Instantiate(buttonTemp, menu.transform);
        downArrow.name = "DownArrow";
        downArrow.transform.localPosition = new Vector3(-4.1f, 1.2f, -25f);
        downArrow.transform.rotation = new(0f, 0f, -.7071f, .7071f);
        downArrow.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        {
            SpriteRenderer inactiveSprite = downArrow.FindChild<SpriteRenderer>("Inactive", true);
            inactiveSprite.sprite = AssetLoader.LoadSprite("presets.arrow.png", 200, true);
            inactiveSprite.gameObject.SetActive(true);
            downArrow.OnClick.RemoveAllListeners();
            downArrow.OnMouseOut.RemoveAllListeners();
            downArrow.OnMouseOver.RemoveAllListeners();
            downArrow.OnClick.m_PersistentCalls.Clear();
            downArrow.OnClick.AddListener((Action)(() => CyclePresets(menu, 1)));
            downArrow.OnMouseOver.AddListener((Action)(() => inactiveSprite.color = Color.cyan));
            downArrow.OnMouseOut.AddListener((Action)(() => inactiveSprite.color = Color.white));
        }
        
        PassiveButton trashBin = Object.Instantiate(buttonTemp, menu.transform);
        trashBin.name = "Trash";
        trashBin.transform.localPosition = new Vector3(-4.6f, 1.2f, -25f);
        trashBin.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        {
            SpriteRenderer inactiveSprite = trashBin.FindChild<SpriteRenderer>("Inactive", true);
            inactiveSprite.sprite = AssetLoader.LoadSprite("presets.trash.png", 200, true);
            inactiveSprite.gameObject.SetActive(true);
            trashBin.OnClick.RemoveAllListeners();
            trashBin.OnMouseOut.RemoveAllListeners();
            trashBin.OnMouseOver.RemoveAllListeners();
            trashBin.OnClick.m_PersistentCalls.Clear();
            trashBin.OnClick.AddListener((Action)(() => Async.Execute(AskToDelete())));
            trashBin.OnMouseOver.AddListener((Action)(() => inactiveSprite.color = Color.cyan));
            trashBin.OnMouseOut.AddListener((Action)(() => inactiveSprite.color = Color.white));
        }
        
        PassiveButton plusSymbol = Object.Instantiate(buttonTemp,  menu.transform);
        plusSymbol.name = "AddPreset";
        plusSymbol.transform.localPosition = new Vector3(-4.6f, 1.725f, -25f);
        plusSymbol.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        {
            SpriteRenderer inactiveSprite = plusSymbol.FindChild<SpriteRenderer>("Inactive", true);
            inactiveSprite.sprite = AssetLoader.LoadSprite("presets.plus.png", 200, true);
            inactiveSprite.gameObject.SetActive(true);
            plusSymbol.OnClick.RemoveAllListeners();
            plusSymbol.OnMouseOut.RemoveAllListeners();
            plusSymbol.OnMouseOver.RemoveAllListeners();
            plusSymbol.OnClick.m_PersistentCalls.Clear();
            plusSymbol.OnClick.AddListener((Action)(() => Async.Execute(PropagateCreatePreset())));
            plusSymbol.OnMouseOver.AddListener((Action)(() => inactiveSprite.color = Color.cyan));
            plusSymbol.OnMouseOut.AddListener((Action)(() => inactiveSprite.color = Color.white));
        }
        
        buttonTemp.gameObject.Destroy();
    }

    private static void CyclePresets(GameSettingMenu menu, int offset)
    {
        _currentPreset += offset;
        if (_currentPreset > AllPresets.Count) _currentPreset -= AllPresets.Count; // overflow by subtracting count
        else if (_currentPreset < 1) _currentPreset += AllPresets.Count; // overflow by adding count
        
        _presetOption.SetHardValue(_currentPreset);
        FinishChangingPreset(); 
    }

    private static IEnumerator AskToDelete()
    {
        InfoTextBox infoTextBox = Object.Instantiate(DestroyableSingleton<AccountManager>.Instance.transform.Find("InfoTextBox").GetComponent<InfoTextBox>());
        infoTextBox.name = "DeleteConfirmationBox";
        infoTextBox.FindChild<BoxCollider2D>("Fill").isTrigger = true;
        infoTextBox.gameObject.layer = LayerMask.NameToLayer("UI");
        infoTextBox.gameObject.GetChildren(true).ForEach(go => go.layer = LayerMask.NameToLayer("UI"));
        
        infoTextBox.transform.SetParent(_lastInitialized.Get().transform, true);
        infoTextBox.transform.localPosition = new(0f, 0f, -100f);
        infoTextBox.titleTexxt.text = "Delete Confirmation";

        if (CurrentPreset.Name != "Host" && CurrentPreset.Name != "Default")
        {
            // Async.Schedule(() =>
            // {
            //     // customScreen.SetTwoButtons(); doesnt work for some reason. so we just have our own below
            //     infoTextBox.button2Trans = infoTextBox.button2.transform;
            //
            //     infoTextBox.button2Trans.gameObject.SetActive(true);
            //     infoTextBox.button1Trans.localPosition = new Vector3(2f, infoTextBox.button1Trans.localPosition.y, 0);
            //     infoTextBox.button2Trans.localPosition = new Vector3(-2f, infoTextBox.button2Trans.localPosition.y, 0);
            //     infoTextBox.button1.gameObject.SetActive(true);
            //     infoTextBox.button2.gameObject.SetActive(true);
            // }, .1f);
            infoTextBox.AutoSetOneButton = false;
            infoTextBox.SetTwoButtons();
            infoTextBox.SetText($"Are you sure you want to delete '{CurrentPreset.Name}'?\nThis action cannot be undone.");
            infoTextBox.button1Text.text = "Yes";
            infoTextBox.button2Text.text = "No";
            infoTextBox.button1.OnClick.AddListener((Action)(() =>
            {
                infoTextBox.button1.gameObject.SetActive(false);
                infoTextBox.button2.gameObject.SetActive(false);
                DeletePreset(CurrentPreset.ID);
                infoTextBox.Close();
            }));
            infoTextBox.button2.OnClick.AddListener((Action)infoTextBox.Close);
        }
        else
        {
            infoTextBox.SetOneButton();
            infoTextBox.SetText($"Preset '{CurrentPreset.Name}' cannot be deleted.");
            infoTextBox.button1Text.text = "OK";
            infoTextBox.button1.OnClick = new Button.ButtonClickedEvent();
            infoTextBox.button1.OnClick.AddListener((Action)infoTextBox.Close);
        }
        
        infoTextBox.gameObject.SetActive(true);
        while (infoTextBox.gameObject.activeSelf)
        {
            // yield until closed
            yield return null;
        }
        infoTextBox.gameObject.Destroy();
    }

    private static IEnumerator PropagateCreatePreset()
    {
        EditName createPreset = Object.Instantiate(DestroyableSingleton<AccountManager>.Instance.FindChild<EditName>("EditName", true));
        createPreset.name = "CreatePreset";
        createPreset.FindChild<BoxCollider2D>("Fill", true).isTrigger = true;
        createPreset.gameObject.layer = LayerMask.NameToLayer("UI");
        createPreset.gameObject.GetChildren(true).ForEach(go => go.layer = LayerMask.NameToLayer("UI"));
        
        createPreset.transform.SetParent(_lastInitialized.Get().transform, true);
        createPreset.transform.localPosition = new(0f, 0f, -100f);

        TextMeshPro titleText = createPreset.FindChild<TextMeshPro>("TitleText_TMP");
        titleText.GetComponent<TextTranslatorTMP>().Destroy();
        titleText.text = "Create Preset";

        PassiveButton randomizeButton = createPreset.FindChild<PassiveButton>("RandomizeName", true);
        randomizeButton.OnClick.m_PersistentCalls.Clear();
        randomizeButton.OnClick.RemoveAllListeners();
        randomizeButton.OnClick.AddListener((Action)(() => createPreset.nameText.nameSource.SetText(RandomPresetNames.ToList().GetRandom())));
        randomizeButton.gameObject.SetActive(false);
        
        PassiveButton submitButton = createPreset.FindChild<PassiveButton>("SubmitButton");
        PassiveButton backButton = createPreset.FindChild<PassiveButton>("BackButton");

        createPreset.nameText.nameSource.characterLimit = 12;
        
        submitButton.OnClick = new Button.ButtonClickedEvent();
        submitButton.OnClick.AddListener((Action)(() =>
        {
            string targetName = createPreset.nameText.nameSource.text.Trim();
            if (targetName == string.Empty) return;
            submitButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(false);
            ChangePreset(targetName);
            createPreset.Close();
        }));
        
        backButton.OnClick = new Button.ButtonClickedEvent();
        backButton.OnClick.AddListener((Action)createPreset.Close);
        createPreset.nameText.transform.FindChild("Background").gameObject.SetActive(false);
        Async.Schedule(() =>
        {
            createPreset.nameText.transform.FindChild("Background").gameObject.SetActive(true);
            
            createPreset.nameText.nameSource.text = $"Preset{_nextPresetID}";
            createPreset.nameText.nameSource.SetText($"Preset{_nextPresetID}");
            createPreset.nameText.FindChild<TextMeshPro>("Text_TMP").text = $"Preset{_nextPresetID}";
        }, .1f);

        createPreset.gameObject.SetActive(true);
        while (createPreset.gameObject.activeSelf)
        {
            // yield until closed
            yield return null;
        }
        createPreset.gameObject.Destroy();
    }

    private static void FinishChangingPreset()
    {
        if (!_lastInitialized.Exists())
        {
            NoDepLogger.Warn("Unable to Finalize Preset Change.");
            return;
        }

        GameSettingMenu menu = _lastInitialized.Get();
        menu.FindChild<TextMeshPro>("GameSettingsLabel").text = CurrentPreset.Name;
        OptionManager.OnChangePreset();
    }

    [QuickPrefix(typeof(NameTextBehaviour), nameof(NameTextBehaviour.Start))]
    private static bool NameTextStart(NameTextBehaviour __instance) => __instance.transform.parent.gameObject.name != "CreatePreset";
}

internal class Preset
{
    public string Name { get; private set; }
    public int ID { get; private set; }
    
    
    private DirectoryInfo presetDirectory;
    private FileInfo presetInfo;
    
    public Preset(int presetID, string? presetName = null)
    {
        Name = presetName ?? $"Preset{presetID}";
        ID = presetID;
        
        string presetPath = Path.Join(OptionManager.OptionPath, $"Preset{presetID}");
        presetDirectory = new(presetPath);
        if (!presetDirectory.Exists) presetDirectory.Create();

        presetInfo = presetDirectory.GetFile("config.txt");
        if (!presetInfo.Exists)
        {
            using FileStream fileStream = presetInfo.Open(FileMode.Create);
            fileStream.Write(Encoding.UTF8.GetBytes(Name));
        }
        else
        {
            using StreamReader streamReader = new(presetInfo.Open(FileMode.Open));
            Name = streamReader.ReadToEnd();
        }
    }

    public void ChangeName(string newName)
    {
        Name = newName;
        using FileStream fileStream = presetInfo.Open(FileMode.Create);
        fileStream.Write(Encoding.UTF8.GetBytes(Name));
    }

    public void ChangeID(int newID)
    {
        if (ID == newID) return; // Skip IOException
        ID = newID;
        presetDirectory.MoveTo(Path.Join(OptionManager.OptionPath, $"Preset{ID}"));
        presetInfo = presetDirectory.GetFile("config.txt");
    }

    public void Delete()
    {
        presetDirectory.Delete(true);
    }

    public string FolderName() => presetDirectory.Name;
}