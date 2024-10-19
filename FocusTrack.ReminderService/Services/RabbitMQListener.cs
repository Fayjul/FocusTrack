using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using FocusTrack.ReminderService.Data;
using FocusTrack.ReminderService.Models;

namespace FocusTrack.ReminderService.Services
{
    public class RabbitMQListener : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQListener(IServiceProvider serviceProvider)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" }; // Adjust the hostname as needed
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "task_exchange", type: "direct");
            _channel.QueueDeclare(queue: "reminder_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: "reminder_queue", exchange: "task_exchange", routingKey: "task.created");
            _channel.QueueBind(queue: "reminder_queue", exchange: "task_exchange", routingKey: "task.updated");
            _channel.QueueBind(queue: "reminder_queue", exchange: "task_exchange", routingKey: "task.deleted");
            _serviceProvider = serviceProvider;
        }

        public void StartListening()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var task = JsonSerializer.Deserialize<ReminderItem>(message);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ReminderDbContext>();

                    if (ea.RoutingKey == "task.created")
                    {
                        // Add new reminder item
                        dbContext.Reminders.Add(task);
                        await dbContext.SaveChangesAsync();

                        // Schedule reminder
                        var reminderScheduler = scope.ServiceProvider.GetRequiredService<ReminderScheduler>();
                        reminderScheduler.ScheduleReminder(task);
                    }
                    else if (ea.RoutingKey == "task.updated")
                    {
                        // Update reminder item
                        var existingTask = await dbContext.Reminders.FindAsync(task.TaskId);
                        if (existingTask != null)
                        {
                            existingTask.Title = task.Title;
                            existingTask.Deadline = task.Deadline;
                            await dbContext.SaveChangesAsync();

                            // Reschedule reminder
                            var reminderScheduler = scope.ServiceProvider.GetRequiredService<ReminderScheduler>();
                            reminderScheduler.RescheduleReminder(existingTask);
                        }
                    }
                    else if (ea.RoutingKey == "task.deleted")
                    {
                        // Delete reminder item
                        var existingTask = await dbContext.Reminders.FindAsync(task.TaskId);
                        if (existingTask != null)
                        {
                            dbContext.Reminders.Remove(existingTask);
                            await dbContext.SaveChangesAsync();

                            // Cancel reminder
                            var reminderScheduler = scope.ServiceProvider.GetRequiredService<ReminderScheduler>();
                            reminderScheduler.CancelReminder(existingTask.TaskId);
                        }
                    }
                }
            };

            _channel.BasicConsume(queue: "reminder_queue", autoAck: true, consumer: consumer);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
