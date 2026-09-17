using Microsoft.AspNetCore.Mvc;
using TaskApi.Models;

namespace TaskApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static readonly List<TaskItem> Tasks = new();
    private static readonly object Sync = new();
    private static int _nextId = 1;

    [HttpGet]
    [ProducesResponseType(typeof(List<TaskItem>), StatusCodes.Status200OK)]
    public ActionResult<List<TaskItem>> GetAll()
    {
        lock (Sync)
        {
            return Ok(Tasks.Select(Copy).ToList());
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TaskItem> GetById(int id)
    {
        lock (Sync)
        {
            var task = Tasks.Find(item => item.Id == id);
            return task is null ? NotFound() : Ok(Copy(task));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TaskItem> Create([FromBody] TaskItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title не должен быть пустым.");
        }

        TaskItem task;
        lock (Sync)
        {
            task = new TaskItem
            {
                Id = _nextId++,
                Title = request.Title.Trim(),
                Description = request.Description ?? string.Empty,
                IsCompleted = request.IsCompleted
            };
            Tasks.Add(task);
        }

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, Copy(task));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TaskItem> Update(int id, [FromBody] TaskItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title не должен быть пустым.");
        }

        lock (Sync)
        {
            var task = Tasks.Find(item => item.Id == id);
            if (task is null)
            {
                return NotFound();
            }

            task.Title = request.Title.Trim();
            task.Description = request.Description ?? string.Empty;
            task.IsCompleted = request.IsCompleted;

            return Ok(Copy(task));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        lock (Sync)
        {
            var task = Tasks.Find(item => item.Id == id);
            if (task is null)
            {
                return NotFound();
            }

            Tasks.Remove(task);
            return NoContent();
        }
    }

    private static TaskItem Copy(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        IsCompleted = task.IsCompleted
    };
}
