using TaskManager.Models;

namespace TaskManager.Data;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<TaskItem> GetTasks()
    {
        return _context.Tasks.ToList();
    }

    public TaskItem? GetTask(int id)
    {
        return _context.Tasks.FirstOrDefault(t => t.Id == id);
    }

    public TaskItem CreateTask(TaskItem task)
    {
        _context.Add(task);
        _context.SaveChanges();
        return task;
    }

    public TaskItem? UpdateTask(int id, TaskItem task)
    {
        var existingTask = _context.Tasks.FirstOrDefault(t => t.Id == id);

        if (existingTask == null)
        {
            return null;
        }

        existingTask.Text = task.Text;
        existingTask.Category = task.Category;
        existingTask.Priority = task.Priority;
        existingTask.Deadline = task.Deadline;
        existingTask.IsCompleted = task.IsCompleted;
        

        _context.SaveChanges();
        return existingTask;
    }

    public bool DeleteTask(int id)
    {
        var existingTask = _context.Tasks.FirstOrDefault(t => t.Id == id);

        if (existingTask == null)
        {
            return false;
        }

        _context.Remove(existingTask);
        _context.SaveChanges();

        return true;
    }
}