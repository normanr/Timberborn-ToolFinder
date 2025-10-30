using HarmonyLib;
using Timberborn.ToolSystem;

namespace Mods.ToolFinder
{
  [HarmonyPatch(typeof(ToolGroupService), nameof(ToolGroupService.ExitToolGroup))]
  static class ToolGroupManagerPatcher
  {
    static void Prefix()
    {
      if (ToolButtonFilter.Singleton != null)
      {
        ToolButtonFilter.Singleton.SetFilter(null);
      }
    }
  }
}
