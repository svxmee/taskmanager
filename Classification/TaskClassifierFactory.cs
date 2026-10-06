using TaskManager.Models;

namespace TaskManager.Classification;

public class TaskClassifierFactory : ITaskClassifierFactory
{
    private readonly AiTaskClassifier _aiClassifier;
    private readonly KeywordTaskClassifier _keywordClassifier;

    public TaskClassifierFactory(
        AiTaskClassifier aiClassifier,
        KeywordTaskClassifier keywordClassifier)
    {
        _aiClassifier = aiClassifier;
        _keywordClassifier = keywordClassifier;
    }

    public ITaskClassifier GetTaskClassifier(ClassifierType type)
    {
        if (type == ClassifierType.AI)
        {
            return _aiClassifier;
        }

        if (type == ClassifierType.KeyWord)
        {
            return _keywordClassifier;
        }

        throw new ArgumentException("Неизвестный тип классификатора");
    }
}