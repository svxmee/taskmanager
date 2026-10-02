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
    public IActionResult CreateTask(TaskItem task)
    {
        var createdTask = _taskService.CreateTask(task);
        return Ok(createdTask);
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
            return NotFound();
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
            return NotFound();
        }

        return NoContent();
    }
}