using System;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;

namespace SplitMoney.Client.Components.UI
{
    public partial class GlobalModal : IDisposable
    {
        [Inject]
        public IModalService ModalService { get; set; } = default!;

        private bool isVisible = false;

        protected override void OnInitialized()
        {
            ModalService.OnShow += ShowModal;
            ModalService.OnHide += HideModal;
        }

        private void ShowModal()
        {
            isVisible = true;
            InvokeAsync(StateHasChanged);
        }

        private void HideModal()
        {
            isVisible = false;
            InvokeAsync(StateHasChanged);
        }

        private void CloseModal()
        {
            ModalService.HideModal();
        }

        private string GetIcon() => ModalService.Type switch
        {
            ModalType.Success => "✅",
            ModalType.Error => "❌",
            ModalType.Warning => "⚠️",
            _ => "ℹ️"
        };

        private string GetTitleColor() => ModalService.Type switch
        {
            ModalType.Success => "var(--success, #10b981)",
            ModalType.Error => "var(--error, #ef4444)",
            ModalType.Warning => "var(--warning, #f59e0b)",
            _ => "var(--primary, #3b82f6)"
        };

        private string GetButtonClass() => ModalService.Type switch
        {
            ModalType.Success => "btn btn-success",
            ModalType.Error => "btn btn-danger",
            ModalType.Warning => "btn btn-warning",
            _ => "editorial-button-primary"
        };

        public void Dispose()
        {
            ModalService.OnShow -= ShowModal;
            ModalService.OnHide -= HideModal;
        }
    }
}
