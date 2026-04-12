namespace SharedModels.Interfaces;

public interface ISaleItemRepository
{
    Task<PaginatedResult<SaleItem>> GetAllSaleItemsAsync(int limit = 50, int offset = 0);
    Task<PaginatedResult<SaleItem>> GetSaleItemsBySaleIdAsync(Guid saleId, int limit = 50, int offset = 0);
    Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId);
    Task<int> CreateSaleItemAsync(SaleItem saleItem);
    Task<int> UpdateSaleItemAsync(SaleItem saleItem);
    Task<int> DeleteSaleItemAsync(Guid saleItemId);
}

public interface ISaleItemService
{
    Task<PaginatedResult<SaleItem>> GetAllSaleItemsAsync(int limit = 50, int offset = 0);
    Task<PaginatedResult<SaleItem>> GetSaleItemsBySaleIdAsync(Guid saleId, int limit = 50, int offset = 0);
    Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId);
    Task<int> CreateSaleItemAsync(SaleItem saleItem);
    Task<int> UpdateSaleItemAsync(SaleItem saleItem);
    Task<int> DeleteSaleItemAsync(Guid saleItemId);
}