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
    public async Task CreateTask_EmptyText() // 2
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
    public async Task GetTask_ReturnsTask() // 3
    {
        var client = _factory.CreateClient();

        var json = """
        {
            "text": "тестовая задача",
            "classifierType": "KeyWord"
        
        }
        """;

        var content = new StringContent(
            json,
            System.Text.Encoding.UTF8,
            "application/json");
        var createResponse = await client.PostAsync("/api/Tasks", content);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var createBody = await createResponse.Content.ReadAsStringAsync();

        using var document = System.Text.Json.JsonDocument.Parse(createBody);
        var id = document.RootElement.GetProperty("id").GetInt32();

        var response = await client.GetAsync($"/api/Tasks/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("тестовая задача", responseBody);
        Assert.Contains($"\"id\":{id}", responseBody);
    }

    [Fact]
    public async Task GetTask_NotFound() // 4
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/Tasks/0");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    }
    

}

