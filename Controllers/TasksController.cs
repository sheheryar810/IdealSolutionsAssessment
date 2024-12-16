using Assessment.Data;
using Assessment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Task = Assessment.Task;

[Route("api/[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(AppDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAllTasks()
    {
        _logger.LogInformation("GetAllTasks called by user: {Username}", User.Identity.Name);
        return Ok(_context.Tasks.ToList());
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return NotFound();
        }

        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = int.Parse(User.FindFirstValue("UserId"));

        _logger.LogDebug("GetTask called by user: {UserId} with role: {Role}", userId, role);

        if (role == "Admin" || task.AssignedUserId == userId)
        {
            _logger.LogInformation("Task {TaskId} returned for user: {UserId}", id, userId);
            return Ok(task);
        }

        _logger.LogWarning("User {UserId} not authorized to access task {TaskId}", userId, id);
        return Forbid();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateTask([FromBody] Task task)
    {
        if (task == null)
        {
            _logger.LogWarning("Attempted to create a null task");
            return BadRequest("Task cannot be null");
        }

        _context.Tasks.Add(task);
        _context.SaveChanges();
        _logger.LogInformation("Task {TaskId} created successfully", task.Id);

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    [Authorize]
    public IActionResult UpdateTask(int id, [FromBody] Task updatedTask)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return NotFound();
        }

        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = int.Parse(User.FindFirstValue("UserId"));

        _logger.LogDebug("UpdateTask called by user: {UserId} with role: {Role}", userId, role);

        if (role == "Admin")
        {
            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Status = updatedTask.Status;
            task.AssignedUserId = updatedTask.AssignedUserId;
            _logger.LogInformation("Task {TaskId} updated by Admin", id);
        }
        else if (role == "User" && task.AssignedUserId == userId)
        {
            task.Status = updatedTask.Status;
            _logger.LogInformation("Task {TaskId} status updated by user: {UserId}", id, userId);
        }
        else
        {
            _logger.LogWarning("User {UserId} not authorized to update task {TaskId}", userId, id);
            return Forbid();
        }

        _context.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return NotFound();
        }

        // Only Admin can delete tasks
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = int.Parse(User.FindFirstValue("UserId"));

        if (role != "Admin")
        {
            _logger.LogWarning("User {UserId} is not authorized to delete task {TaskId}", userId, id);
            return Forbid();
        }

        _context.Tasks.Remove(task);
        _context.SaveChanges();
        _logger.LogInformation("Task {TaskId} deleted by Admin", id);

        return NoContent();
    }
}
