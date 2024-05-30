using System.Collections.Concurrent;
using Integration.Common;
using Integration.Backend;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();
    private ConcurrentDictionary<string, Item> Cache { get; set; } = new();

    public Result SaveItem(string itemContent)
    {
        var content = Cache.GetOrAdd(itemContent, new Item());
        lock (content)
        {
            if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
            {
                return new Result(false, $"Duplicate item received with content {itemContent}.");
            }

            var item = ItemIntegrationBackend.SaveItem(itemContent);

            return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
        }
    }

    public async Task<List<Item>> GetAllItems()
    {
        return await Task.Run(() => ItemIntegrationBackend.GetAllItems());
    }
}