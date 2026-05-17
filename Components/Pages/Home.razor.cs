using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Pages
{
    public partial class Home : IDisposable
    {
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public ITipService TipService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public IModalService ModalService { get; set; } = default!;
        [Inject] public INotificationService NotificationService { get; set; } = default!;
        [Inject] public IAuthService AuthService { get; set; } = default!;

        private string userName = "User";
        private bool isTipModalOpen = false;
        private DashboardViewModel? dashboard;
        private bool isLoading = true;
        private bool isDeleting = false;
        private bool isPremium = false;
        private bool showAudit = false;
        private bool loadingAudit = false;
        private bool showPaywall = false;
        private bool showNotifications = false;
        private int unreadCount = 0;
        private string paywallTitle = "Controle su Dinero";
        private TipViewModel? currentTip;
        private ExpenseAuditViewModel? auditHistory;
        private ExpenseDetailViewModel? selectedExpense;

        private void ShowTipModal() => isTipModalOpen = true;
        private void CloseTipModal() => isTipModalOpen = false;
        private void HandleUpgrade() => Navigation.NavigateTo("/premium");
        
        private async Task HandleSimulatePremium() 
        {
            await AuthService.SimulatePremiumAsync();
            isPremium = true;
            ToastService.ShowToast("🌟 MODO PREMIUM ACTIVADO (SIMULACIÓN)", ToastLevel.Info);
            StateHasChanged();
        }

        private async Task ShowExpenseDetail(Guid expenseId)
        {
            showAudit = false; // Reset to details tab
            auditHistory = null;
            var detailResult = await ExpenseService.GetExpenseDetailsAsync(expenseId);
            if (detailResult.Succeeded)
            {
                selectedExpense = detailResult.Data;
            }
            else
            {
                selectedExpense = null;
                ToastService.ShowToast(detailResult.Message ?? "Error al cargar detalle", ToastLevel.Error);
            }
            StateHasChanged();
        }

        private async Task LoadAudit()
        {
            if (selectedExpense == null) return;
            if (!isPremium)
            {
                paywallTitle = "Historial de Cambios";
                showPaywall = true;
                return;
            }
            showAudit = true;
            loadingAudit = true;
            var auditResult = await ExpenseService.GetExpenseAuditAsync(selectedExpense.Id);
            loadingAudit = false;
            
            if (auditResult.Succeeded)
            {
                auditHistory = auditResult.Data;
            }
            else
            {
                ToastService.ShowToast(auditResult.Message ?? "Error al cargar historial", ToastLevel.Error);
            }
        }

        private void CloseExpenseDetail()
        {
            selectedExpense = null;
        }

        private async Task ConfirmDelete()
        {
            if (selectedExpense == null) return;
            
            bool confirmed = await ModalService.ShowConfirmAsync("Eliminar Gasto", $"¿Borrar '{selectedExpense.Description}'? Esta acción es irreversible.", ModalType.Warning);
            
            if (confirmed)
            {
                isDeleting = true;
                var result = await ExpenseService.DeleteExpenseAsync(selectedExpense.Id);
                isDeleting = false;
        
                if (result.Succeeded)
                {
                    ToastService.ShowToast("Gasto eliminado 🗑️", ToastLevel.Success);
                    selectedExpense = null;
                    await OnInitializedAsync(); // Refresh dashboard
                }
                else
                {
                    ToastService.ShowToast(result.Message ?? "No se pudo eliminar el gasto", ToastLevel.Error);
                }
            }
        }

        private void NavigateToEdit()
        {
            if (selectedExpense != null)
            {
                Navigation.NavigateTo($"/expenses/edit/{selectedExpense.Id}");
            }
        }

        private IEnumerable<string> GetAllInvolvedMembers()
        {
            if (selectedExpense == null) return Enumerable.Empty<string>();
            
            var paymentUsers = selectedExpense.Payments.Select(p => p.UserName);
            var splitUsers = selectedExpense.Splits.Select(s => s.UserName);
            
            return paymentUsers.Union(splitUsers).Distinct().OrderBy(u => u);
        }

        private bool IsDashboardEmpty() => 
            dashboard == null || 
            (dashboard.TotalToReceive == 0 && 
             dashboard.TotalToPay == 0 && 
             dashboard.TotalMonthSpending == 0 && 
             (dashboard.RecentExpenses == null || !dashboard.RecentExpenses.Any()));

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            
            NotificationService.NotificationsChanged += OnNotificationsChanged;

            isPremium = await AuthService.IsPremiumAsync();

            var user = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var firstName = user.Claims.FirstOrDefault(c => c.Type == "FirstName" || c.Type == ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(firstName))
                    userName = firstName;
            }
            var dashboardResult = await ExpenseService.GetDashboardAsync();
            if (dashboardResult.Succeeded)
            {
                dashboard = dashboardResult.Data;
            }
            else
            {
                dashboard = null;
                ToastService.ShowToast(dashboardResult.Message ?? "Error de conexión", ToastLevel.Error);
            }
            
            currentTip = TipService.GetMonthlyTip();
            
            await RefreshUnreadCount();

            isLoading = false;
        }

        private async Task RefreshUnreadCount()
        {
            var notifications = await NotificationService.GetNotificationsAsync();
            unreadCount = notifications.Count(n => !n.IsRead);
        }

        private void ToggleNotifications()
        {
            showNotifications = !showNotifications;
            StateHasChanged();
        }

        private async Task HandleCreateExpenseClick()
        {
            var groupsResult = await ExpenseService.GetUserGroupsAsync();
            var groups = groupsResult.Data;
            if (groups == null || !groups.Any())
            {
                ModalService.ShowModal("No hay Grupos", "Debes pertenecer a un círculo antes de cargar gastos. Crea tu primer grupo ahora.", ModalType.Warning);
                Navigation.NavigateTo("/groups");
            }
            else
            {
                Navigation.NavigateTo("/expenses/new");
            }
        }

        private async void OnNotificationsChanged()
        {
            await InvokeAsync(async () => 
            {
                await RefreshUnreadCount();
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            NotificationService.NotificationsChanged -= OnNotificationsChanged;
        }
    }
}
