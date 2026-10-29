using Moq;
using TaskManager.Classification;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;
using Microsoft.Extensions.Configuration;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    [Fact]
    public void CreateTask_TestClassifier() // 1
    {
        var repository = new Mock<ITaskRepository>();
        var classifier = new Mock<ITaskClassifier>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();

        classifier
            .Setup(x => x.Classify("купить молоко"))
            .Returns(new TaskClassification
            {
                Category = TaskCategory.Shopping,
                Priority = TaskPriority.Medium,
                Deadline = null
            });

        factory
            .Setup(x => x.GetTaskClassifier(ClassifierType.KeyWord))
            .Returns(classifier.Object);

        repository
            .Setup(x => x.CreateTask(It.IsAny<TaskItem>()))
            .Returns((TaskItem task) => task);

        var service = new TaskService (
            repository.Object,
            factory.Object,
            configuration.Object
        );

        var task = new TaskItem {
            Text = "купить молоко"
        };

        var res = service.CreateTask (
            task,
            ClassifierType.KeyWord
        );

        Assert.Equal(TaskCategory.Shopping, res.Category);
        Assert.Equal(TaskPriority.Medium, res.Priority);
        Assert.Null(res.Deadline);
    }

    [Fact]
    public void CreateTask_WithDeadline() // 2
    {
        var repository = new Mock<ITaskRepository>();
        var classifier = new Mock<ITaskClassifier>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();

        classifier 
            .Setup(x => x.Classify("завтра сделать отчет"))
            .Returns(new TaskClassification
            {
                Category = TaskCategory.Work,
                Priority = TaskPriority.Medium,
                Deadline = DateTime.Today.AddDays(1)
            });
        factory
            .Setup(x => x.GetTaskClassifier(ClassifierType.KeyWord))
            .Returns(classifier.Object);
        
        repository
            .Setup(x => x.CreateTask(It.IsAny<TaskItem>()))
            .Returns((TaskItem task) => task);

        var service = new TaskService (
            repository.Object,
            factory.Object,
            configuration.Object
        );

        var task = new TaskItem
        {
            Text = "завтра сделать отчет"
        };

        var res = service.CreateTask (
            task,
            ClassifierType.KeyWord
        );

        Assert.Equal(TaskCategory.Work, res.Category);
        Assert.Equal(TaskPriority.Medium, res.Priority);
        Assert.Equal(DateTime.Today.AddDays(1), res.Deadline);
    }

    [Fact]
    public void CreateTask_CheckCreatedAt() // 3
    {
        var repository = new Mock<ITaskRepository>();
        var classifier = new Mock<ITaskClassifier>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();


        classifier 
            .Setup(x => x.Classify("сделать отчет"))
            .Returns(new TaskClassification
            {
                Category = TaskCategory.Work,
                Priority = TaskPriority.Medium,
                Deadline = null
            });
        factory
            .Setup(x => x.GetTaskClassifier(ClassifierType.KeyWord))
            .Returns(classifier.Object);
        
        repository
            .Setup(x => x.CreateTask(It.IsAny<TaskItem>()))
            .Returns((TaskItem task) => task);

        var service = new TaskService (
            repository.Object,
            factory.Object,
            configuration.Object
        );

        var task = new TaskItem
        {
            Text = "сделать отчет"
        };

        var res = service.CreateTask (
            task,
            ClassifierType.KeyWord
        );

        Assert.NotEqual(default, res.CreatedAt);
    }

    [Fact]
    public void GetTask_Check() // 4
    {
        var repository = new Mock<ITaskRepository>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();

        repository
            .Setup(x => x.GetTask(1))
            .Returns(new TaskItem
            {
                Id = 1,
                Text = "купить молоко"
            });

        var service = new TaskService(
            repository.Object,
            factory.Object,
            configuration.Object
        );
        var res = service.GetTask(1);

        Assert.NotNull(res);
        Assert.Equal(1, res.Id);
        Assert.Equal("купить молоко", res.Text);
    }
    
    [Fact]
    public void GetTask_FakeTask() // 5
    {
        var repository = new Mock<ITaskRepository>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();

        repository
            .Setup(x => x.GetTask(1))
            .Returns(new TaskItem
            {
                Id = 1,
                Text = "купить молоко"
            });

        var service = new TaskService(
            repository.Object,
            factory.Object,
            configuration.Object
        );
        var res = service.GetTask(2);

        Assert.Null(res);
    }

    [Fact]
    public void DeleteTask_Check() // 6
    {
        var repository = new Mock<ITaskRepository>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();

        repository
            .Setup(x => x.DeleteTask(1))
            .Returns(true);

        var service = new TaskService(
            repository.Object,
            factory.Object,
            configuration.Object
        );

        var res = service.DeleteTask(1);

        Assert.True(res);
    }

    [Fact]
    public void DeleteTask_NotFound() // 7
    {
        var repository = new Mock<ITaskRepository>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();

        repository
            .Setup(x => x.DeleteTask(9999))
            .Returns(false);

        var service = new TaskService(
            repository.Object,
            factory.Object,
            configuration.Object
        );

        var res = service.DeleteTask(9999);

        Assert.False(res);
    }


    // тесты только классификатора 
    [Fact]
    public void Test_Classify_Preparat()
    {
        var classifier = new KeywordTaskClassifier();

        var res = classifier.Classify("купить препарат");

        Assert.Equal(TaskCategory.Shopping, res.Category);
    }

    [Fact]
    public void Test_Classify_Svodit()
    {
        var classifier = new KeywordTaskClassifier();
        
        var res = classifier.Classify("сводить сына к врачу");

        Assert.Equal(TaskCategory.Personal, res.Category);
    }

    [Theory]
    [InlineData("не срочно купить хлеб")]
    [InlineData("неважно, купить хлеб")]
    [InlineData("необязательно купить хлеб")]

    public void Test_Classify_Nesrochno(string text)
    {
        var classifier = new KeywordTaskClassifier();

        var res = classifier.Classify(text);

        Assert.Equal(TaskPriority.Low, res.Priority);
    }

    [Fact]
    public void Test_Classify_1030()
    {
        var classifier = new KeywordTaskClassifier();

        var exception = Record.Exception(() =>
        {
            classifier.Classify("созвон в 10.30");
        });

        Assert.Contains("10.30", exception.Message);
    }

    [Fact]
    public void Test_Classift_Friday()
    {
        var classifier = new KeywordTaskClassifier();

        var res = classifier.Classify("сделать отчет до пятницы");

        int daysUntilFriday = ((int)DayOfWeek.Friday - (int)DateTime.Today.DayOfWeek + 7) % 7; 

        if (daysUntilFriday == 0) daysUntilFriday = 7;

        var expectedFriday = DateTime.Today.AddDays(daysUntilFriday);

        Assert.Equal(res.Deadline, expectedFriday.Date);
    }

    [Fact]
    public void CreateTask_Empty()
    {
        var repository = new Mock<ITaskRepository>();
        var classifier = new Mock<ITaskClassifier>();
        var factory = new Mock<ITaskClassifierFactory>();
        var configuration = new Mock<IConfiguration>();

        var service = new TaskService (
            repository.Object,
            factory.Object,
            configuration.Object
        );

        var task = new TaskItem
        {
            Text = ""
        };

        Assert.Throws<ArgumentException>(() => service.CreateTask(new TaskItem { Text = ""}, ClassifierType.KeyWord));
    }
}