using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedModels;

public partial class SaleViewModel : ObservableObject
{
    public Sale Model { get; }

    [ObservableProperty] private Guid saleId;
    [ObservableProperty] private string? referencia;
    [ObservableProperty] private Guid employeeId;
    [ObservableProperty] private string? employeeName;
    [ObservableProperty] private decimal discount;
    [ObservableProperty] private decimal addition;
    [ObservableProperty] private decimal totalAmount;
    [ObservableProperty] private int status;
    [ObservableProperty] private DateTime saleDate;
    [ObservableProperty] private DateTime createdAt;

    public string StatusLabel => Status switch
    {
        SaleStatus.Finalizada => "Finalizada",
        SaleStatus.Cancelada => "Cancelada",
        _ => "Aberta"
    };

    public SaleViewModel(Sale sale)
    {
        Model = sale;

        SaleId = sale.SaleId;
        Referencia = sale.Referencia;
        EmployeeId = sale.EmployeeId;
        EmployeeName = sale.Employee?.Name;
        Discount = sale.Discount;
        Addition = sale.Addition;
        TotalAmount = sale.TotalAmount;
        Status = sale.Status;
        SaleDate = sale.SaleDate;
        CreatedAt = sale.CreatedAt;
    }
}
