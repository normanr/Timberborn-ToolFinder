using System;
using System.Globalization;
using HarmonyLib;
using Timberborn.BlockObjectTools;
using Timberborn.Debugging;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.KeyBindingSystem;
using Timberborn.Localization;
using Timberborn.PlantingUI;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Timberborn.Common;
using Timberborn.Workshops;
using Timberborn.BaseComponentSystem;

namespace Mods.ToolFinder
{
  public class ToolButtonFilter : ILoadableSingleton, IUnloadableSingleton
  {
    public static ToolButtonFilter Singleton { get; private set; }

    private static readonly string FilterToolsKey = "FilterTools";
    private static readonly string FilterToolsLocKey = "FilterTools.Filter";

    private readonly EventBus _eventBus;
    private readonly ILoc _loc;
    private readonly InputBoxShower _inputBoxShower;
    private readonly InputService _inputService;
    private readonly KeyBindingRegistry _keyBindingRegistry;
    private readonly RecipeSpecService _recipeSpecService;

    private string filterText;

    public ToolButtonFilter(
      EventBus eventBus,
      ILoc loc,
      InputBoxShower inputBoxShower,
      InputService inputService,
      KeyBindingRegistry keyBindingRegistry,
      RecipeSpecService recipeSpecService)
    {
      _eventBus = eventBus;
      _loc = loc;
      _inputBoxShower = inputBoxShower;
      _inputService = inputService;
      _keyBindingRegistry = keyBindingRegistry;
      _recipeSpecService = recipeSpecService;
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
      if (_keyBindingRegistry.IsUpAfterShortHeld(FilterToolsKey))
      {
        _inputBoxShower.Create().SetLocalizedMessage(FilterToolsLocKey).SetConfirmButton(SetFilter)
                                .Show();
      }
      if (_inputService.Cancel && !string.IsNullOrEmpty(filterText))
      {
        SetFilter(null);
        return true;
      }
      return false;
    }

    internal void SetFilter(string newFilterText)
    {
      filterText = newFilterText;
      // TODO: Invoke ToggleDisplayStyle for ToolGroupButton and ToolButton VisualElements directly?
      _eventBus.Post(new DevModeToggledEvent(enabled: false));
    }

    bool NameMatches(string name)
    {
      return CultureInfo.InstalledUICulture.CompareInfo.IndexOf(name, filterText, CompareOptions.IgnoreCase) >= 0;
    }

    bool LabeledEntitySpecMatches(BaseComponent baseComponent)
    {
      var labeledEntitySpec = baseComponent.GetComponentFast<LabeledEntitySpec>();
      return NameMatches(_loc.T(labeledEntitySpec.DisplayNameLocKey));
    }

    bool ManufactorySpecMatches(BaseComponent baseComponent)
    {
      var manufactorySpec = baseComponent.GetComponentFast<ManufactorySpec>();
      if (manufactorySpec == null)
      {
        return false;
      }
      var productionRecipeIds = Traverse.Create(manufactorySpec).Property("ProductionRecipeIds").GetValue<ReadOnlyList<string>>();
      foreach (var recipeId in productionRecipeIds)
      {
        var recipe = _recipeSpecService.GetRecipe(recipeId);
        if (NameMatches(_loc.T(recipe.DisplayLocKey)))
        {
          return true;
        }
      }
      return false;
    }

    internal bool ToolMatches(Tool tool)
    {
      if (string.IsNullOrEmpty(filterText))
      {
        return true;
      }
      if (tool is PlantingTool plantingTool)
      {
        return LabeledEntitySpecMatches(plantingTool.PlantableSpec);
      }
      if (tool is BlockObjectTool blockObjectTool)
      {
        try
        {
          if (LabeledEntitySpecMatches(blockObjectTool.Prefab))
          {
            return true;
          }
        }
        catch (NullReferenceException) {
          return true;  // avoid NullReferenceException during fast shutdown if filter is active
        }
        return ManufactorySpecMatches(blockObjectTool.Prefab);
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
