using System.Collections.Generic;

namespace Assessment.Services
{
    public interface ITaskService
    {
        List<Assessment.Task> GetAllTasks();
        Assessment.Task GetTaskById(int id);
        void AddTask(Assessment.Task task);
        void UpdateTask(Assessment.Task task);
        void DeleteTask(int id);
    }
}
