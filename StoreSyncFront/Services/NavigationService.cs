using Avalonia.Controls;
using StoreSyncFront.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StoreSyncFront.Services;

public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : class;

    void NavigateToRoot<TViewModel>() where TViewModel : class;

    void NavigateToBack();

    void Initialize(ContentControl contentControl);
}

public class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

    private readonly Stack stackNavigation = new();

    private ContentControl contentControl = new();

    public void Initialize(ContentControl contentControl)
    {
        this.contentControl = contentControl;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = serviceProvider.GetRequiredService<TViewModel>();
        var viewType = ResolveViewType(viewModel.GetType());
        if (viewModel is ObservableObject)
        {
            stackNavigation.Push(viewModel);
            var view = (UserControl)serviceProvider.GetRequiredService(viewType);
            view.DataContext = viewModel;
            contentControl.Content = view;
        }
    }

    public void NavigateToRoot<TViewModel>() where TViewModel : class
    {
        stackNavigation.Clear();
        NavigateTo<TViewModel>();
    }

    public void NavigateToBack()
    {
        if (stackNavigation.Count > 1) stackNavigation.Pop();
        var viewModel = stackNavigation.Pop();
        if (viewModel is ViewModelBase)
        {
            stackNavigation.Push(viewModel);
            var viewType = ResolveViewType(viewModel.GetType());
            var view = (UserControl)serviceProvider.GetRequiredService(viewType);
            view.DataContext = viewModel;
            contentControl.Content = view;
        }
    }

    private static Type ResolveViewType(Type viewModelType)
    {
        var viewName = viewModelType.FullName!.Replace("ViewModel", "View");
        var viewAssemblyName = viewModelType.Assembly.FullName;
        var viewTypeName = $"{viewName}, {viewAssemblyName}";
        return Type.GetType(viewTypeName)!;
    }
}