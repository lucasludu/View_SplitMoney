using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Pages
{
    public partial class SettleDebt
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private SettleDebtModel settlement = new();
        private List<DebtSummaryViewModel> myDebts = new();
        private List<SettlementViewModel> mySettlements = new();
        private bool isLoading = true;
        private bool showForm = false;
        private string selectedCreditorName = "";
        private string selectedMethod = "💸 Efectivo";
        private bool submitting = false;
        private List<string> paymentMethods = new() { "💸 Efectivo", "🏦 Transferencia", "📱 Mercado Pago" };

        protected override async Task OnInitializedAsync() => await LoadData();

        private async Task LoadData()
        {
            isLoading = true;
            myDebts.Clear();
            mySettlements.Clear();
            
            try
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                var currentUserId = authState.User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
                var groupsResult = await ExpenseService.GetUserGroupsAsync();
                var groups = groupsResult.Succeeded ? groupsResult.Data : new List<GroupSummaryViewModel>();
                
                foreach (var group in groups)
                {
                    var balancesResult = await ExpenseService.GetGroupBalancesAsync(group.Id);
                    var balances = balancesResult.Succeeded ? balancesResult.Data : new List<BalanceResponse>();
                    
                    var activeDebts = balances
                        .Where(b => b.DebtorId == currentUserId && b.Amount > 0)
                        .Select(b => new DebtSummaryViewModel 
                        {
                            GroupId = Guid.Parse(group.Id),
                            GroupName = group.Name,
                            CreditorId = b.CreditorId,
                            CreditorName = b.CreditorName,
                            Amount = b.Amount
                        });
                    myDebts.AddRange(activeDebts);
                }
                var settlementsResult = await ExpenseService.GetMySettlementsAsync();
                mySettlements = settlementsResult.Succeeded ? settlementsResult.Data : new List<SettlementViewModel>();
            }
            finally { isLoading = false; }
        }

        private void SelectDebt(DebtSummaryViewModel debt)
        {
            settlement = new SettleDebtModel { GroupId = debt.GroupId, PayeeId = debt.CreditorId, Amount = Math.Round(debt.Amount, 2) };
            selectedCreditorName = debt.CreditorName;
            showForm = true;
        }

        private void GoBack() => Navigation.NavigateTo("/");

        private async Task HandleSubmit()
        {
            submitting = true;
            var result = await ExpenseService.SettleDebtAsync(settlement);
            submitting = false;
            if (result.Succeeded) {
                ToastService.ShowToast("¡Pago registrado! ✅", ToastLevel.Success);
                showForm = false;
                await LoadData();
            } else { ToastService.ShowToast(result.Message ?? "Error al procesar el pago", ToastLevel.Error); }
        }
    }
}
