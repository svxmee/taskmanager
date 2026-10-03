

namespace TaskManager.Models;

public class CreateTaskRequest
{
    public string Text {get; set; } = string.Empty;
    public ClassifierType ClassifierType { get; set; }   
}