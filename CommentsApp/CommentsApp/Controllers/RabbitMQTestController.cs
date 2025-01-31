using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace CommentsApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RabbitMQTestController : ControllerBase
{
    [HttpGet("test")]
    public IActionResult TestRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "test_queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = "Hello RabbitMQ!";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: "test_queue",
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine(" [x] Sent {0}", message);

            return Ok("RabbitMQ message sent successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
