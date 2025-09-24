using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharuaTaskManager.Models;

namespace SharuaTaskManager.Services
{
    public class TaskService
    {
        private readonly string _dataPath;
        private List<Task> _tasks;

        public TaskService()
        {
            _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SharuaTaskManager", "tasks.json");
            _tasks = LoadTasks();
        }

        public List<Task> GetAllTasks()
        {
            return _tasks.ToList();
        }

        public List<Task> GetTodayTasks()
        {
            var today = DateTime.Today;
            return _tasks.Where(t => !t.IsInBacklog && 
                (t.DueDate.HasValue && t.DueDate.Value.Date == today || t.CreatedAt.Date == today) && 
                !t.IsCompleted).ToList();
        }

        public List<Task> GetBacklogTasks()
        {
            return _tasks.Where(t => t.IsInBacklog && !t.IsCompleted).ToList();
        }

        public List<Task> GetCompletedTasks()
        {
            return _tasks.Where(t => t.IsCompleted).ToList();
        }

        public Dictionary<DateTime, int> GetTaskStats(int days)
        {
            var stats = new Dictionary<DateTime, int>();
            var startDate = DateTime.Today.AddDays(-days);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var completedCount = _tasks.Count(t => t.CompletedAt.HasValue && t.CompletedAt.Value.Date == date);
                stats[date] = completedCount;
            }

            return stats;
        }

        public void AddTask(Task task)
        {
            _tasks.Add(task);
            SaveTasks();
        }

        public void UpdateTask(Task task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                var index = _tasks.IndexOf(existingTask);
                _tasks[index] = task;
                SaveTasks();
            }
        }

        public void DeleteTask(Guid taskId)
        {
            _tasks.RemoveAll(t => t.Id == taskId);
            SaveTasks();
        }

        public void CompleteTask(Guid taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.IsCompleted = true;
                task.CompletedAt = DateTime.Now;
                SaveTasks();
            }
        }

        public void MoveToBacklog(Guid taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.IsInBacklog = true;
                SaveTasks();
            }
        }

        public void MoveFromBacklog(Guid taskId, DateTime? dueDate)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.IsInBacklog = false;
                if (dueDate.HasValue)
                    task.DueDate = dueDate;
                SaveTasks();
            }
        }

        private List<Task> LoadTasks()
        {
            try
            {
                if (File.Exists(_dataPath))
                {
                    var json = File.ReadAllText(_dataPath);
                    return DeserializeTasks(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading tasks: " + ex.Message);
            }

            return new List<Task>();
        }

        private void SaveTasks()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var json = SerializeTasks(_tasks);
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving tasks: " + ex.Message);
            }
        }

        private string SerializeTasks(List<Task> tasks)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                sb.AppendLine("  {");
                sb.AppendLine("    \"Id\": \"" + task.Id + "\",");
                sb.AppendLine("    \"Title\": \"" + EscapeJson(task.Title) + "\",");
                sb.AppendLine("    \"CreatedAt\": \"" + task.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss") + "\",");
                sb.AppendLine("    \"DueDate\": " + (task.DueDate.HasValue ? "\"" + task.DueDate.Value.ToString("yyyy-MM-dd") + "\"" : "null") + ",");
                sb.AppendLine("    \"CompletedAt\": " + (task.CompletedAt.HasValue ? "\"" + task.CompletedAt.Value.ToString("yyyy-MM-ddTHH:mm:ss") + "\"" : "null") + ",");
                sb.AppendLine("    \"IsCompleted\": " + task.IsCompleted.ToString().ToLower() + ",");
                sb.AppendLine("    \"IsInBacklog\": " + task.IsInBacklog.ToString().ToLower());
                sb.AppendLine(i < tasks.Count - 1 ? "  }," : "  }");
            }
            
            sb.AppendLine("]");
            return sb.ToString();
        }

        private List<Task> DeserializeTasks(string json)
        {
            var tasks = new List<Task>();
            if (string.IsNullOrWhiteSpace(json)) return tasks;

            try
            {
                var lines = json.Split('\n');
                Task currentTask = null;
                
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    
                    if (trimmed == "{")
                    {
                        currentTask = new Task();
                    }
                    else if (trimmed == "}," || trimmed == "}")
                    {
                        if (currentTask != null)
                        {
                            tasks.Add(currentTask);
                            currentTask = null;
                        }
                    }
                    else if (currentTask != null && trimmed.Contains(":"))
                    {
                        var parts = trimmed.Split(':');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim().Trim('"');
                            var value = parts[1].Trim().Trim('"', ',');
                            
                            switch (key)
                            {
                                case "Id":
                                    Guid id;
                                    if (Guid.TryParse(value, out id))
                                        currentTask.Id = id;
                                    break;
                                case "Title":
                                    currentTask.Title = UnescapeJson(value);
                                    break;
                                case "CreatedAt":
                                    DateTime createdAt;
                                    if (DateTime.TryParse(value, out createdAt))
                                        currentTask.CreatedAt = createdAt;
                                    break;
                                case "DueDate":
                                    if (value != "null")
                                    {
                                        DateTime dueDate;
                                        if (DateTime.TryParse(value, out dueDate))
                                            currentTask.DueDate = dueDate;
                                    }
                                    break;
                                case "CompletedAt":
                                    if (value != "null")
                                    {
                                        DateTime completedAt;
                                        if (DateTime.TryParse(value, out completedAt))
                                            currentTask.CompletedAt = completedAt;
                                    }
                                    break;
                                case "IsCompleted":
                                    currentTask.IsCompleted = value == "true";
                                    break;
                                case "IsInBacklog":
                                    currentTask.IsInBacklog = value == "true";
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error parsing tasks: " + ex.Message);
            }

            return tasks;
        }

        private string EscapeJson(string value)
        {
            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        private string UnescapeJson(string value)
        {
            return value.Replace("\\\"", "\"")
                       .Replace("\\\\", "\\")
                       .Replace("\\n", "\n")
                       .Replace("\\r", "\r")
                       .Replace("\\t", "\t");
        }
    }
}