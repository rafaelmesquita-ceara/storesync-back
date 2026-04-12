using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncFront.ViewModels;

public partial class ClientsViewModel : ObservableValidator
{
    [ObservableProperty] private string _searchBarField = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount = 0;
    private int _pageSize = 50;

    public bool CanPreviousPage => CurrentPage > 1;
    public bool CanNextPage => CurrentPage < TotalPages;

    [RelayCommand(CanExecute = nameof(CanPreviousPage))]
    private async Task PreviousPage()
    {
        CurrentPage--;
        await LoadDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanNextPage))]
    private async Task NextPage()
    {
        CurrentPage++;
        await LoadDataAsync();
    }

    private readonly IClientService _clientService;

    public ObservableCollection<Client> Clients { get; } = new();
    private List<Client>? _allClients;

    public ObservableCollection<StatusItem> StatusOptions { get; } = new()
    {
        new StatusItem(ClientStatus.Ativo, "Ativo"),
        new StatusItem(ClientStatus.Inativo, "Inativo"),
        new StatusItem(ClientStatus.Bloqueado, "Bloqueado"),
    };

    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private Guid _clientId = Guid.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    private string _name = string.Empty;

    [ObservableProperty] private string _cpfCnpj = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _addressNumber = string.Empty;
    [ObservableProperty] private string _addressComplement = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _state = string.Empty;
    [ObservableProperty] private string _postalCode = string.Empty;
    [ObservableProperty] private int _selectedStatus = ClientStatus.Ativo;

    public IRelayCommand ToggleEditCommand { get; }

    public ClientsViewModel(IClientService clientService)
    {
        _clientService = clientService;
        ToggleEditCommand = new RelayCommand(() => IsEdit = !IsEdit);
    }

    public async Task LoadDataAsync()
    {
        var offset = (CurrentPage - 1) * _pageSize;
        var paginatedResult = await _clientService.GetAllClientsAsync(_pageSize, offset);

        TotalCount = paginatedResult.TotalCount;
        TotalPages = (int)Math.Ceiling((double)TotalCount / _pageSize);
        if (TotalPages == 0) TotalPages = 1;

        Clients.Clear();
        foreach (var c in paginatedResult.Items)
            Clients.Add(c);

        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();

        _allClients = Clients.ToList();
    }

    [RelayCommand]
    private async Task AddClient()
    {
        ClearErrors();
        ValidateAllProperties();
        if (HasErrors) return;

        var client = new Client
        {
            Name = Name,
            CpfCnpj = string.IsNullOrWhiteSpace(CpfCnpj) ? null : CpfCnpj,
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone,
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email,
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address,
            AddressNumber = string.IsNullOrWhiteSpace(AddressNumber) ? null : AddressNumber,
            AddressComplement = string.IsNullOrWhiteSpace(AddressComplement) ? null : AddressComplement,
            City = string.IsNullOrWhiteSpace(City) ? null : City,
            State = string.IsNullOrWhiteSpace(State) ? null : State,
            PostalCode = string.IsNullOrWhiteSpace(PostalCode) ? null : PostalCode,
            Status = SelectedStatus
        };

        if (ClientId != Guid.Empty)
        {
            client.ClientId = ClientId;
            await _clientService.UpdateClientAsync(client);
            ClearForm();
            await LoadDataAsync();
            return;
        }

        await _clientService.CreateClientAsync(client);
        ClearForm();
        await LoadDataAsync();
    }

    [RelayCommand]
    public void OpenEdit(Guid clientId)
    {
        ClearErrors();
        var c = Clients.FirstOrDefault(x => x.ClientId == clientId);
        if (c == null) return;

        ClientId = c.ClientId;
        Name = c.Name ?? string.Empty;
        CpfCnpj = c.CpfCnpj ?? string.Empty;
        Phone = c.Phone ?? string.Empty;
        Email = c.Email ?? string.Empty;
        Address = c.Address ?? string.Empty;
        AddressNumber = c.AddressNumber ?? string.Empty;
        AddressComplement = c.AddressComplement ?? string.Empty;
        City = c.City ?? string.Empty;
        State = c.State ?? string.Empty;
        PostalCode = c.PostalCode ?? string.Empty;
        SelectedStatus = c.Status;
        IsEdit = true;
    }

    [RelayCommand]
    public async void Delete(Guid clientId)
    {
        await _clientService.DeleteClientAsync(clientId);
        await LoadDataAsync();
    }

    [RelayCommand]
    public void ClearForm()
    {
        ClearErrors();
        IsEdit = false;
        ClientId = Guid.Empty;
        Name = string.Empty;
        CpfCnpj = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Address = string.Empty;
        AddressNumber = string.Empty;
        AddressComplement = string.Empty;
        City = string.Empty;
        State = string.Empty;
        PostalCode = string.Empty;
        SelectedStatus = ClientStatus.Ativo;
    }

    [RelayCommand]
    public async void Refresh()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allClients == null) _allClients = Clients.ToList();

        var query = (_searchBarField ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            Clients.Clear();
            foreach (var c in _allClients) Clients.Add(c);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(Normalize)
            .ToArray();

        var filtered = _allClients.Where(c =>
        {
            var combined = new StringBuilder();
            combined.Append(c.Reference ?? "").Append(' ');
            combined.Append(c.Name ?? "").Append(' ');
            combined.Append(c.CpfCnpj ?? "").Append(' ');
            combined.Append(c.Phone ?? "").Append(' ');
            combined.Append(c.Email ?? "").Append(' ');
            combined.Append(c.City ?? "").Append(' ');
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Clients.Clear();
        foreach (var c in filtered) Clients.Add(c);
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

public record StatusItem(int Value, string Label);
