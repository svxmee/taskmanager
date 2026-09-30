using System.Text.RegularExpressions;
using TaskManager.Models;

namespace TaskManager.Classification;

public class KeywordTaskClassifier : ITaskClassifier
{
    public TaskClassification Classify(string text)
    {
        string textLower = text.ToLower();
        TaskCategory category;

        string[] studyWords = {
            "лекци", "лаборатор", "учеб", "экзамен", "зачет", "зачёт",
            "курсов", "диплом", "практик", "семинар", "контрольн",
            "домашн", "реферат", "презентац", "конспект", "пара"
        }; // использовал слова не целиком, а варианты без окончаний чтобы не делать много проверок и падежи и склонения все равно замечал фильтр

        string[] workWords = {
            "работ", "отчет", "отчёт", "проект", "совещан",
            "задач", "документ", "клиент", "начальник", "коллег",
            "презентац", "договор", "встреч", "дедлайн", "почт"
        };

        string[] shoppingWords = {
            "куп", "магазин", "продукт", "хлеб", "молок", "мяс",
            "овощ", "фрукт", "сыр", "яйц", "вод", "товар",
            "заказ", "доставк"
        };

        string[] personalWords = {
            "уборк", "помыт", "почин", "квартир", "дом",
            "спорт", "трениров", "зал", "врач", "парикмахер",
            "документ", "паспорт", "звон", "встреч", "отдых",
            "погуля", "друз", "семь"
        };

        if (studyWords.Any(word => textLower.Contains(word)))
        {
            category = TaskCategory.Study;
        }
        
        
        else if (workWords.Any(word => textLower.Contains(word)))
        {
            category = TaskCategory.Work;
        }
        
        else if (shoppingWords.Any(word => textLower.Contains(word)))
        {
            category = TaskCategory.Shopping;
        }

        else if (personalWords.Any(word => textLower.Contains(word)))
        {
            category = TaskCategory.Personal;
        }

        else
        {
            category = TaskCategory.Other;
        }

        TaskPriority priority = TaskPriority.Medium;

        string[] lowWords = {
            "можн", "неважн", "если успе", "по желан", "когда будет врем",
            "не срочн", "необязательн", "потом", "когда-нибудь", "желательн"
        };

        string[] mediumWords = {
            "нужн", "надо", "следует", "сдела", "подготов",
            "занима", "реш", "провер", "законч", "выполн"
        };

        string[] highWords = {
            "срочн", "важн", "немедлен", "сегодня", "сейчас",
            "как можно скоре", "до завтра", "дедлайн", "горит",
            "обязательн", "критич", "нельзя забы"
        };

        if (highWords.Any(word => textLower.Contains(word)))
        {
            priority = TaskPriority.High;
        }

        else if (mediumWords.Any(word => textLower.Contains(word)))
        {
            priority = TaskPriority.Medium;
        }


        else if (lowWords.Any(word => textLower.Contains(word)))
        {
            priority = TaskPriority.Low;
        }

        DateTime? deadline = null;

        if (textLower.Contains("сегодня"))
        {
            deadline = DateTime.Today;
        }

        else if (textLower.Contains("послезавтра"))
        {
            deadline = DateTime.Today.AddDays(2);
        }

        else if (textLower.Contains("завтра"))
        {
            deadline = DateTime.Today.AddDays(1);
        }
        
        else if (textLower.Contains("через неделю"))
        {
            deadline = DateTime.Today.AddDays(7);
        }

        else
        {
            Match match = Regex.Match(textLower, @"через (\d+) дней?"); // используем regex для определения шаблона "через n дней"

            if (match.Success)
            {
                int days = int.Parse(match.Groups[1].Value);
                deadline = DateTime.Today.AddDays(days);
            }
        }

        TaskClassification result = new TaskClassification
        {
            Category = category,
            Priority = priority,
            Deadline = deadline,
        };

        return result;
    }
}