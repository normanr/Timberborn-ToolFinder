using System;
using System.Reflection;
using HarmonyLib;
using Timberborn.BaseComponentSystem;

namespace Mods.ToolFinder
{
  public static class ComponentExtensions
  {
    private static readonly Type ManufactorySpecType;
    private static readonly MethodInfo GetComponentFast_ManufactorySpec_Method;

    static ComponentExtensions()
    {
      ManufactorySpecType = Type.GetType("Timberborn.Workshops.ManufactorySpec, Timberborn.Workshops");
      GetComponentFast_ManufactorySpec_Method = AccessTools.Method(typeof(BaseComponent), "GetComponentFast", null, new[] { ManufactorySpecType });
    }

    public static object GetComponentFast_ManufactorySpec(this BaseComponent baseComponent)
    {
      return GetComponentFast_ManufactorySpec_Method.Invoke(baseComponent, null);
    }

  }
}
