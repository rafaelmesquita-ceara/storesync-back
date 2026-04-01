namespace SharedModels.Interfaces;

public interface ISaleItemRepository
{
    Task<IEnumerable<SaleItem>> GetAllSaleItemsAsync();
    Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId);
    Task<int> CreateSaleItemAsync(SaleItem saleItem);
    Task<int> UpdateSaleItemAsync(SaleItem saleItem);
    Task<int> DeleteSaleItemAsync(Guid saleItemId);
}

public interface ISaleItemService
{
    Task<IEnumerable<SaleItem>> GetAllSaleItemsAsync();
    Task<SaleItem?> GetSaleItemByIdAsync(Guid saleId);
    Task<int> CreateSaleItemAsync(SaleItem sale);
    Task<int> UpdateSaleItemAsync(SaleItem sale);
    Task<int> DeleteSaleItemAsync(Guid saleId);
}