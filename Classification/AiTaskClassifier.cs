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
        var currentDayOfWeek = DateTimeOffset.Now.ToString("dddd", new System.Globalization.CultureInfo("ru-RU"));
        List<ChatMessage> messages = [
            new SystemChatMessage(
                "Ты классификатор задач. Для каждой задачи определи категорию, приоритет и дедлайн.\n" +
                "Категории: Study, Work, Shopping, Personal.\n" +
                "Если ни одна из категорий не подходит, ставь категорию Other.\n" +
                "Приоритеты: Low, Medium, High.\n" +
                $"Текущая дата и время: {currentDateTime}.\n" +
                $"Сегодня день недели: {currentDayOfWeek}.\n" +
                "Используй это время для определения относительных дедлайнов по типу: сегодня, завтра, послезавтра, через n дней и т.д.\n" +
                "Дедлайн указывай только тогда, когда пользователь явно указал дату или срок в тексте задачи.\n" +
                "Если в тексте нет явного указания на дату или срок, Deadline должен быть пустым.\n" +
                "Никогда не придумывай дедлайн самостоятельно и не устанавливай его по смыслу задачи.\n" +
                "Например: \"сходить в магазин\" - Deadline пустой.\n" +
                "Если в задаче нет даты или срока, категорически запрещено указывать текущую или ближайшую дату.\n" +
                "\"купить молоко завтра\" - Deadline = дата завтрашнего дня.\n" +
                "\"сдать отчет до пятницы\" - Deadline = дата ближайшей пятницы.\n" +
                "Дедлайн должен быть отдан в формате ДД.ММ.ГГГГ и ничего больше.\n" + 
                "При определении сложных календарных дат сначала точно вычисляй дату по календарю, а не приблизительно.\n" +
                "Для выражений вида \"первый/второй/третий/четвертый день недели следующего месяца\" сначала определи следующий календарный месяц, затем найди все вхождения указанного дня недели в этом месяце и выбери нужное по порядковому номеру.\n" +
                "Например, если сегодня 03.10.2026, то второй четверг следующего месяца — 12.11.2026.\n" +
                "Не заменяй календарное вычисление приблизительным количеством дней от текущей даты.\n" +
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