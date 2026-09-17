using System.ComponentModel.DataAnnotations;

namespace TaskApi.Models;

public class TaskItemRequest
{
    [Required]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
}
