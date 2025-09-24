using System;

namespace SharuaTaskManager.Models
{
    public class Task
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsInBacklog { get; set; }
        public TaskPriority Priority { get; set; }
        public string[] Tags { get; set; }

        public Task()
        {
            Id = Guid.NewGuid();
            Title = string.Empty;
            Description = string.Empty;
            CreatedAt = DateTime.Now;
            Priority = TaskPriority.Medium;
            Tags = new string[0];
        }
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
}