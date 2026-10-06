using TaskManager.Models;

namespace TaskManager.Classification;

public interface ITaskClassifierFactory
{
    ITaskClassifier GetTaskClassifier(ClassifierType type);
}