using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Notifications
{
    public partial class NotificationCenter
    {
        [Inject] public INotificationService NotificationService { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        private List<NotificationViewModel> notifications = new();
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadNotifications();
        }

        private async Task LoadNotifications()
        {
            isLoading = true;
            notifications = await NotificationService.GetNotificationsAsync();
            isLoading = false;
            StateHasChanged();
        }

        private async Task ConfirmExpense(Guid expenseId)
        {
            var success = await NotificationService.ConfirmExpenseAsync(expenseId);
            if (success)
            {
                ToastService.ShowToast("Gasto confirmado correctamente.", ToastLevel.Success);
                await LoadNotifications();
            }
            else
            {
                ToastService.ShowToast("Error al confirmar el gasto.", ToastLevel.Error);
            }
        }

        private async Task DismissNotification(Guid id)
        {
            var success = await NotificationService.DeleteNotificationAsync(id);
            if (success)
            {
                await LoadNotifications();
            }
        }

        private async Task ClearAll()
        {
            var success = await NotificationService.ClearAllNotificationsAsync();
            if (success)
            {
                await LoadNotifications();
            }
        }

        private string GetTimeAgo(DateTime date)
        {
            var diff = DateTime.UtcNow - date;
            if (diff.TotalMinutes < 1) return "Recién ahora";
            if (diff.TotalMinutes < 60) return $"Hace {Math.Floor(diff.TotalMinutes)} min";
            if (diff.TotalHours < 24) return $"Hace {Math.Floor(diff.TotalHours)} h";
            return date.ToShortDateString();
        }
    }
}
