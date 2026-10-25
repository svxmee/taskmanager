using TaskManager.Models;

public interface ITaskService
{
    List<TaskItem> GetTasks(TaskCategory? category, TaskPriority? priority); // получить одно
    TaskItem? GetTask(int id); // получить все
    TaskItem CreateTask(TaskItem task, ClassifierType? classifierType); // создать
    TaskItem? UpdateTask(int id, TaskItem task); // пометить как выполненное(обновить статус)
    bool DeleteTask(int id); // и удалить 
}