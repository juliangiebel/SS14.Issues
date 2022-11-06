namespace SS14.Issues.Jobs;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CronScheduleAttribute : Attribute
{
    public string CronExpression { get; }
    public string Name { get; }
    public string Group { get; }

    public CronScheduleAttribute(string cronExpression, string name, string group = "default")
    {
        CronExpression = cronExpression;
        Group = group;
        Name = name;
    }
}