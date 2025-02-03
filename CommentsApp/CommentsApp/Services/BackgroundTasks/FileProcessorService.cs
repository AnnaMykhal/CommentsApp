using CommentsApp.Services.RabbitMQ;

namespace CommentsApp.Services.BackgroundTasks;

public class FileProcessorService
{
    private readonly FileProcessingQueue _queue;

    public FileProcessorService(FileProcessingQueue queue)
    {
        _queue = queue;
    }

    public void StartProcessing()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                var filePath = _queue.Dequeue(); // Отримуємо файл з черги
                if (!string.IsNullOrEmpty(filePath))
                {
                    // Логіка обробки файлів
                    await ProcessFileAsync(filePath);
                }
                await Task.Delay(1000); // Затримка перед наступною обробкою
            }
        });
    }

    private async Task ProcessFileAsync(string filePath)
    {
        // Тут можна додавати обробку файлів, наприклад, збереження або зміну формату
        Console.WriteLine($"Processing file: {filePath}");
        // Реалізуйте обробку файлів відповідно до ваших вимог
    }
}
