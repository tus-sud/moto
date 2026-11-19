using MotoShopWeb.ViewModels;
namespace MotoShopWeb.Services;
public interface IStoreService
{
    Task<StoreList> BrowseAsync(string? q, int page, int pageSize, CancellationToken ct, string? brand = null, decimal? budget = null, string sort = "new");
    Task<StoreDetail?> DetailAsync(int id, CancellationToken ct);
}
