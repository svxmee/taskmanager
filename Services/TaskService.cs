using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Classification;

namespace TaskManager.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ITaskClassifier _classifier;
    public TaskService(ITaskRepository repository, ITaskClassifier classifier) {

        _repository = repository;
        _classifier = classifier;
    }

    public List<TaskItem> GetTasks()
    {
        return _repository.GetTasks();
    }

    public TaskItem? GetTask(int id)
    {
        return _repository.GetTask(id);
    }
    
    public TaskItem CreateTask(TaskItem task)
    {
        var classification = _classifier.Classify(task.Text);

        task.Category = classification.Category;
        task.Priority = classification.Priority;
        task.Deadline = classification.Deadline;
        task.CreatedAt = DateTime.Now;
        return _repository.CreateTask(task);
    }

    public TaskItem? UpdateTask(int id, TaskItem task)
    {
        return _repository.UpdateTask(id, task);
    }

    public bool DeleteTask(int id)
    {
        return _repository.DeleteTask(id);
    }
}