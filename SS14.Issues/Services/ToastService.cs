using System.Timers;
using SS14.Issues.Data;
using Timer = System.Timers.Timer;

namespace SS14.Issues.Services;

public sealed class ToastService : IDisposable
{
    public event Action<string, ToastLevel> OnShow = (_, _) => {};
    public event Action OnHide = () => {};
    private Timer? _countdown;

    public void ShowToast(string message, ToastLevel level)
    {
        OnShow?.Invoke(message, level);
        StartCountdown();
    }

    private void StartCountdown()
    {
        SetCountdown();

        if (_countdown!.Enabled)
        {
            _countdown.Stop();
            _countdown.Start();
        }
        else
        {
            _countdown.Start();
        }
    }

    private void SetCountdown()
    {
        if (_countdown != null)
            return;
        
        _countdown = new Timer(5000);
        _countdown.Elapsed += HideToast;
        _countdown.AutoReset = false;
    }

    private void HideToast(object? source, ElapsedEventArgs args)
    {
        OnHide?.Invoke();
    }

    public void Dispose()
    {
        _countdown?.Dispose();
    }
}