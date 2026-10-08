using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskManager.Tests;

public class TaskApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TaskApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTask_ReturnsTask() // 1
    {
        var client = _factory.CreateClient();

        var json = """
        {
            "text": "сходить в магазин",
            "classifierType": "KeyWord"
        }
        """;

        var content = new StringContent (
            json, 
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("/api/Tasks", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Contains("сходить в магазин", responseBody);
    }

    [Fact]
    public async Task CreateTask_EmptyText()
    {
        var client = _factory.CreateClient();

        var json = """
        {
            "text": "",
            "classifierType": "KeyWord"
        }
        """;

        var content = new StringContent (
            json,
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("/api/Tasks", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTask_ReturnsTask()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Tasks/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"id\":1", responseBody);
    }

    [Fact]
    public async Task GetTask_NotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/Tasks/124443");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    }
    

}