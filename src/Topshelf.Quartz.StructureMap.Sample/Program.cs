#define SERVICE_CONTROL

using System;
using System.Threading;
using Quartz;
using StructureMap;
using Topshelf.StructureMap;

namespace Topshelf.Quartz.StructureMap.Sample
{
	class Program
	{
		static void Main() {
			HostFactory.Run(c => {
				var container = new Container(cfg => {
					cfg.For<IDependency>().Use<Dependency>().AlwaysUnique();
					cfg.For<IScopeDependency>().Use<ScopeDependency>();
				});
				c.UseStructureMap(container);
#if SERVICE_CONTROL

				c.Service<SampleServiceControl>(
					 StructureMapServiceConfiguratorExtensions.GetFactory<SampleServiceControl>()
					 ,
					 s => {
						 //s.ConstructUsingStructureMap();
						 s.UseQuartzStructureMap()
							.ScheduleQuartzJob(q =>
									q.WithJob(() =>
										JobBuilder.Create<SampleJob>().Build())
										.AddTrigger(() =>
											TriggerBuilder.Create()
												.WithSimpleSchedule(builder => builder.WithIntervalInSeconds(1).WithRepeatCount(25)).Build())
									);
					 });
#else
				c.Service<SampleService>(
					 s => {
						 //Construct topshelf service instance with StructureMap
						 s.ConstructUsingStructureMap();

						 s.WhenStarted((service, control) => service.Start());
						 s.WhenStopped((service, control) => service.Stop());

						 //Construct IJob instance with StructureMap
						 s.UseQuartzStructureMap()
						  .ScheduleQuartzJob(q =>
								q.WithJob(() =>
									JobBuilder.Create<SampleJob>().Build())
									.AddTrigger(() =>
										TriggerBuilder.Create()
											.WithSimpleSchedule(builder => builder.WithIntervalInSeconds(1).WithRepeatCount(25)).Build())
								);
					 });
#endif
			});
		}
	}

	internal class SampleJob : IJob
	{
		private readonly IDependency _first;
		private readonly IDependency _second;

		public SampleJob(IDependency first, IDependency second) {
			_first = first;
			_second = second;
		}

		public void Execute(IJobExecutionContext context) {
			Console.WriteLine("{0} - Sample job executing...", DateTime.Now);
			Console.WriteLine("Dependencies are {0} equal.", _first.Equals(_second) ? "" : "not");
			Thread.Sleep(2000);
			Console.WriteLine("First dependency is acting.");
			_first.Write();
			Console.WriteLine("Second dependency is acting.");
			_second.Write();
			Console.WriteLine("Sample job executed.");
		}
	}


	public interface IScopeDependency
	{
		Guid Id { get; }
	}

	public class ScopeDependency : IScopeDependency, IDisposable
	{
		private readonly Guid _corelationGuid;

		public ScopeDependency() {
			_corelationGuid = Guid.NewGuid();
		}

		public Guid Id {
			get { return _corelationGuid; }
		}

		public void Dispose() {
			Console.WriteLine("----------------------------------");
			Console.WriteLine("{0} was disposed.", _corelationGuid);
			Console.WriteLine("----------------------------------");
		}
	}

	public interface IDependency
	{
		void Write();
	}

	public class Dependency : IDependency
	{
		private readonly IScopeDependency _dependency;

		public Dependency(IScopeDependency dependency) {
			_dependency = dependency;
		}

		public void Write() {

			Console.WriteLine("Line from dependency. ScopeId = {0}", _dependency.Id);
		}
	}

	internal class SampleService
	{
		private readonly IDependency _dependency;

		public SampleService(IDependency dependency) {
			_dependency = dependency;
		}

		public bool Start() {
			Console.WriteLine("--------------------------------");
			Console.WriteLine("Sample Service Started...");
			_dependency.Write();
			Console.WriteLine("--------------------------------");

			return true;
		}

		public bool Stop() {
			return true;
		}
	}

	internal class SampleServiceControl : ServiceControl
	{
		private readonly IDependency _dependency;

		public SampleServiceControl(IDependency dependency) {
			_dependency = dependency;
		}

		public bool Start(HostControl hostControl) {
			Console.WriteLine("--------------------------------");
			Console.WriteLine("Sample Service Started...");
			Console.WriteLine("--------------------------------");

			return true;
		}

		public bool Stop(HostControl hostControl) {
			return true;
		}
	}
}