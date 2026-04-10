namespace SharedModels.Interfaces;

public interface ISaleItemRepository
{
    Task<IEnumerable<SaleItem>> GetAllSaleItemsAsync();
    Task<IEnumerable<SaleItem>> GetSaleItemsBySaleIdAsync(Guid saleId);
    Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId);
    Task<int> CreateSaleItemAsync(SaleItem saleItem);
    Task<int> UpdateSaleItemAsync(SaleItem saleItem);
    Task<int> DeleteSaleItemAsync(Guid saleItemId);
}

public interface ISaleItemService
{
    Task<IEnumerable<SaleItem>> GetAllSaleItemsAsync();
    Task<IEnumerable<SaleItem>> GetSaleItemsBySaleIdAsync(Guid saleId);
    Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId);
    Task<int> CreateSaleItemAsync(SaleItem saleItem);
    Task<int> UpdateSaleItemAsync(SaleItem saleItem);
    Task<int> DeleteSaleItemAsync(Guid saleItemId);
}