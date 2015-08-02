using System;
using System.Collections.Generic;
using StructureMap;
using StructureMap.Configuration.DSL;
using Topshelf.Builders;
using Topshelf.Configurators;
using Topshelf.HostConfigurators;

namespace Topshelf.StructureMap
{
    public class StructureMapBuilderConfigurator : HostBuilderConfigurator
    {
        private static IContainer _container;

        public static IContainer Container
        {
            get { return _container; }
        }

        public StructureMapBuilderConfigurator(Registry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            _container = new Container(registry);
        }

        public StructureMapBuilderConfigurator(IContainer container, params Registry[] registries)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (registries != null && registries.Length > 0)
                container.Configure(x =>
                                    {
                                        foreach (var registry in registries)
                                            x.AddRegistry(registry);
                                    });
            _container = container;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            yield break;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            return builder;
        }
    }
}