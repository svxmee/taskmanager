namespace TaskManager.Models;

public class TaskClassification
{
    public TaskCategory Category {get; set;}
    public TaskPriority Priority {get; set;}
    public DateTime? Deadline {get; set;}
}