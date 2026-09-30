using TaskManager.Models;

public interface ITaskService
{
    List<TaskItem> GetTasks(); // получить одно
    TaskItem? GetTask(int id); // получить все
    TaskItem CreateTask(TaskItem task); // создать
    TaskItem? UpdateTask(int id, TaskItem task); // пометить как выполненное(обновить статус)
    bool DeleteTask(int id); // и удалить 
}