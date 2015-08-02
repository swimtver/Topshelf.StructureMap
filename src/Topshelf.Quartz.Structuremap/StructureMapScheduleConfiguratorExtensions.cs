using System;
using Quartz;
using Quartz.Spi;
using Topshelf.HostConfigurators;
using Topshelf.ServiceConfigurators;
using Topshelf.StructureMap;
using SimpleStructureMapJobFactory = Topshelf.Quartz.StructureMap.SimpleJobFactory;

namespace Topshelf.Quartz.StructureMap
{
    public static class StructureMapScheduleConfiguratorExtensions
    {
        public static ServiceConfigurator<T> UseQuartzStructureMap<T>(this ServiceConfigurator<T> configurator, bool withNestedContainers = true)
              where T : class
        {
            SetupStructureMap(withNestedContainers);

            return configurator;
        }

        public static ServiceConfigurator UseQuartzStructureMap(this ServiceConfigurator configurator, bool withNestedContainers = true)
        {
            SetupStructureMap(withNestedContainers);
            return configurator;
        }

        public static HostConfigurator UseQuartzStructureMap(this HostConfigurator configurator, bool withNestedContainers = false)
        {
            SetupStructureMap(withNestedContainers);

            return configurator;
        }

        internal static void SetupStructureMap(bool withNestedContainers)
        {
            var container = StructureMapBuilderConfigurator.Container;
            if (container == null)
                throw new Exception("You must call UseStructureMap() to use the Quartz Topshelf Structuremap integration.");

            container.Configure(c =>
                                {
                                    if (withNestedContainers)
                                        c.For<IJobFactory>().Use<NestedContainerJobFactory>();
                                    else
                                        c.For<IJobFactory>().Use<SimpleStructureMapJobFactory>();
                                    
                                    c.For<ISchedulerFactory>().Use<StructureMapSchedulerFactory>();
                                    c.For<IScheduler>().Use(ctx => ctx.GetInstance<ISchedulerFactory>().GetScheduler()).Singleton();
                                });

            ScheduleJobServiceConfiguratorExtensions.SchedulerFactory = () => container.GetInstance<IScheduler>();
        }
    }
}
