using System.Security.Cryptography.X509Certificates;
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
        else
        {
            category = TaskCategory.Other;
        }

        TaskCategory priority;

        string[] lowWords = {""};
        string[] mediumWords = {""};
        string[] highWords = {""};

        if (lowWords.Any(word => textLower.Contains(word)))

        TaskClassification result = new TaskClassification
        {
            Category = TaskCategory.Other,
            Priority = TaskPriority.Medium,
            Deadline = null
        };

        return result;
    }
}