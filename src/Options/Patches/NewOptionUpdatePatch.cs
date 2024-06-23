using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Options.Extensions;
using Rewired.Utils;
using VentLib.Options.UI.Controllers;
using VentLib.Options.UI;

namespace VentLib.Options.Patches;

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class GameSettingsStartPatch
{
    public static void Prefix(GameSettingMenu __instance)
    {
        // Unlocks map/impostor amount changing in online (for testing on your custom servers)
        // Changed to be able to change the map in online mode without having to re-establish the room.
        __instance.GameSettingsTab.HideForOnline = new Il2CppReferenceArray<Transform>(0);
    }
    public static void Postfix(GameSettingMenu __instance)
    {
        SettingsOptionController.Start(__instance);
    }
}

[HarmonyPriority(Priority.Low)]
[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
public static class NewOptionUpdatePatch
{
    public static void Postfix(GameOptionsMenu __instance)
    {
        SettingsOptionController.DoRender(__instance);
    }
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
public static class TabChangePatch
{
    public static void Prefix(ref int tabNum)
    {
        if (!SettingsOptionController.Enabled) return;
        // Skip Game Preset Tab
        if (tabNum == 0)
            tabNum = 1;    
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Initialize))]
public static class GameOptionsMenuStartPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(GameOptionsMenuStartPatch));
    public static float? InnerslothHeight = null;
    public static void Postfix(GameOptionsMenu __instance)
    {
        log.Info($"(Initialize) Mod Settings Opened: {SettingsOptionController.ModSettingsOpened}");
        if (!SettingsOptionController.ModSettingsOpened) 
        {
            if (InnerslothHeight != null) {
                __instance.scrollBar.SetYBoundsMax((byte)InnerslothHeight);
            }
            return;
        }
        if (InnerslothHeight == null) {
            InnerslothHeight = __instance.scrollBar.ContentYBounds.max;
        }
        OptionExtensions.categoryHeaders.RemoveAll(header => {
            if (header.IsNullOrDestroyed()) {
                return true;
            } else {{
                header.gameObject.SetActive(header.name == "ModdedCategory"); 
                return false;
            }}
        });
        if (__instance.Children != null) {
            __instance.Children.ToArray().ForEach(child => {
                child.gameObject.SetActive(child.name == "ModdedSetting"); 
            });
        }
        __instance.MapPicker.gameObject.SetActive(false);
        __instance.scrollBar.GetComponentsInChildren<UiElement>().ForEach(ui => __instance.ControllerSelectable.Add(ui));
		__instance.scrollBar.SetYBoundsMax(-SettingsOptionController.OptionRenderer.GetHeight() - 1.65f);
    }
}


[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
public static class GameOptionsMenuCreateSettingsPatch
{
    public static void Prefix(GameOptionsMenu __instance) => OptionExtensions.categoryHeaders = new();
}