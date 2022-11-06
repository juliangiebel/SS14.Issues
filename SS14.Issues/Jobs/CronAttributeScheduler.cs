using System.Reflection;
using Quartz;

namespace SS14.Issues.Jobs;

public static class CronAttributeScheduler
{
    public static async void ScheduleMarkedJobs(ISchedulerFactory schedulerFactory)
    {
        var jobTypes = from type in Assembly.GetExecutingAssembly().GetTypes()
            where type.IsDefined(typeof(CronScheduleAttribute), false)
            select type;

        var scheduler = await schedulerFactory.GetScheduler();
        
        foreach (var jobType in jobTypes)
        {
            var attribute = (CronScheduleAttribute)Attribute.GetCustomAttribute(jobType, typeof(CronScheduleAttribute))!;

            var job = JobBuilder.Create(jobType)
                .WithIdentity(attribute.Name, attribute.Group)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(attribute.Name + "-trigger", attribute.Group)
                .WithCronSchedule(attribute.CronExpression)
                .ForJob(job)
                .Build();
            
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}