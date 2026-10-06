using System.Net.Http.Headers;
using Microsoft.VisualBasic;
using Moq;
using TaskManager.Classification;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Tests;

public class TaskServiceTests
{
    [Fact]
    public void CreateTask_TestClassifier() // 1
    {
        var repository = new Mock<ITaskRepository>();
        var classifier = new Mock<ITaskClassifier>();
        var factory = new Mock<ITaskClassifierFactory>();

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
            factory.Object
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
            factory.Object
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
            factory.Object
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

        repository
            .Setup(x => x.GetTask(1))
            .Returns(new TaskItem
            {
                Id = 1,
                Text = "купить молоко"
            });

        var service = new TaskService(
            repository.Object,
            factory.Object
        );
        var res = service.GetTask(1);

        Assert.NotNull(res);
        Assert.Equal(1, res.Id);
        Assert.Equal("купить молоко", res.Text);
    }
    
    [Fact]
    public void GetTask_FakeTask()
    {
        var repository = new Mock<ITaskRepository>();
        var factory = new Mock<ITaskClassifierFactory>();

        repository
            .Setup(x => x.GetTask(1))
            .Returns(new TaskItem
            {
                Id = 1,
                Text = "купить молоко"
            });

        var service = new TaskService(
            repository.Object,
            factory.Object
        );
        var res = service.GetTask(2);

        Assert.Null(res);
    }
}