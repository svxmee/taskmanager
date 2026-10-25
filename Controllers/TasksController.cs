using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Controllers;

[ApiController]
[Route("api/[controller]")]


public class TasksController : ControllerBase
{

    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
    [HttpPost]
    public IActionResult CreateTask(CreateTaskRequest request)
    {  
        var task = new TaskItem
        {
            Text = request.Text
        };
        try
        {
            var createdTask = _taskService.CreateTask(task, request.ClassifierType);
            return Ok(createdTask);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new
            {
                error = ex.Message
            });
        }
    }
    
    [HttpGet]
    public IActionResult GetTasks(TaskCategory? category, TaskPriority? priority)
    {
        var tasks = _taskService.GetTasks(category, priority);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public IActionResult GetTask(int id)
    {
        var task = _taskService.GetTask(id);

        if (task == null)
        {
            return NotFound(new
            {
                error = "Задача не найдена"
            });
        }
        return Ok(task);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateTask(int id, TaskItem task) 
    {
        var existingTask = _taskService.UpdateTask(id, task);

        if (existingTask == null)
        {
            return NotFound();
        }

        return Ok(existingTask);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTask(int id)
    {
        var deleted = _taskService.DeleteTask(id);

        if (!deleted)
        {
            return NotFound(new
            {
                error = "Задача не найдена"
            });
        }

        return NoContent();
    }
}