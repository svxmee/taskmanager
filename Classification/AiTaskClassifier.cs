using TaskManager.Models;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Microsoft.VisualBasic;
using SQLitePCL;


namespace TaskManager.Classification;

public class AiTaskClassifier : ITaskClassifier
{
    
    public TaskClassification Classify(string text)
    {
        string? baseUrl = Environment.GetEnvironmentVariable("BASE_URL"); 
        if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? uri) || uri.Scheme != Uri.UriSchemeHttps) throw new InvalidOperationException("Base-URL должен содержать корректный Https адрес");

        var options = new OpenAIClientOptions
        {
            Endpoint = uri
        };

        string? apiKey = Environment.GetEnvironmentVariable("AI_API_KEY");
        if(string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("AI-классификатор не может быть создан из-за отсутствия API-ключа");
        
        var credential = new ApiKeyCredential(apiKey);

        string? aiModel = Environment.GetEnvironmentVariable("AI_MODEL");
        if (string.IsNullOrWhiteSpace(aiModel)) throw new InvalidOperationException("AI-модель не выбрана");

        ChatClient client = new(
            model: aiModel,
            credential: credential,
            options: options);
            
        var currentDateTime = DateTimeOffset.Now.ToString("O");
        var currentDayOfWeek = DateTimeOffset.Now.ToString("dddd", new System.Globalization.CultureInfo("ru-RU"));
        List<ChatMessage> messages = [
            new SystemChatMessage(
                "Ты классификатор задач. Определи категорию, приоритет и дедлайн.\n" +
                "Категории: Study, Work, Shopping, Personal, Other.\n" +
                "Study - учёба, экзамены, лабораторные и курсовые работы.\n" +
                "Work - работа, профессиональные обязанности, рабочие проекты и практика.\n" +
                "Shopping - покупка товаров и выбор вещей для приобретения.\n" +
                "Personal - здоровье, спорт, отдых, бытовые дела и личная жизнь.\n" +
                "Other - понятные задачи, не относящиеся к остальным категориям, включая самостоятельные исследования и технические эксперименты без учебного или рабочего контекста.\n" +
                "Выбирай категорию по цели и контексту, а не по отдельным словам. Не придумывай отсутствующий контекст. Например, «изучить устройство радиоприёмника для интереса» - Other, «подготовиться к экзамену» - Study.\n\n" +
                "Приоритет: Low, Medium или High. Оценивай важность и срочность задачи по её содержанию. Не считай задачу High только из-за наличия дедлайна.\n\n" +
                $"Текущие дата и время: {currentDateTime:O}.\n" +
                $"День недели: {currentDayOfWeek}.\n" +
                "Определяй относительные сроки по текущей дате и календарю. Например, «завтра» - следующий календарный день, «до пятницы» - ближайшая будущая пятница; если сегодня пятница, выбирай пятницу следующей недели.\n" +
                "Учитывай календарные месяцы и годы при вычислении сложных дат, включая первый, второй, третий или четвёртый день недели следующего месяца. Проверяй, что дата соответствует указанному дню недели.\n" +
                "Указывай дедлайн только при явном указании даты или срока в задаче. Если срок не указан, оставляй поле пустым. Никогда не придумывай дату.\n\n" +
                "Если текст явно бессмысленный, состоит из случайных символов или не содержит понятной задачи, верни только INVALID. Короткие, но понятные задачи допустимы: например, «купить молоко».\n\n" +
                "Верни строго одну строку: Category, Priority, Deadline.\n" +
                "Категория и приоритет должны соответствовать разрешённым значениям. Дедлайн - в формате ДД.ММ.ГГГГ либо пустое поле.\n" +
                "Не добавляй пояснения, подписи, Markdown или другие строки.\n" +
                "Примеры:\n" +
                "«подготовиться к экзамену завтра» = Study, High, [дата завтра]\n" +
                "«купить молоко» = Shopping, Medium,\n" +
                "«asdfghjkl qwerty 123» = INVALID"
            ),
            new UserChatMessage(text)
        ];
        
        ChatCompletion completion;

        try
        {
            completion = client.CompleteChat(messages);
        }
        catch (ClientResultException ex) when (ex.Status == 401)
        {
            throw new InvalidOperationException("AI-классификатор не смог подключиться к провайдеру. Проверьте API-ключ", ex);
        }

        string? res = completion.Content.Count > 0 ? completion.Content[0].Text : null;
        if (string.IsNullOrWhiteSpace(res)) throw new InvalidOperationException("ИИ вернул пустой ответ");
        res = res.Trim();
        Console.WriteLine($"ответ AI: [{res}]");
        if (res == "INVALID") throw new InvalidOperationException("ИИ не смог классифицировать задачу");
        
        

        string[] parts = res.Split(',', StringSplitOptions.None);

        if (parts.Length != 3) throw new InvalidOperationException("ИИ вернул ответ в неверном формате");

        if (!Enum.TryParse(parts[0].Trim(), true, out TaskCategory category) || !Enum.IsDefined(typeof(TaskCategory), category))
        {
            throw new InvalidOperationException("ИИ вернул неизвестную категорию");
        }

        if (!Enum.TryParse(parts[1].Trim(), true, out TaskPriority priority) || !Enum.IsDefined(typeof(TaskPriority), priority)) 
        {
            throw new InvalidOperationException("ИИ вернул неизвестный приоритет");
        }

        string deadlineText = parts[2].Trim();

        DateTime? deadline = null;

        if (!string.IsNullOrWhiteSpace(deadlineText))
        {
            if (!DateTime.TryParseExact(deadlineText, 
            "dd.MM.yyyy", 
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out DateTime parsedDeadline)) throw new InvalidOperationException("ИИ вернул дедлайн в неправильном формате");

            deadline = parsedDeadline;
        }

        return new TaskClassification
        {
            Category = category,
            Priority = priority,
            Deadline = deadline
        };
    }
}