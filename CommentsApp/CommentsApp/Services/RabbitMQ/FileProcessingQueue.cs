using RabbitMQ.Client;
using System.Text;

namespace CommentsApp.Services.RabbitMQ;

public class FileProcessingQueue : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;


    public FileProcessingQueue(IConnection connection, IModel channel)
    {
        _connection = connection;
        _channel = channel;

        // Створення черги
        _channel.QueueDeclare(queue: "file_queue",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }
    //public FileProcessingQueue()
    //{
    //    var factory = new ConnectionFactory()
    //    {
    //        HostName = "localhost",  
    //        Port = 5672,            
    //        UserName = "guest",     
    //        Password = "guest",     

    //        // Опціонально: Налаштування тайм-аутів
    //        RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
    //        HandshakeContinuationTimeout = TimeSpan.FromSeconds(30),
    //        ContinuationTimeout = TimeSpan.FromSeconds(30)
    //    };
    //    //var factory = new ConnectionFactory()
    //    //{
    //    //    // Ensure the correct RabbitMQ URI or fallback to localhost
    //    //    Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URI") ?? "amqp://guest:guest@rabbitmq_container:5672"),

    //    //    // Optional: Adjust connection timeout to avoid handshake timeouts
    //    //    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),

    //    //    // Optional: Adjust other timeouts if needed
    //    //    HandshakeContinuationTimeout = TimeSpan.FromSeconds(30),
    //    //    ContinuationTimeout = TimeSpan.FromSeconds(30)
    //    //};

    //    try
    //    {
    //        _connection = factory.CreateConnection(); // Establish connection
    //        _channel = _connection.CreateModel();     // Create a channel

    //        // Declare the queue to ensure it exists
    //        _channel.QueueDeclare(queue: "file_queue",
    //                             durable: true,
    //                             exclusive: false,
    //                             autoDelete: false,
    //                             arguments: null);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"[Error] Failed to connect to RabbitMQ: {ex.Message}");
    //        throw;
    //    }
    //}

    public void Enqueue(string filePath)
    {
        var body = Encoding.UTF8.GetBytes(filePath);

        _channel.BasicPublish(exchange: "",
                             routingKey: "file_queue",
                             basicProperties: null,
                             body: body);

        Console.WriteLine($"[x] Завдання для обробки файлу '{filePath}' додано у чергу.");
    }

    // Метод для витягування елемента з черги
    public string? Dequeue()
    {
        var result = _channel.BasicGet("file_queue", true); // Отримуємо елемент з черги

        if (result != null)
        {
            var filePath = Encoding.UTF8.GetString(result.Body.ToArray());
            Console.WriteLine($"[x] Завдання для обробки файлу '{filePath}' отримано з черги.");
            return filePath;
        }

        return null; // Якщо черга порожня
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}
