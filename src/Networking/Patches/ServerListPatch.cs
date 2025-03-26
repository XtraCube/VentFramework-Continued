using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VentLib.Logging.Default;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Harmony.Attributes;

namespace VentLib.Networking.Patches;

static class RegionMenuPatch
{
    private const int MaxColumns = 4;

    private const float ButtonSpacing = 0.6f;
    private const float ButtonSpacingSide = 2.25f;

    // **JUST** this function from https://github.com/EnhancedNetwork/TownofHost-Enhanced/blob/f21362e2c59b8f2c73580a6dd4a8643decd0f5c5/Patches/RegionMenuPatch.cs
    [QuickPostfix(typeof(RegionMenu), nameof(RegionMenu.OnEnable))]
    public static void AdjustButtonPositions_Postfix(RegionMenu __instance)
    {
        List<UiElement> buttons = __instance.controllerSelectable.ToArray().ToList();

        int buttonsPerColumn = 6;
        int columnCount = (buttons.Count + buttonsPerColumn - 1) / buttonsPerColumn;

        while (columnCount > MaxColumns)
        {
            buttonsPerColumn++;
            columnCount = (buttons.Count + buttonsPerColumn - 1) / buttonsPerColumn;
        }

        float totalWidth = (columnCount - 1) * ButtonSpacingSide;
        float totalHeight = (buttonsPerColumn - 1) * ButtonSpacing;

        Vector3 startPosition = new Vector3(-totalWidth / 2, totalHeight / 2, 0f);
        buttons.ForEach((b, i) => b.transform.localPosition = startPosition + new Vector3(i / buttonsPerColumn * ButtonSpacingSide, -(i % buttonsPerColumn) * ButtonSpacing, 0f));
    }
}

static class ServerDropDownPatch
{
    private const int DefServersPerColumn = 6;
    private const int MaxColumns = 6;

    [QuickPrefix(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
    public static void FillServerOptions_Prefix(ServerDropdown __instance)
    {
        // Memory leak by innersloth. Everytime you click the dropdown button, it makes new buttons, and doesn't remove the old ones from the list.
        __instance.controllerSelectable.Clear(); 
    }
    
    [QuickPostfix(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
    public static void AdjustServerPosiions_Postfix(ServerDropdown __instance)
    {
        List<UiElement> buttons = __instance.controllerSelectable.ToArray().ToList();
        
        int buttonsPerColumn = DefServersPerColumn;
        int columnCount = (buttons.Count + buttonsPerColumn - 1) / buttonsPerColumn;

        while (columnCount > MaxColumns)
        {
            buttonsPerColumn++;
            columnCount = (buttons.Count + buttonsPerColumn - 1) / buttonsPerColumn;
        }

        float xAddPerColumn = 3.48f;
        float xSizeAdd = 1.8f;

        __instance.background.transform.localPosition = new Vector3(Mathf.CeilToInt(columnCount / 2) * 1.8f, __instance.initialYPos + -.3f * buttonsPerColumn, 0f);
        __instance.background.size = new Vector2(4.2f + xSizeAdd * columnCount, 1.2f + .6f * buttonsPerColumn);
        
        int indexAtColumn = 1;
        buttons.ForEach((b, i) =>
        {
            b.transform.localPosition = new(Mathf.FloorToInt(i / buttonsPerColumn) * xAddPerColumn, __instance.y_posButton + -0.55f * (indexAtColumn - 1), -1f);
            indexAtColumn += 1;
            if (indexAtColumn > buttonsPerColumn) indexAtColumn = 1;
        });
    }
}