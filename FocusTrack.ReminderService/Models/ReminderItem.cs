using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FocusTrack.ReminderService.Models
{
    public class ReminderItem
    {
        public int Id { get; set; }
        public int TaskId { get; set; }  // Link to TaskId
        public string Title { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsReminderSent { get; set; }
    }
}