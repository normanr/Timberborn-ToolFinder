using HarmonyLib;
using Timberborn.ModManagerScene;

namespace Mods.ToolFinder
{
  public class ModStarter : IModStarter
  {
    public void StartMod()
    {
      StartMod(null);
    }

    public void StartMod(IModEnvironment modEnvironment)
    {
      var harmony = new Harmony("Tool Finder");
      harmony.PatchAll();
    }
  }
}
