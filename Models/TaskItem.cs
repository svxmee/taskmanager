namespace TaskManager.Models; // указывает что этот класс относится к разделу Models

public class TaskItem // общий класс определяющий каждую задачу
{
    public int Id {get; set; }
    public string Text {get; set; }  = string.Empty;
    public TaskCategory Category {get; set;}
    public TaskPriority Priority {get; set;}
    public DateTime? Deadline {get; set;}
    public bool IsCompleted {get; set;}
    public DateTime CreatedAt {get; set;}

}