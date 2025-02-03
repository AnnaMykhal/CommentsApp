using RabbitMQ.Client; 
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;


namespace CommentsApp.Services.RabbitMQ;

public class FileProcessor
{

    public void StartProcessing()
    {
        // Створення фабрики з'єднання
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        // Створення з'єднання та каналу
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Оголошуємо чергу
        channel.QueueDeclare(queue: "file_queue",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Споживач для обробки повідомлень
        var consumer = new EventingBasicConsumer(channel);

        // Обробка отриманих повідомлень
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var filePath = Encoding.UTF8.GetString(body);

            Console.WriteLine($"[x] Початок обробки файлу: {filePath}");
            await ProcessFileAsync(filePath);
            Console.WriteLine($"[x] Обробка файлу завершена: {filePath}");
        };

        // Споживання повідомлень з черги
        channel.BasicConsume(queue: "file_queue",
                             autoAck: true,
                             consumer: consumer);

        Console.WriteLine("Очікування завдань для обробки...");
        Console.ReadLine(); // Залишаємо процес працюючим
    }


    private async Task ProcessFileAsync(string filePath)
    {
        // Логіка обробки файлу
        await Task.Delay(2000); // Імітація часу обробки
        Console.WriteLine($"Файл {filePath} оброблено.");
    }
}
