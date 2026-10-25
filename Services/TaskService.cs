using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Classification;

namespace TaskManager.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ITaskClassifierFactory _classifier;

    private readonly IConfiguration _configuration;

    private ClassifierType ParseDefaultClassifier()
    {
        var value = _configuration["TaskClassification:DefaultClassifier"];

        if (Enum.TryParse<ClassifierType>(value, ignoreCase: true, out var result) && Enum.IsDefined(result))
        {
            return result;
        }

        throw new InvalidOperationException($"Неизвестный классификатор по умолчанию: {value}");
    }
    public TaskService(ITaskRepository repository, ITaskClassifierFactory classifier, IConfiguration configuration) {

        _repository = repository;
        _classifier = classifier;
        _configuration = configuration;
    }

    public List<TaskItem> GetTasks(TaskCategory? category, TaskPriority? priority)
    {
        return _repository.GetTasks(category, priority);
    }

    public TaskItem? GetTask(int id)
    {
        return _repository.GetTask(id);
    }
    
    public TaskItem CreateTask(TaskItem task, ClassifierType? classifierType)
    {
        if (task == null) throw new ArgumentException(nameof(task));
        if (string.IsNullOrWhiteSpace(task.Text)) throw new ArgumentException("Текст задачи не может быть пустым", nameof(task));

        var selectedType = classifierType ?? ParseDefaultClassifier(); 
        var classifier = _classifier.GetTaskClassifier(selectedType);
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