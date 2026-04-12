using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Net.Http;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StoreSyncFront.Services;
using StoreSyncFront.ViewModels;
using StoreSyncFront.Views;

namespace StoreSyncFront;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LiveCharts.Configure(config => config.AddSkiaSharp().AddDefaultMappers().AddLightTheme());

        // Datas vindas da API são UTC — converter automaticamente para horário local ao desserializar
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        BindingPlugins.DataValidators.RemoveAt(0);
        var collection = new ServiceCollection();
        
        // Registrar todos os serviços, Views e ViewModels
        collection.AddCommonServices();
        collection.AddTransient<HomeView>().AddTransient<HomeViewModel>();
        collection.AddTransient<ProductsView>().AddTransient<ProductsViewModel>();
        collection.AddTransient<ClientsView>().AddTransient<ClientsViewModel>();
        
        var services = collection.BuildServiceProvider();

        // Adiciona o ViewLocator à coleção de DataTemplates da aplicação.
        // Isso permite que o ContentControl encontre e renderize a View correta para cada ViewModel automaticamente.
        DataTemplates.Add(new ViewLocator());
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = services.GetRequiredService<MainWindow>();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}