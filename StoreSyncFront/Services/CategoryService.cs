using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class CategoryService(IApiService apiService) : ICategoryService
{
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        Response response = await apiService.GetAsync("/api/Categories");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<IEnumerable<Category>>(response.Body);

        SnackBarService.Send("Erro ao buscar categorias:" + response.Body);
        return new List<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
    {
        Response response = await apiService.GetAsync($"/api/Categories/{categoryId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Category>(response.Body);

        SnackBarService.Send("Erro ao buscar a categoria: " + response.Body);
        return null;
    }

    public async Task<int> CreateCategoryAsync(Category category)
    {
        Response response = await apiService.PostAsync($"/api/Categories", JsonContent.Create(category));
        SnackBarService.Send(response.IsSuccess() ? "Categoria inserida com sucesso." : "Erro ao inserir categoria: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> UpdateCategoryAsync(Category category)
    {
        Response response = await apiService.PutAsync($"/api/Categories/{category.CategoryId}", JsonContent.Create(category));
        SnackBarService.Send(response.IsSuccess() ? "Categoria atualizada com sucesso." : "Erro ao atualizar categoria: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> DeleteCategoryAsync(Guid categoryId)
    {
        Response response = await apiService.DeleteAsync($"/api/Categories/{categoryId}");
        SnackBarService.Send(response.IsSuccess() ? "Categoria excluída com sucesso." : "Erro ao excluir categoria: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }
}