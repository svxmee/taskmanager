namespace TaskManager.Models;

public class TaskClassification // определяем категорию приоритет и дедлайн для каждой задачи
{
    public TaskCategory Category {get; set;}
    public TaskPriority Priority {get; set;}
    public DateTime? Deadline {get; set;}
}