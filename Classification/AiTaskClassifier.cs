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
            Endpoint = new Uri("https://routerai.ru/api/v1") // если использовать другого провайдера, напр. OpenRouter, то вводить другой BaseUrl
        };

        var credential = new ApiKeyCredential(Environment.GetEnvironmentVariable("AI_API_KEY"));

        ChatClient client = new(
            model: "anthropic/claude-haiku-5.5", // для использования другой модели, вставить в кавычки нужную модель
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
                "Если в тексте соддержится любой текст, который похож на бессмыслицу, набор символов или не имеет понятного смысла то верни: INVALID \n" +
                "Используй это время для определения относительных дедлайнов по типу: сегодня, завтра, послезавтра, через n дней и т.д.\n" +
                "Дедлайн указывай только тогда, когда пользователь явно указал дату или срок в тексте задачи.\n" +
                "Если в тексте нет явного указания на дату или срок, Deadline должен быть пустым.\n" +
                "Никогда не придумывай дедлайн самостоятельно и не устанавливай его по смыслу задачи.\n" +
                "Например: \"сходить в магазин\" - Deadline пустой.\n" +
                "Если в задаче нет даты или срока, категорически запрещено указывать текущую или ближайшую дату.\n" +
                "\"купить молоко завтра\" - Deadline = дата завтрашнего дня.\n" +
                "\"сдать отчет до пятницы\" - Deadline = дата ближайшей будущей пятницы.\n" +
                "Под ближайшей будущей пятницей понимай первую пятницу, которая наступит после текущей даты.\n" +
                "Если сегодня пятница, для выражения \"до пятницы\" выбирай пятницу следующей недели.\n" +
                "Обязательно вычисляй дату по календарю, а не прибавляй приблизительное количество дней.\n" +
                "Проверь, что полученная дата действительно соответствует указанному дню недели.\n" +
                "Дедлайн должен быть отдан в формате ДД.ММ.ГГГГ и ничего больше.\n" + 
                "При определении сложных календарных дат сначала точно вычисляй дату по календарю, а не приблизительно.\n" +
                "Для выражений вида \"первый/второй/третий/четвертый день недели следующего месяца\" сначала определи следующий календарный месяц, затем найди все вхождения указанного дня недели в этом месяце и выбери нужное по порядковому номеру.\n" +
                "Например, если сегодня 03.10.2026, то второй четверг следующего месяца — 12.11.2026.\n" +
                "Не заменяй календарное вычисление приблизительным количеством дней от текущей даты.\n" +
                "Верни строго одну строку в формате: Study, High, 13.10.2026\n" +
                "Не добавляй подписи Category:, Priority:, Deadline:, пояснения или Markdown.\n" +
                "Если дедлайна нет, верни пустое третье поле: Shopping, Medium,\n" +
                "Если задача бессмысленная, верни только INVALID."
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
            deadline = DateTime.ParseExact(
                parts[2].Trim(),
                "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        return new TaskClassification
        {
            Category = category,
            Priority = priority,
            Deadline = deadline
        };
    }
}