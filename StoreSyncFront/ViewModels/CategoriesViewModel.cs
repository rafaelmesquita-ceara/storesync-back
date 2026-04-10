using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Services;

namespace StoreSyncFront.ViewModels;

public partial class CategoryRowViewModel : ObservableObject
{
    public Category Model { get; }

    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _draftName = string.Empty;

    public Guid CategoryId => Model.CategoryId;
    public string? Name => Model.Name;
    public DateTime CreatedAt => Model.CreatedAt;

    public CategoryRowViewModel(Category model)
    {
        Model = model;
        _draftName = model.Name ?? string.Empty;
    }

    public void BeginEdit()
    {
        DraftName = Model.Name ?? string.Empty;
        IsEditing = true;
    }

    public void CancelEdit()
    {
        DraftName = Model.Name ?? string.Empty;
        IsEditing = false;
    }
}

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryService _categoryService;

    public ObservableCollection<CategoryRowViewModel> Categories { get; } = new();
    private List<CategoryRowViewModel>? _allCategories;

    [ObservableProperty] private string _searchBarField = string.Empty;
    [ObservableProperty] private string _newCategoryName = string.Empty;

    public CategoriesViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task LoadDataAsync()
    {
        var cats = await _categoryService.GetAllCategoriesAsync();
        Categories.Clear();
        foreach (var c in cats)
            Categories.Add(new CategoryRowViewModel(c));
        _allCategories = Categories.ToList();
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        var name = NewCategoryName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            SnackBarService.Send("Informe o nome da categoria.");
            return;
        }

        var code = await _categoryService.CreateCategoryAsync(new Category { Name = name });
        if (code == 0)
        {
            NewCategoryName = string.Empty;
            await LoadDataAsync();
        }
    }

    public void BeginEdit(Guid categoryId)
    {
        foreach (var row in Categories)
        {
            if (row.CategoryId == categoryId)
                row.BeginEdit();
            else
                row.CancelEdit();
        }
    }

    public async Task CommitEdit(CategoryRowViewModel row)
    {
        var name = row.DraftName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            SnackBarService.Send("O nome da categoria não pode estar vazio.");
            return;
        }

        var updated = new Category
        {
            CategoryId = row.CategoryId,
            Name = name,
            CreatedAt = row.CreatedAt
        };

        var code = await _categoryService.UpdateCategoryAsync(updated);
        if (code == 0)
        {
            row.CancelEdit();
            await LoadDataAsync();
        }
    }

    public void CancelEdit(CategoryRowViewModel row) => row.CancelEdit();

    [RelayCommand]
    public async Task Delete(Guid categoryId)
    {
        await _categoryService.DeleteCategoryAsync(categoryId);
        await LoadDataAsync();
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allCategories == null) _allCategories = Categories.ToList();

        var query = (SearchBarField ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Categories.Clear();
            foreach (var c in _allCategories) Categories.Add(c);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(Normalize)
            .ToArray();

        var filtered = _allCategories.Where(row =>
        {
            var combined = new StringBuilder();
            combined.Append(row.Name ?? "").Append(' ');
            combined.Append(row.CategoryId.ToString());
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Categories.Clear();
        foreach (var c in filtered) Categories.Add(c);
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
