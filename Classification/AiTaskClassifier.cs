using TaskManager.Models;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Microsoft.VisualBasic;


namespace TaskManager.Classification;

public class AiTaskClassifier : ITaskClassifier
{
    
    public TaskClassification Classify(string text)
    {
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://routerai.ru/api/v1")
        };

        var credential = new ApiKeyCredential(Environment.GetEnvironmentVariable("ROUTERAI_API_KEY"));

        ChatClient client = new(
            model: "openai/gpt-4o-mini",
            credential: credential,
            options: options);
            
        var currentDateTime = DateTimeOffset.Now.ToString("O");
        List<ChatMessage> messages = [
            new SystemChatMessage(
                "Ты классификатор задач. Для каждой задачи определи категорию, приоритет и дедлайн.\n" +
                "Категории: Study, Work, Shopping, Personal.\n" +
                "Если ни одна из категорий не подходит, ставь категорию Other.\n" +
                "Приоритеты: Low, Medium, High.\n" +
                $"Текущая дата и время: {currentDateTime}.\n" +
                "Используй это время для определения относительных дедлайнов по типу: сегодня, завтра, послезавтра, через n дней и т.д.\n" +
                "Дедлайн может быть не всегда. Если его нет, ничего не указывай на его месте.\n" +
                "Дедлайн должен быть отдан в формате ДД.ММ.ГГГГ и ничего больше \n" + 
                "Формат ответа: Category, Priority, Deadline"
            ),
            new UserChatMessage(text)
        ];
        ChatCompletion completion = client.CompleteChat(messages);

        string res = completion.Content[0].Text;
        Console.WriteLine("AI RESPONSE:");
        Console.WriteLine(res);
        string[] parts = res.Split(',');
        TaskCategory category = Enum.Parse<TaskCategory>(parts[0].Trim());
        TaskPriority priority = Enum.Parse<TaskPriority>(parts[1].Trim());

        DateTime? deadline = null;

        if (!string.IsNullOrWhiteSpace(parts[2]))
        {
            deadline = DateTime.Parse(parts[2].Trim());
        }

        return new TaskClassification
        {
            Category = category,
            Priority = priority,
            Deadline = deadline
        };
    }
}