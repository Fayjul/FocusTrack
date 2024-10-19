using Hangfire;
using System;

namespace FocusTrack.ReminderService.Services
{
    public class ReminderScheduler
    {
        public void ScheduleReminder(ReminderItem task)
        {
            // Schedule a job to send a reminder 30 minutes before the deadline
            var reminderTime = task.Deadline.AddMinutes(-30);
            BackgroundJob.Schedule(() => SendReminder(task), reminderTime);
        }

        public void RescheduleReminder(ReminderItem task)
        {
            // Reschedule the reminder by canceling the old one and scheduling a new one
            CancelReminder(task.TaskId);
            ScheduleReminder(task);
        }

        public void CancelReminder(int taskId)
        {
            // Cancel scheduled job
            BackgroundJob.Delete(taskId.ToString());
        }

        public void SendReminder(ReminderItem task)
        {
            // Publish reminder event to RabbitMQ or send email/SMS notifications
            Console.WriteLine($"Reminder sent for task: {task.Title}, Deadline: {task.Deadline}");
            // Optionally, publish a reminder.sent event to RabbitMQ here
        }
    }
}
