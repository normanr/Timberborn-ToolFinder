using HarmonyLib;
using Timberborn.ToolSystem;

namespace Mods.ToolFinder
{
  [HarmonyPatch(typeof(ToolGroupManager), nameof(ToolGroupManager.CloseToolGroup))]
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
