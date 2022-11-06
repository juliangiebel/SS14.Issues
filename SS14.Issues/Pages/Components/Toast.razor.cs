using Microsoft.AspNetCore.Components;
using SS14.Issues.Data;
using SS14.Issues.Services;

namespace SS14.Issues.Pages.Components;

public class ToastBase : ComponentBase, IDisposable
{
    [Inject] protected ToastService? ToastService { get; set; }

    protected string? Heading { get; set; }
    protected string? Message { get; set; }
    protected bool IsVisible { get; set; }
    protected string? BackgroundCssClass { get; set; }
    protected string? IconCssClass { get; set; }

    protected override void OnInitialized()
    {
        ToastService!.OnShow += ShowToast;
        ToastService!.OnHide += async () => await InvokeAsync(HideToast);
    }

    private void ShowToast(string message, ToastLevel level)
    {
        BuildToastSettings(level, message);    
        IsVisible = true;
        StateHasChanged();
    }

    private void HideToast()
    {
        IsVisible = false;
        StateHasChanged();
    }

    private void BuildToastSettings(ToastLevel level, string message)
    {
        switch (level)
        {
            case ToastLevel.Info:
                BackgroundCssClass = "color-bg-accent-emphasis";
                IconCssClass = "info";
                Heading = "Info";
                break;
            case ToastLevel.Success:
                BackgroundCssClass = "color-bg-success-emphasis";
                IconCssClass = "check";
                Heading = "Success";
                break;
            case ToastLevel.Warning:
                BackgroundCssClass = "color-bg-attention-emphasis";
                IconCssClass = "exclamation";
                Heading = "Warning";
                break;
            case ToastLevel.Error:
                BackgroundCssClass = "color-bg-danger-emphasis";
                IconCssClass = "times";
                Heading = "Error";
                break;
        }
        
        Message = message;
    }

    public void Dispose()
    {
        ToastService!.OnShow -= ShowToast;
    }
}