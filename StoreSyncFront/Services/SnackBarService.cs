using System;
using Avalonia.Threading;
using Material.Styles.Controls;
using Material.Styles.Models;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Services;

public static class SnackBarService
{
    public static void Send(string content)
    {
        SnackbarHost.Post(
            new SnackbarModel(
                content,
                TimeSpan.FromSeconds(8)),
            MainWindowViewModel.SnackBarName,
            DispatcherPriority.Normal);
    }
}