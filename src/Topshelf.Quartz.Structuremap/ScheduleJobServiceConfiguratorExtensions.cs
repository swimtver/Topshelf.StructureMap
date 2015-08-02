using System;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Topshelf.ServiceConfigurators;

namespace Topshelf.Quartz.StructureMap
{
    public static class ScheduleJobServiceConfiguratorExtensions
    {
        private static readonly Func<IScheduler> DefaultSchedulerFactory = () =>
        {
            var schedulerFactory = new StdSchedulerFactory();
            return schedulerFactory.GetScheduler();
        };

        private static Func<IScheduler> _customSchedulerFactory;
        private static IScheduler _scheduler;
        internal static IJobFactory JobFactory;

        public static Func<IScheduler> SchedulerFactory
        {
            get { return _customSchedulerFactory ?? DefaultSchedulerFactory; }
            set { _customSchedulerFactory = value; }
        }

        private static IScheduler GetScheduler()
        {
            var scheduler = SchedulerFactory();

            if (JobFactory != null)
                scheduler.JobFactory = JobFactory;

            return scheduler;
        }

        public static ServiceConfigurator UsingQuartzJobFactory<TJobFactory>(this ServiceConfigurator configurator, Func<TJobFactory> jobFactory)
            
            where TJobFactory : IJobFactory
        {
            JobFactory = jobFactory();
            return configurator;
        }

        public static ServiceConfigurator UsingQuartzJobFactory<TJobFactory>(this ServiceConfigurator configurator)
            where TJobFactory : IJobFactory, new()
        {
            return UsingQuartzJobFactory(configurator, () => new TJobFactory());
        }

        public static ServiceConfigurator ScheduleQuartzJob(this ServiceConfigurator configurator, Action<QuartzConfigurator> jobConfigurator)
        {
            ConfigureJob(configurator, jobConfigurator);
            return configurator;
        }

        public static ServiceConfigurator<T> ScheduleQuartzJob<T>(this ServiceConfigurator<T> configurator, Action<QuartzConfigurator> jobConfigurator) where T : class
        {
            ConfigureJob(configurator, jobConfigurator);
            return configurator;
        }

        private static void ConfigureJob(ServiceConfigurator configurator, Action<QuartzConfigurator> jobConfigurator)
        {
            var jobConfig = new QuartzConfigurator();
            jobConfigurator(jobConfig);

            if (jobConfig.JobEnabled == null || jobConfig.JobEnabled() || (jobConfig.Job == null || jobConfig.Triggers == null))
            {
                var jobDetail = jobConfig.Job != null ? jobConfig.Job() : null;
                var jobTriggers = (jobConfig.Triggers ?? Enumerable.Empty<Func<ITrigger>>())
                    .Select(triggerFactory => triggerFactory()).Where(trigger => trigger != null).ToArray();

                configurator.BeforeStartingService(() =>
                {
                    _scheduler = _scheduler ?? GetScheduler();
                    if (_scheduler == null || jobDetail == null || !jobTriggers.Any()) return;

                    var triggersForJob = new global::Quartz.Collection.HashSet<ITrigger>(jobTriggers);
                    _scheduler.ScheduleJob(jobDetail, triggersForJob, false);
                    _scheduler.Start();
                });
                configurator.BeforeStoppingService(() => _scheduler.Shutdown(true));
            }
        }
    }
}