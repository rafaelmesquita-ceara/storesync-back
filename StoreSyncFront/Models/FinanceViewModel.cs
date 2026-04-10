using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedModels;

public partial class FinanceViewModel : ObservableObject
{
    public Finance Model { get; }

    [ObservableProperty] private Guid financeId;
    [ObservableProperty] private string? reference;
    [ObservableProperty] private string? description;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private DateTime dueDate;
    [ObservableProperty] private int status;
    [ObservableProperty] private int type;
    [ObservableProperty] private int titleType;
    [ObservableProperty] private decimal? settledAmount;
    [ObservableProperty] private DateTime? settledAt;
    [ObservableProperty] private string? settledNote;
    [ObservableProperty] private Guid? parentId;
    [ObservableProperty] private DateTime createdAt;

    public string StatusLabel => Status switch
    {
        FinanceStatus.Liquidado             => "Liquidado",
        FinanceStatus.LiquidadoParcialmente => "Liq. Parcial",
        _                                   => "Aberto"
    };

    public string TitleTypeLabel => TitleType == FinanceTitleType.Residual ? "Residual" : "Original";

    public FinanceViewModel(Finance finance)
    {
        Model = finance;

        FinanceId     = finance.FinanceId;
        Reference     = finance.Reference;
        Description   = finance.Description;
        Amount        = finance.Amount;
        DueDate       = finance.DueDate;
        Status        = finance.Status;
        Type          = finance.Type;
        TitleType     = finance.TitleType;
        SettledAmount = finance.SettledAmount;
        SettledAt     = finance.SettledAt;
        SettledNote   = finance.SettledNote;
        ParentId      = finance.ParentId;
        CreatedAt     = finance.CreatedAt;
    }
}
