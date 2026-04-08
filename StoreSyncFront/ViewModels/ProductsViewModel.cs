using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;
using StoreSyncFront.Services;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StoreSyncFront.ViewModels;

public partial class ProductsViewModel : ObservableValidator
{
    [ObservableProperty]
    private string _searchBarField = string.Empty;

    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    public ObservableCollection<ProductViewModel> Products { get; }

    private List<ProductViewModel>? _allProducts;

    public ObservableCollection<Category> Categories { get; }

    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private Guid _productId = Guid.Empty;

    // 2. Adicionamos atributos de validação às propriedades do formulário
    [ObservableProperty]
    [Required(ErrorMessage = "O campo Referência é obrigatório.")]
    private string _reference = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    private string _name = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Estoque é obrigatório.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "O campo aceita apenas valores numéricos inteiros.")]
    private string _stockQuantity = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Preço é obrigatório.")]
    [RegularExpression(@"^\d+([,.]\d{1,2})?$", ErrorMessage = "O campo aceita apenas valores numéricos.")]
    private string _price = string.Empty;

    [ObservableProperty] private Category? _selectedCategory;


    public IRelayCommand ToggleEditCommand { get; }

    public ProductsViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;

        ToggleEditCommand = new RelayCommand(() => IsEdit = !IsEdit);
        Categories = new ObservableCollection<Category>();
        Products = new ObservableCollection<ProductViewModel>();
    }

    public async Task LoadDataAsync()
    {
        var products = await _productService.GetAllProductsAsync();
        Products.Clear();
        foreach (var p in products)
        {
            Products.Add(new ProductViewModel(p));
        }

        var categories = await _categoryService.GetAllCategoriesAsync();
        Categories.Clear();
        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        if (Products != null && Products.Any())
            _allProducts = Products.ToList();
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        ClearErrors();
        ValidateAllProperties();

        if (HasErrors)
        {
            return;
        }

        int.TryParse(StockQuantity, out int stock);
        decimal.TryParse(Price.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal priceValue);

        Product newProductModel = new Product
        {
            Reference = Reference,
            Name = Name,
            StockQuantity = stock,
            Price = priceValue,
            Category = SelectedCategory,
            CategoryId = SelectedCategory?.CategoryId ?? null
        };

        if (ProductId != Guid.Empty)
        {
            newProductModel.ProductId = ProductId;
            await _productService.UpdateProductAsync(newProductModel);

            ClearForm();
            await LoadDataAsync();
            return;
        }

        await _productService.CreateProductAsync(newProductModel);
        ClearForm();
        await LoadDataAsync();
    }

    [RelayCommand]
    public void OpenEdit(Guid productId)
    {
        ClearErrors();
        var productVm = Products.FirstOrDefault(p => p.ProductId == productId);
        if (productVm == null)
            return;

        ProductId = productVm.ProductId;
        Name = productVm.Name ?? string.Empty;
        Price = productVm.Price.ToString(System.Globalization.CultureInfo.CurrentCulture);
        Reference = productVm.Reference ?? string.Empty;
        StockQuantity = productVm.StockQuantity.ToString();
        SelectedCategory = productVm.Category;
        IsEdit = !IsEdit;
    }


    [RelayCommand]
    public async void Delete(Guid productId)
    {
        await _productService.DeleteProductAsync(productId);
        await LoadDataAsync();
    }

    [RelayCommand]
    public void ClearForm()
    {
        ClearErrors();
        IsEdit = !IsEdit;
        ProductId = Guid.Empty;
        Name = string.Empty;
        Price = string.Empty;
        Reference = string.Empty;
        StockQuantity = string.Empty;
        SelectedCategory = null;
    }

    [RelayCommand]
    public async void Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allProducts == null)
            _allProducts = Products.ToList();

        var query = (_searchBarField ?? string.Empty).Trim();

        // se vazio, restaura tudo
        if (string.IsNullOrWhiteSpace(query))
        {
            Products.Clear();
            foreach (var p in _allProducts)
                Products.Add(p);
            return;
        }

        // tokens (ex.: "martelo vermelho" -> ["martelo","vermelho"])
        var tokens = Regex.Split(query, @"\s+")
                          .Where(t => !string.IsNullOrWhiteSpace(t))
                          .Select(Normalize)
                          .ToArray();

        // filtra
        var filtered = _allProducts.Where(p =>
        {
            // cria um grande texto com todos os campos que queremos buscar
            var combined = new StringBuilder();

            // Ajuste conforme propriedades do seu ProductViewModel; estou assumindo nomes parecidos
            combined.Append(p.Reference ?? string.Empty).Append(' ');
            combined.Append(p.Name ?? string.Empty).Append(' ');
            combined.Append(p.Category?.Name ?? string.Empty).Append(' ');
            combined.Append(p.ProductId.ToString()).Append(' ');
            combined.Append(p.Price.ToString(CultureInfo.InvariantCulture)).Append(' ');
            combined.Append(p.StockQuantity.ToString(CultureInfo.InvariantCulture)).Append(' ');
            combined.Append(p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(' ');

            var combinedNorm = Normalize(combined.ToString());

            // exige que todos os tokens apareçam (AND). Se preferir OR, mude Any -> Any
            foreach (var token in tokens)
            {
                if (!combinedNorm.Contains(token))
                    return false;
            }
            return true;
        }).ToList();

        // atualiza ObservableCollection de forma eficiente (clear + add)
        Products.Clear();
        foreach (var p in filtered)
            Products.Add(p);
    }
    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

}