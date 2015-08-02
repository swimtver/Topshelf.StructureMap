using Quartz;
using Quartz.Core;
using Quartz.Impl;
using Quartz.Spi;

namespace Topshelf.Quartz.StructureMap
{
    public class StructureMapSchedulerFactory : StdSchedulerFactory
    {
        private readonly IJobFactory _jobFactory;

        public StructureMapSchedulerFactory(IJobFactory jobFactory)
        {
            _jobFactory = jobFactory;
        }

        protected override IScheduler Instantiate(QuartzSchedulerResources resources, QuartzScheduler scheduler)
        {
            scheduler.JobFactory = _jobFactory;
            return base.Instantiate(resources, scheduler);
        }
    }
}