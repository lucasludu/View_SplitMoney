using System;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;

namespace SplitMoney.Client.Components.UI
{
    public partial class ToastNotification : IDisposable
    {
        [Inject]
        public IToastService ToastService { get; set; } = default!;

        private bool isVisible = false;

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
            ToastService.OnHide += HideToast;
        }

        private void ShowToast()
        {
            isVisible = true;
            InvokeAsync(StateHasChanged);
        }

        private void HideToast()
        {
            isVisible = false;
            InvokeAsync(StateHasChanged);
        }

        private string LevelClass => ToastService.Level switch
        {
            ToastLevel.Success => "success",
            ToastLevel.Error => "error",
            ToastLevel.Warning => "warning",
            _ => "info"
        };

        private string Icon => ToastService.Level switch
        {
            ToastLevel.Success => "✅",
            ToastLevel.Error => "⚠️",
            ToastLevel.Warning => "🚨",
            _ => "ℹ️"
        };

        public void Dispose()
        {
            ToastService.OnShow -= ShowToast;
            ToastService.OnHide -= HideToast;
        }
    }
}
