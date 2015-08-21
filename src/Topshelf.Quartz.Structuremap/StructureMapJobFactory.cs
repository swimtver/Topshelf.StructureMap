using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Quartz;
using Quartz.Spi;
using StructureMap;

namespace Topshelf.Quartz.StructureMap
{
	public class SimpleJobFactory : IJobFactory
	{
		private readonly IContainer _container;

		public SimpleJobFactory(IContainer container) {
			_container = container;
		}

		public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler) {
			IJob job;
			var jobDetail = bundle.JobDetail;
			var jobType = jobDetail.JobType;
			try {
				job = _container.GetInstance(jobType) as IJob;
			}
			catch (Exception ex) {
				Debug.WriteLine("Exception on Starting job - ", (ex.InnerException ?? ex).Message);
				throw new SchedulerException(string.Format(CultureInfo.InvariantCulture,
					"Problem instantiating class '{0}'", jobDetail.JobType.FullName), ex);
			}
			return job;
		}

		public void ReturnJob(IJob job) {
		}
	}


	public class NestedContainerJobFactory : IJobFactory
	{
		private readonly IContainer _container;
		static readonly ConcurrentDictionary<int, IContainer> Containers = new ConcurrentDictionary<int, IContainer>();

		public NestedContainerJobFactory(IContainer container) {
			_container = container;
		}

		public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler) {
			IJob job;
			var jobDetail = bundle.JobDetail;
			var jobType = jobDetail.JobType;
			try {
				var nestedContainer = _container.GetNestedContainer();
				job = nestedContainer.GetInstance(jobType) as IJob;
				Containers.TryAdd(job.GetHashCode(), nestedContainer);
				Debug.WriteLine("Start job({1}) in thread - {0}. Containers count - {2}",
					Thread.CurrentThread.ManagedThreadId,
					job.GetHashCode(),
					Containers.Count);
			}
			catch (Exception ex) {
				Debug.WriteLine("Exception on Starting job - ", (ex.InnerException ?? ex).Message);
				var cultureInfo = CultureInfo.InvariantCulture;
				object[] fullName = { jobDetail.JobType.FullName };
				throw new SchedulerException(string.Format(cultureInfo, "Problem instantiating class '{0}'", fullName), ex);
			}
			return job;
		}

		public void ReturnJob(IJob job) {
			if (job == null) {
				Debug.WriteLine("Job is null");
				return;
			}

			IContainer container;
			if (Containers.TryRemove(job.GetHashCode(), out container)) {
				if (container == null) { Debug.WriteLine("Container is null"); return; }
				container.Dispose();
			} else {
				Debug.WriteLine("Can't find ({0})", job.GetHashCode());
			}

			Debug.WriteLine("Return job({1}) in thread - {0}. Containers count - {2}",
				Thread.CurrentThread.ManagedThreadId,
				job.GetHashCode(),
				Containers.Count);
		}
	}
}