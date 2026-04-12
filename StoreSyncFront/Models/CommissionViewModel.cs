using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedModels;

public partial class CommissionViewModel : ObservableObject
{
    public Commission Model { get; }

    [ObservableProperty] private Guid commissionId;
    [ObservableProperty] private string reference = string.Empty;
    [ObservableProperty] private string employeeName = string.Empty;
    [ObservableProperty] private string period = string.Empty;
    [ObservableProperty] private decimal totalSales;
    [ObservableProperty] private decimal commissionRate;
    [ObservableProperty] private decimal commissionValue;
    [ObservableProperty] private string? observation;
    [ObservableProperty] private DateTime startDate;
    [ObservableProperty] private DateTime endDate;
    [ObservableProperty] private DateTime createdAt;

    public CommissionViewModel(Commission commission)
    {
        Model = commission;

        CommissionId    = commission.CommissionId ?? Guid.Empty;
        Reference       = commission.Reference;
        EmployeeName    = commission.Employee?.Name ?? string.Empty;
        StartDate       = commission.StartDate;
        EndDate         = commission.EndDate;
        Period          = $"{commission.StartDate:dd/MM/yyyy} – {commission.EndDate:dd/MM/yyyy}";
        TotalSales      = commission.TotalSales;
        CommissionRate  = commission.CommissionRate;
        CommissionValue = commission.CommissionValue;
        Observation     = commission.Observation;
        CreatedAt       = commission.CreatedAt;
    }
}
