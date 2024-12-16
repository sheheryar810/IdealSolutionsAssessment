using System.Collections.Generic;
using System.Linq;
using Assessment.Data;

namespace Assessment.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public List<Assessment.Task> GetAllTasks() => _context.Tasks.ToList();

        public Assessment.Task GetTaskById(int id) => _context.Tasks.FirstOrDefault(t => t.Id == id);

        public void AddTask(Assessment.Task task)
        {
            _context.Tasks.Add(task);
            _context.SaveChanges();
        }

        public void UpdateTask(Assessment.Task task)
        {
            var existingTask = _context.Tasks.Find(task.Id);
            if (existingTask != null)
            {
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Status = task.Status;
                existingTask.AssignedUserId = task.AssignedUserId;
                _context.SaveChanges();
            }
        }

        public void DeleteTask(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
            }
        }
    }
}
