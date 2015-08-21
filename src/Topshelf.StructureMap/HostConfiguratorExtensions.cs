using StructureMap;
using StructureMap.Configuration.DSL;
using Topshelf.HostConfigurators;

namespace Topshelf.StructureMap
{
	public static class HostConfiguratorExtensions
	{
		public static HostConfigurator UseStructureMap(this HostConfigurator configurator, IContainer container, params Registry[] registries) {
			configurator.AddConfigurator(new StructureMapBuilderConfigurator(container, registries));
			return configurator;
		}

		public static HostConfigurator UseStructureMap(this HostConfigurator configurator, Registry registry) {
			configurator.AddConfigurator(new StructureMapBuilderConfigurator(registry));
			return configurator;
		}
	}
}