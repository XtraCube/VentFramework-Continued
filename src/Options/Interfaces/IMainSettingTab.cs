using VentLib.Utilities.Extensions;
using VentLib.Options.UI.Options;
using System.Collections.Generic;
using VentLib.Options.Enum;
using System.Linq;
using VentLib.Options.Interfaces;
using VentLib.Options.UI;

namespace VentLib.Options.Interfaces;

/// <summary>
/// Interface for a Main Setting Tab.
/// </summary>
public interface IMainSettingTab
{
    /// <summary>
    /// Gets the text for the button on Laptop.
    /// </summary>
    string ButtonText { get; }
    
    /// <summary>
    /// Text for the "What Is This?" display.
    /// </summary>
    string AreaDescription { get; }
    
    /// <summary>
    /// Adds an option to this tab.
    /// </summary>
    /// <param name="option">The option to add.</param>
    void AddOption(GameOption option);
    
    /// <summary>
    /// Removes an option from this tab.
    /// </summary>
    /// <param name="option">The option to remove.</param>
    void RemoveOption(GameOption option);

    /// <summary>
    /// Removes all options.
    /// </summary>
    void ClearOptions();

    /// <summary>
    /// Gets all options of this tab.
    /// </summary>
    /// <returns>A list of game options.</returns>
    List<GameOption> GetOptions();
    
    /// <summary>
    /// The options to display in the laptop.
    /// </summary>
    /// <returns>A list of game options.</returns>
    List<GameOption> PreRender();

    /// <summary>
    /// The height to start displaying settings at.
    /// </summary>
    /// <returns>A Y position.</returns>
    float StartHeight();

    /// <summary>
    /// Ran when the tab is changed to the Main Tab.
    /// </summary>
    void Activate();

    /// <summary>
    /// Ran when the tab is removed from the Main Tab.
    /// </summary>
    void Deactivate();
}