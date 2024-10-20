using System.Globalization;
using Timberborn.BlockObjectTools;
using Timberborn.Debugging;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.Localization;
using Timberborn.PlantingUI;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;

namespace Mods.ToolFinder
{
  public class ToolButtonFilter : ILoadableSingleton, IUnloadableSingleton
  {
    public static ToolButtonFilter Singleton { get; private set; }

    private static readonly string FilterToolsKey = "FilterTools";
    private static readonly string FilterToolsLocKey = "FilterTools.Filter";

    private readonly DevModeManager _devModeManager;
    private readonly ILoc _loc;
    private readonly InputBoxShower _inputBoxShower;
    private readonly InputService _inputService;

    private string filterText;

    public ToolButtonFilter(DevModeManager devModeManager, ILoc loc, InputBoxShower inputBoxShower, InputService inputService)
    {
      _devModeManager = devModeManager;
      _loc = loc;
      _inputBoxShower = inputBoxShower;
      _inputService = inputService;
    }

    public void Load()
    {
      Singleton = this;
    }

    public void Unload()
    {
      Singleton = null;
    }

    internal bool ProcessInput()
    {
      if (_inputService.IsKeyDown(FilterToolsKey))
      {
        _inputBoxShower.Create().SetLocalizedMessage(FilterToolsLocKey).SetConfirmButton(SetFilter)
                                .Show();
      }
      if (_inputService.Cancel && filterText != null)
      {
        SetFilter(null);
        return true;
      }
      return false;
    }

    internal void SetFilter(string newFilterText)
    {
      filterText = newFilterText;
      // TODO: Trigger OnDevModeToggledEvent directly to avoid GameAnalytics?
      if (_devModeManager.Enabled)
      {
        _devModeManager.Enable();
      }
      else
      {
        _devModeManager.Disable();
      }
    }

    bool NameMatches(string name)
    {
      return CultureInfo.InstalledUICulture.CompareInfo.IndexOf(name, filterText, CompareOptions.IgnoreCase) >= 0;
    }

    bool LabeledEntitySpecMatches(LabeledEntitySpec labeledEntitySpec)
    {
      return NameMatches(_loc.T(labeledEntitySpec.DisplayNameLocKey));
    }

    internal bool ToolMatches(Tool tool)
    {
      if (string.IsNullOrEmpty(filterText))
      {
        return true;
      }
      if (tool is PlantingTool plantingTool)
      {
        return LabeledEntitySpecMatches(plantingTool.Plantable.GetComponentFast<LabeledEntitySpec>());
      }
      if (tool is BlockObjectTool blockObjectTool)
      {
        return LabeledEntitySpecMatches(blockObjectTool.Prefab.GetComponentFast<LabeledEntitySpec>());
      }
      var description = tool.Description();
      if (!string.IsNullOrEmpty(description?.Title))
      {
        return NameMatches(description.Title);
      }
      return true;
    }
  }
}
