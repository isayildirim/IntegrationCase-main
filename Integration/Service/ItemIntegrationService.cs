using Integration.Common;
using Integration.Backend;
using StackExchange.Redis;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();
    private static IDatabase _redisDb;

    public ItemIntegrationService(string redisConnection)
    {
        _redisDb = ConnectionMultiplexer.Connect(redisConnection).GetDatabase();
    }

    public Result SaveItem(string itemContent)
    {
        var lockKey = $"lock:{itemContent}";
        var lockToken = Guid.NewGuid().ToString();
        var lockAcquired = _redisDb.LockTake(lockKey, lockToken, TimeSpan.FromSeconds(10));

        if (!lockAcquired)
        {
            return new Result(false, $"Failed to acquire lock for content {itemContent}. Try again.");
        }

        try
        {
            if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
            {
                return new Result(false, $"Duplicate item received with content {itemContent}.");
            }

            var item = ItemIntegrationBackend.SaveItem(itemContent);
            return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
        }
        finally
        {
            _redisDb.LockRelease(lockKey, lockToken);
        }
    }

    public async Task<List<Item>> GetAllItems()
    {
        return await Task.Run(() => ItemIntegrationBackend.GetAllItems());
    }
}