using TaskManager.Models;

namespace TaskManager.Data;

public interface ITaskRepository
{
    List<TaskItem> GetTasks(TaskCategory? category, TaskPriority? priority); 
    TaskItem? GetTask(int id); 
    TaskItem CreateTask(TaskItem task); 
    TaskItem? UpdateTask(int id, TaskItem task); 
    bool DeleteTask(int id);
}