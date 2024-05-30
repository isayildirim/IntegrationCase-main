using Integration.Service;
using Microsoft.Extensions.Configuration;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var service = new ItemIntegrationService(GetRedisConnection());

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(500);

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));

        Thread.Sleep(5000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItems().Result.ForEach(Console.WriteLine);

        Console.ReadLine();
    }

    private static String GetRedisConnection()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return configuration["Redis:ConnectionString"] ?? "localhost:6379";
    }
}