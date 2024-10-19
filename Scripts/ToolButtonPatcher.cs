using HarmonyLib;
using Timberborn.ToolSystem;

namespace Mods.ToolFinder
{
  [HarmonyPatch(typeof(ToolButton), nameof(ToolButton.ToolEnabled), MethodType.Getter)]
  static class ToolButtonPatcher
  {
    static void Postfix(ToolButton __instance, ref bool __result)
    {
      if (__result && ToolButtonFilter.Singleton != null)
      {
        __result = ToolButtonFilter.Singleton.ToolMatches(__instance.Tool);
      }
    }
  }
}
