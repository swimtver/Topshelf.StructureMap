#Getting Started
You can download packages on NuGet Gallery:
* [Topshelf.StructureMap](http://www.nuget.org/packages/Topshelf.StructureMap/)
* [Topshelf.Quartz.StructureMap](https://www.nuget.org/packages/Topshelf.Quartz.StructureMap/)

#Example
``` c#
class Program
{
    static void Main()
    {
        HostFactory.Run(c =>
        {
            var container = new Container(cfg =>
                                            {
                                                cfg.For<IDependency>().Use<Dependency>();
                                            });
            // Init StructureMap container 
            c.UseStructureMap(container);

            c.Service<SampleService>(s =>
            {
                //Construct topshelf service instance with StructureMap
                s.ConstructUsingStructureMap();

                s.WhenStarted((service, control) => service.Start());
                s.WhenStopped((service, control) => service.Stop());
                
                //Construct IJob instance with StructureMap
                s.UseQuartzStructureMap();

                s.ScheduleQuartzJob(q =>
                    q.WithJob(() =>
                        JobBuilder.Create<SampleJob>().Build())
                        .AddTrigger(() =>
                            TriggerBuilder.Create()
                                .WithSimpleSchedule(builder => builder
                                                                .WithIntervalInSeconds(5)
                                                                .RepeatForever())
                                                                .Build())
                    );
            });
        });
    }
}
```
``` c#
public interface IDependency
{
    void Write();
}

public class Dependency : IDependency
{
    public void Write()
    {
        Console.WriteLine("Line from Service dependency");
    }
}
```
``` c#
internal class SampleService
{
    private readonly IDependency _dependency;
    public SampleService(IDependency dependency)
    {
        _dependency = dependency;
    }

    public bool Start()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Sample Service Started...");
        _dependency.Write();
        Console.WriteLine("--------------------------------");
        return true;
    }

    public bool Stop()
    {
        return true;
    }
}
```
``` c#
internal class SampleJob:IJob
{
    private readonly IDependency _dependency;

    public SampleJob(IDependency dependency)
    {
        _dependency = dependency;
    }

    public void Execute(IJobExecutionContext context)
    {
        Console.WriteLine("{0} - Sample job executing...", DateTime.Now);
        _dependency.Write();
        Console.WriteLine("Sample job executed.");
    }
}
```
