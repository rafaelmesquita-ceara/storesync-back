using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncFront.ViewModels;

public class PaymentTypeItem
{
    public int Value { get; }
    public string Label { get; }

    public PaymentTypeItem(int value, string label)
    {
        Value = value;
        Label = label;
    }
}

public partial class PaymentMethodsViewModel : ObservableValidator
{
    private readonly IPaymentMethodService _service;

    public string Title => "Formas de Pagamento";

    public ObservableCollection<PaymentMethod> PaymentMethods { get; } = new();
    public ObservableCollection<PaymentMethodRate> Rates { get; } = new();

    public ObservableCollection<PaymentTypeItem> TypeOptions { get; } = new()
    {
        new PaymentTypeItem(PaymentMethodType.Cash, "Dinheiro"),
        new PaymentTypeItem(PaymentMethodType.DebitCard, "Débito"),
        new PaymentTypeItem(PaymentMethodType.CreditCard, "Crédito"),
        new PaymentTypeItem(PaymentMethodType.Pix, "Pix"),
    };

    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private Guid _paymentMethodId = Guid.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    private string _name = string.Empty;

    [ObservableProperty] private PaymentTypeItem? _selectedTypeItem;
    [ObservableProperty] private PaymentMethod? _selectedPaymentMethod;
    [ObservableProperty] private PaymentMethodRate? _selectedRate;

    // For adding new rate
    [ObservableProperty] private string _newInstallments = "1";
    [ObservableProperty] private string _newRatePercentage = "0";

    public bool ShowRates => SelectedTypeItem?.Value == PaymentMethodType.DebitCard ||
                             SelectedTypeItem?.Value == PaymentMethodType.CreditCard;

    public IRelayCommand ToggleEditCommand { get; }

    public PaymentMethodsViewModel(IPaymentMethodService service)
    {
        _service = service;
        ToggleEditCommand = new RelayCommand(OpenNewForm);
    }

    public async Task LoadDataAsync()
    {
        var methods = await _service.GetAllAsync();
        PaymentMethods.Clear();
        foreach (var m in methods)
            PaymentMethods.Add(m);
    }

    private void OpenNewForm()
    {
        PaymentMethodId = Guid.Empty;
        Name = string.Empty;
        SelectedTypeItem = TypeOptions.FirstOrDefault();
        Rates.Clear();
        SelectedRate = null;
        NewInstallments = "1";
        NewRatePercentage = "0";
        IsEdit = true;
    }

    public void OpenEdit(PaymentMethod pm)
    {
        PaymentMethodId = pm.PaymentMethodId;
        Name = pm.Name;
        SelectedTypeItem = TypeOptions.FirstOrDefault(t => t.Value == pm.Type);
        Rates.Clear();
        foreach (var r in pm.Rates ?? new System.Collections.Generic.List<PaymentMethodRate>())
            Rates.Add(r);
        SelectedRate = null;
        NewInstallments = "1";
        NewRatePercentage = "0";
        IsEdit = true;
    }

    [RelayCommand]
    public async Task Save()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        var pm = new PaymentMethod
        {
            PaymentMethodId = PaymentMethodId,
            Name   = Name.Trim(),
            Type   = SelectedTypeItem?.Value ?? PaymentMethodType.Cash,
            Status = PaymentMethodStatus.Ativo
        };

        int result;
        if (PaymentMethodId == Guid.Empty)
            result = await _service.CreateAsync(pm);
        else
            result = await _service.UpdateAsync(pm);

        if (result == 0)
        {
            await LoadDataAsync();
            ClearForm();
        }
    }

    [RelayCommand]
    public async Task Delete(PaymentMethod pm)
    {
        var result = await _service.DeleteAsync(pm.PaymentMethodId);
        if (result == 0)
            await LoadDataAsync();
    }

    [RelayCommand]
    public async Task AddRate()
    {
        if (PaymentMethodId == Guid.Empty) return;

        if (!int.TryParse(NewInstallments, out int installments) || installments < 1)
        {
            StoreSyncFront.Services.SnackBarService.Send("Informe um número de parcelas válido (>= 1).");
            return;
        }

        if (!decimal.TryParse(NewRatePercentage.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ratePerc) || ratePerc < 0)
        {
            StoreSyncFront.Services.SnackBarService.Send("Informe uma taxa percentual válida (>= 0).");
            return;
        }

        var rate = new PaymentMethodRate
        {
            PaymentMethodId = PaymentMethodId,
            Installments    = installments,
            RatePercentage  = ratePerc
        };

        var result = await _service.AddRateAsync(PaymentMethodId, rate);
        if (result == 0)
        {
            var refreshed = await _service.GetByIdAsync(PaymentMethodId);
            if (refreshed != null)
            {
                Rates.Clear();
                foreach (var r in refreshed.Rates ?? new System.Collections.Generic.List<PaymentMethodRate>())
                    Rates.Add(r);
            }
            NewInstallments = "1";
            NewRatePercentage = "0";
        }
    }

    [RelayCommand]
    public async Task RemoveRate()
    {
        if (SelectedRate == null || PaymentMethodId == Guid.Empty) return;

        var result = await _service.DeleteRateAsync(PaymentMethodId, SelectedRate.RateId);
        if (result == 0)
        {
            Rates.Remove(SelectedRate);
            SelectedRate = null;
        }
    }

    [RelayCommand]
    public void ClearForm()
    {
        IsEdit = false;
        PaymentMethodId = Guid.Empty;
        Name = string.Empty;
        SelectedTypeItem = null;
        SelectedPaymentMethod = null;
        Rates.Clear();
        SelectedRate = null;
        NewInstallments = "1";
        NewRatePercentage = "0";
        ClearErrors();
    }

    [RelayCommand]
    public async Task Refresh()
    {
        await LoadDataAsync();
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(SelectedTypeItem))
            OnPropertyChanged(nameof(ShowRates));
    }
}
