using Bindito.Core;

namespace Mods.ToolFinder
{
  [Context("Game")]
  internal class ModConfigurator : IConfigurator
  {
    public virtual void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<ToolButtonFilter>().AsSingleton();
    }
  }
}
