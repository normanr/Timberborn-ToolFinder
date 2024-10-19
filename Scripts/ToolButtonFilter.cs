using Timberborn.BlockObjectTools;
using Timberborn.InputSystem;
using Timberborn.PlantingUI;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Timberborn.Debugging;
using Timberborn.CoreUI;

namespace Mods.ToolFinder
{
  public class ToolButtonFilter : ILoadableSingleton, IUnloadableSingleton
  {
    public static ToolButtonFilter Singleton { get; private set; }

    private static readonly string FilterToolsKey = "FilterTools";
    private static readonly string FilterToolsLocKey = "FilterTools.Filter";

    private readonly DevModeManager _devModeManager;
    private readonly InputBoxShower _inputBoxShower;
    private readonly InputService _inputService;

    private string filterText;

    public ToolButtonFilter(DevModeManager devModeManager, InputBoxShower inputBoxShower, InputService inputService)
    {
      _devModeManager = devModeManager;
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
      return name.ToLower().Contains(filterText);
    }

    internal bool ToolMatches(Tool tool)
    {
      if (string.IsNullOrEmpty(filterText))
      {
        return true;
      }
      if (tool is PlantingTool plantingTool)
      {
        return NameMatches(plantingTool.Plantable.PrefabName);
      }
      if (tool is BlockObjectTool blockObjectTool)
      {
        return NameMatches(blockObjectTool.Prefab.name);
      }
      return true;
    }
  }
}
