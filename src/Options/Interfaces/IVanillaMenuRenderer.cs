using System.Collections.Generic;
using UnityEngine;
using VentLib.Options.UI;

namespace VentLib.Options.Interfaces;

public interface IVanillaMenuRenderer
{
    internal void Render(MonoBehaviour menu, List<GameOption> customOptions, IEnumerable<OptionBehaviour> vanillaOptions, RenderOptions renderOptions);
}