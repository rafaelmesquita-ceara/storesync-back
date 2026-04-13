using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedModels;

namespace StoreSyncFront.ViewModels.Dashboard;

public record DashboardDataBundle(
    List<Sale> Sales,
    List<Finance> Finances,
    List<Product> Products,
    List<Category> Categories,
    List<Employee> Employees,
    List<SaleItem> SaleItems,
    List<SalePayment> SalePayments,
    List<PaymentMethod> PaymentMethods
);

public abstract partial class DashboardPageViewModelBase : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;

    public abstract void BuildFromData(DashboardDataBundle bundle);
}
