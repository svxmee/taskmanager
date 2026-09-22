using TaskManager.Models;

namespace TaskManager.Classification;

public interface ITaskClassifier
{
    TaskClassification Classify(string text);
}