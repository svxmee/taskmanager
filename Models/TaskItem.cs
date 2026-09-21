namespace TaskManager.Models;

public class TaskItem
{
    public int Id {get; set; }
    public string Text {get; set; }  = string.Empty;
    public TaskCategory Category {get; set;}
    public TaskPriority Priority {get; set;}
    public DateTime? Deadline {get; set;}
    public bool IsCompleted {get; set;}
    public DateTime CreatedAt {get; set;}

}