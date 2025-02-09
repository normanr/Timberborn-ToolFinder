using System;
using HarmonyLib;
using Timberborn.ModManagerScene;

namespace Mods.ToolFinder {
  public class ModStarter : IModStarter {
    public void StartMod() => throw new NotImplementedException();

    public void StartMod(IModEnvironment modEnvironment)
    {
      var harmony = new Harmony("Tool Finder");
      harmony.PatchAll();
    }
  }
}
