using System;
using Topshelf.ServiceConfigurators;

namespace Topshelf.StructureMap
{
	public static class StructureMapServiceConfiguratorExtensions
	{
		public static Func<T> GetFactory<T>() where T : class => () => StructureMapBuilderConfigurator.Container.GetInstance<T>();

		public static ServiceConfigurator<T> ConstructUsingStructureMap<T>(this ServiceConfigurator<T> configurator) where T : class {
			configurator.ConstructUsing(GetFactory<T>());
			return configurator;
		}
	}
}