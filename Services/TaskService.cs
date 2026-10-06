using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Classification;

namespace TaskManager.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ITaskClassifierFactory _classifier;
    public TaskService(ITaskRepository repository, ITaskClassifierFactory classifier) {

        _repository = repository;
        _classifier = classifier;
    }

    public List<TaskItem> GetTasks(TaskCategory? category, TaskPriority? priority)
    {
        return _repository.GetTasks(category, priority);
    }

    public TaskItem? GetTask(int id)
    {
        return _repository.GetTask(id);
    }
    
    public TaskItem CreateTask(TaskItem task, ClassifierType classifierType)
    {
        var classifier = _classifier.GetTaskClassifier(classifierType);
        var classification = classifier.Classify(task.Text);

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