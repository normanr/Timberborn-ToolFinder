using HarmonyLib;
using Timberborn.CursorToolSystem;

namespace Mods.ToolFinder
{
  [HarmonyPatch(typeof(CursorTool), "ProcessShowOptions")]
  static class CursorToolPatcher
  {
    public static bool Prefix()
    {
      if (ToolButtonFilter.Singleton != null)
      {
        return !ToolButtonFilter.Singleton.ProcessInput();
      }
      return true;
    }
  }
}
