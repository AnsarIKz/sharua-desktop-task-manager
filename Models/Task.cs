using System;

namespace SharuaTaskManager.Models
{
    public class Task
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsInBacklog { get; set; }
        public string[] Tags { get; set; }

        public Task()
        {
            Id = Guid.NewGuid();
            Title = string.Empty;
            CreatedAt = DateTime.Now;
            Tags = new string[0];
        }
    }
}