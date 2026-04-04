using System;
using System.Collections.Generic;

namespace SplitMoney.Client.Services
{
    public interface IToastService
    {
        event Action OnShow;
        event Action OnHide;
        string Message { get; }
        ToastLevel Level { get; }
        void ShowToast(string message, ToastLevel level);
    }

    public class ToastService : IToastService, IDisposable
    {
        public event Action OnShow;
        public event Action OnHide;
        public string Message { get; private set; }
        public ToastLevel Level { get; private set; }
        private System.Timers.Timer _countdown;

        public void ShowToast(string message, ToastLevel level)
        {
            Message = message;
            Level = level;
            OnShow?.Invoke();
            StartCountdown();
        }

        private void StartCountdown()
        {
            _countdown?.Stop();
            _countdown?.Dispose();

            _countdown = new System.Timers.Timer(3000);
            _countdown.Elapsed += (sender, args) => OnHide?.Invoke();
            _countdown.AutoReset = false;
            _countdown.Start();
        }

        public void Dispose()
        {
            _countdown?.Dispose();
        }
    }

    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }
}
