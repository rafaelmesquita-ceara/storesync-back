using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class ProductService(IApiService apiService) : IProductService
{
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        Response response = await apiService.GetAsync("/api/Products");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<IEnumerable<Product>>(response.Body);

        SnackBarService.Send("Erro ao buscar produtos:" + response.Body);
        return new List<Product>();
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        Response response = await apiService.GetAsync($"/api/Products/{productId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Product>(response.Body);

        SnackBarService.Send("Erro ao buscar o produto: " + response.Body);
        return null;
    }

    public async Task<int> CreateProductAsync(Product product)
    {
        Response response = await apiService.PostAsync($"/api/Products", JsonContent.Create(product));
        SnackBarService.Send(response.IsSuccess() ? "Produto inserido com sucesso." : "Erro ao inserir produto: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> UpdateProductAsync(Product product)
    {
        Response response = await apiService.PutAsync($"/api/Products/{product.ProductId}", JsonContent.Create(product));
        SnackBarService.Send(response.IsSuccess() ? "Produto atualizado com sucesso." : "Erro ao atualizar produto: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> DeleteProductAsync(Guid productId)
    {
        Response response = await apiService.DeleteAsync($"/api/Products/{productId}");
        SnackBarService.Send(response.IsSuccess() ? "Produto excluído com sucesso." : "Erro ao excluir produto: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }
}