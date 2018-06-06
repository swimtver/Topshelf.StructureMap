using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Topshelf.ServiceConfigurators;

namespace Topshelf.Quartz.StructureMap
{
    public static class ScheduleJobServiceConfiguratorExtensions
    {
        private static readonly Func<Task<IScheduler>> DefaultSchedulerFactory = () => new StdSchedulerFactory().GetScheduler();

        private static Func<Task<IScheduler>> _customSchedulerFactory;
        private static IScheduler _scheduler;
        internal static IJobFactory JobFactory;

        public static Func<Task<IScheduler>> SchedulerFactory
        {
            get => _customSchedulerFactory ?? DefaultSchedulerFactory;
            set => _customSchedulerFactory = value;
        }

        private static IScheduler GetScheduler()
        {
            var scheduler = SchedulerFactory().GetAwaiter().GetResult();

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

            if (
                jobConfig.JobEnabled == null ||
                jobConfig.JobEnabled() ||
                jobConfig.Job == null ||
                jobConfig.Triggers == null)
            {
                var jobDetail = jobConfig.Job?.Invoke();
                IList<ITrigger> jobTriggers = new List<ITrigger>();
                if (jobConfig.Triggers != null)
                {
                    for (var i = 0; i < jobConfig.Triggers.Count; i++)
                    {
                        var trigger = jobConfig.Triggers[i]?.Invoke();
                        if (trigger != null)
                            jobTriggers.Add(trigger);
                    }
                }

                configurator.BeforeStartingService(() =>
                {
                    _scheduler = _scheduler ?? GetScheduler();
                    if (_scheduler == null || jobDetail == null || jobTriggers.Count == 0)
                        return;

                    IReadOnlyCollection<ITrigger> triggersForJob = new ReadOnlyCollection<ITrigger>(jobTriggers);
                    _scheduler.ScheduleJob(jobDetail, triggersForJob, false);
                    _scheduler.Start();
                });
                configurator.BeforeStoppingService(() => _scheduler.Shutdown(true));
            }
        }
    }
}