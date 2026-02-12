namespace EduOnline.IntegrationTest;

[AttributeUsage(AttributeTargets.Method)]
public class TestPriorityAttribute(int priority) : Attribute
{
    public int Priority { get; } = priority;
}
