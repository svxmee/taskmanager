using TaskManager.Models;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManager.Classification;

public class TaskClassifierFactory : ITaskClassifierFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TaskClassifierFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ITaskClassifier GetTaskClassifier(ClassifierType type)
    {
        string key = type switch
        {
            ClassifierType.AI => "AI",
            ClassifierType.KeyWord => "Keyword",
            _ => throw new ArgumentException("Неизвестный тип классификатора")
        };

        return _serviceProvider.GetRequiredKeyedService<ITaskClassifier>(key);
    }
}