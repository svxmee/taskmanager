using System.Text.RegularExpressions;
using TaskManager.Models;

namespace TaskManager.Classification;

public class KeywordTaskClassifier : ITaskClassifier
{

    private bool ContainsWord(string text, string word)
    {
        return Regex.IsMatch(
            text,
            $@"\b{Regex.Escape(word)}\w*",
            RegexOptions.IgnoreCase
        );
    }

    private bool ContainsExactWord(string text, string word)
    {
        return Regex.IsMatch(
            text,
            $@"\b{Regex.Escape(word)}\b",
            RegexOptions.IgnoreCase
        );
    }
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

        if (studyWords.Any(word => ContainsWord(textLower, word)))
        {
            category = TaskCategory.Study;
        }
        
        
        else if (workWords.Any(word => ContainsWord(textLower, word)))
        {
            category = TaskCategory.Work;
        }
        
        else if (shoppingWords.Any(word => ContainsWord(textLower, word)))
        {
            category = TaskCategory.Shopping;
        }

        else if (personalWords.Any(word => ContainsWord(textLower, word)))
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

        if (lowWords.Any(word => ContainsWord(textLower, word)))
        {
            priority = TaskPriority.Low;
        }

        else if (mediumWords.Any(word => ContainsWord(textLower, word)))
        {
            priority = TaskPriority.Medium;
        }


        else if (highWords.Any(word => ContainsWord(textLower, word)))
        {
            priority = TaskPriority.High;
        }

        DateTime? deadline = null;

        string[] daysOfWeek =
        {
            "понедельник", "понедельника", "понедельнику", "понедельником", "понедельнике",

            "вторник", "вторника", "вторнику","вторником", "вторнике",

            "среда", "среды", "среде", "среду", "средой",

            "четверг", "четверга", "четвергу", "четвергом", "четверге",

            "пятница", "пятницы", "пятнице", "пятницу", "пятницей",

            "суббота", "субботы", "субботе", "субботу", "субботой",

            "воскресенье", "воскресенья", "воскресенью", "воскресеньем", "воскресенье"
        };

        if (ContainsExactWord(textLower, "сегодня"))
        {
            deadline = DateTime.Today;
        }

        else if (ContainsExactWord(textLower, "послезавтра"))
        {
            deadline = DateTime.Today.AddDays(2);
        }

        else if (ContainsExactWord(textLower, "завтра"))
        {
            deadline = DateTime.Today.AddDays(1);
        }
        
        else if (ContainsExactWord(textLower, "через неделю"))
        {
            deadline = DateTime.Today.AddDays(7);
        }

        else if (daysOfWeek.Any(word => ContainsExactWord(textLower, word)))
        {
            DayOfWeek targetDay;
            
            if (ContainsWord(textLower, "понедельник")) targetDay = DayOfWeek.Monday;
            else if (ContainsWord(textLower, "вторник")) targetDay = DayOfWeek.Tuesday;
            else if (ContainsWord(textLower, "сред")) targetDay = DayOfWeek.Wednesday;
            else if (ContainsWord(textLower, "четверг")) targetDay = DayOfWeek.Thursday;
            else if (ContainsWord(textLower, "пятниц")) targetDay = DayOfWeek.Friday;
            else if (ContainsWord(textLower, "суббот")) targetDay = DayOfWeek.Saturday;
            else if (ContainsWord(textLower, "воскресен")) targetDay = DayOfWeek.Sunday;
            else targetDay = DayOfWeek.Sunday;
            int daysUntilTarget = ((int)targetDay - (int)DateTime.Today.DayOfWeek + 7) % 7; 

            if (daysUntilTarget == 0) daysUntilTarget = 7;
            deadline = DateTime.Today.AddDays(daysUntilTarget);
        }

        else
        {
            Match match = Regex.Match(textLower, @"через\s+(\d+)\s+(?:день|дня|дней)\b"); // используем regex для определения шаблона "через n дней/дня/дней"

            if (match.Success)
            {
                if (!int.TryParse(match.Groups[1].Value, out int days))
                {
                    throw new ArgumentException("Количество дней слишком большое или указано неверно");
                }

                int maxDays = (DateTime.MaxValue.Date - DateTime.Today).Days;
                Console.WriteLine($"maxDays = {maxDays}, days = {days}");
                if (days > maxDays)
                {
                    throw new ArgumentException($"Нельзя указать дедлайн дальше чем через {maxDays} дней");
                }

                deadline = DateTime.Today.AddDays(days);
            }
            else
            {
                Match dateMatch = Regex.Match(textLower, @"\b(\d{1,2})\.(\d{1,2})(?:\.(\d{2}|\d{4}))?\b");

                if (dateMatch.Success)
                {
                    int day = int.Parse(dateMatch.Groups[1].Value);
                    int month = int.Parse(dateMatch.Groups[2].Value);

                    int year;

                    if (dateMatch.Groups[3].Success)
                    {
                        string yearText = dateMatch.Groups[3].Value;
                        year = int.Parse(yearText);

                        if (yearText.Length == 2) year += 2000;
                    }
                    else
                    {
                        year = DateTime.Today.Year;
                    }
                    if (month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
                    {
                        throw new ArgumentException($"Указана несуществующая дата: {dateMatch.Value}");
                    }
                    deadline = new DateTime(year, month, day);
                  
                }
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