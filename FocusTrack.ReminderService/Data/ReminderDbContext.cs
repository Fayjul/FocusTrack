using Microsoft.EntityFrameworkCore;
using FocusTrack.ReminderService.Models;

namespace FocusTrack.ReminderService.Data
{
    public class ReminderDbContext : DbContext
    {
        public ReminderDbContext(DbContextOptions<ReminderDbContext> options) : base(options) { }

        public DbSet<ReminderItem> Reminders { get; set; }
    }
}
