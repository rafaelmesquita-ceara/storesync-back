using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace StoreSyncFront.Services;

public class ToastModel
{
    public string Message { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#1565C0";
    public string IconKind { get; set; } = "Information";
}

public static class SnackBarService
{
    public static ObservableCollection<ToastModel> Toasts { get; } = new();

    public static void Send(string content) => Show(content, "Info");
    public static void SendSuccess(string content) => Show(content, "Success");
    public static void SendWarning(string content) => Show(content, "Warning");
    public static void SendError(string content) => Show(content, "Error");

    private static void Show(string content, string type)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var toast = new ToastModel
            {
                Message = content,
                BackgroundColor = type switch
                {
                    "Success" => "#43A047", // A beautiful shade of Green
                    "Warning" => "#F57C00", // A beautiful shade of Orange/Yellow
                    "Error" => "#E53935",   // A beautiful shade of Red
                    _ => "#1E88E5"          // A beautiful shade of Blue
                },
                IconKind = type switch
                {
                    "Success" => "CheckCircleOutline",
                    "Warning" => "AlertCircleOutline",
                    "Error" => "CloseCircleOutline",
                    _ => "InformationOutline"
                }
            };
            
            Toasts.Add(toast);
            await Task.Delay(4000); // Exibe por 4 segundos
            Toasts.Remove(toast);
        });
    }
}