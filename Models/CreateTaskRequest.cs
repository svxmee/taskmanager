

namespace TaskManager.Models;

public class CreateTaskRequest // модель для создания запроса на создание задачи
{
    public string Text {get; set; } = string.Empty;
    public ClassifierType? ClassifierType { get; set; }   
}