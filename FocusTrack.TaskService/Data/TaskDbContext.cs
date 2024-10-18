using FocusTrack.TaskService.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusTrack.TaskService.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
    }
}