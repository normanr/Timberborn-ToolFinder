using System;
using System.Globalization;
using Timberborn.BlockObjectTools;
using Timberborn.BlueprintSystem;
using Timberborn.Debugging;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.KeyBindingSystem;
using Timberborn.Localization;
using Timberborn.PlantingUI;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Timberborn.ToolSystemUI;
using Timberborn.Workshops;

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

    bool LabeledEntitySpecMatches(ComponentSpec baseComponent)
    {
      var labeledEntitySpec = baseComponent.GetSpec<LabeledEntitySpec>();
      return NameMatches(_loc.T(labeledEntitySpec.DisplayNameLocKey));
    }

    bool ManufactorySpecMatches(ComponentSpec baseComponent)
    {
      var manufactorySpec = baseComponent.GetSpec<ManufactorySpec>();
      if (manufactorySpec == null)
      {
        return false;
      }
      foreach (var recipeId in manufactorySpec.ProductionRecipeIds)
      {
        var recipe = _recipeSpecService.GetRecipe(recipeId);
        if (NameMatches(_loc.T(recipe.DisplayLocKey)))
        {
          return true;
        }
      }
      return false;
    }

    internal bool ToolMatches(ITool tool)
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
          if (LabeledEntitySpecMatches(blockObjectTool.Template))
          {
            return true;
          }
        }
        catch (NullReferenceException) {
          return true;  // avoid NullReferenceException during fast shutdown if filter is active
        }
        return ManufactorySpecMatches(blockObjectTool.Template);
      }
      var description = (tool as IToolDescriptor)?.DescribeTool();
      if (!string.IsNullOrEmpty(description?.Title))
      {
        return NameMatches(description.Title);
      }
      return true;
    }
  }
}
