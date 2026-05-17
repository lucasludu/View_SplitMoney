using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;
using SplitMoney.Client.Models.ViewModels;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace SplitMoney.Client.Components.Pages
{
    public partial class Groups
    {
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public IModalService ModalService { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] public Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; } = default!;

        private List<GroupSummaryViewModel>? groups;
        private GroupSpendingBreakdownViewModel? breakdown;
        private List<GroupMemberResponse>? members;
        private List<BalanceResponse>? balances;
        private GroupSpendingSummaryViewModel? spendingSummary;
        private GroupSummaryViewModel? selectedGroup;
        private bool isLoading = true;
        private bool loadingDetails = false;
        private bool loadingSummary = false;
        private bool exporting = false;
        private bool isPremium = false;
        private bool showPaywall = false;
        private string paywallTitle = "Función Premium";
        private string currentUserEmail = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var isRealPremium = authState.User.IsInRole("PremiumUser");
            var isSimulatedPremium = await LocalStorage.GetItemAsync<bool>("is_simulated_premium");
            isPremium = isRealPremium || isSimulatedPremium;
            currentUserEmail = authState.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? "";

            await LoadGroups();
        }

        private async Task LoadGroups()
        {
            isLoading = true;
            var result = await ExpenseService.GetUserGroupsAsync();
            groups = result.Succeeded ? result.Data : new List<GroupSummaryViewModel>();
            isLoading = false;
            StateHasChanged();
        }

        private async Task ShowGroupDetails(GroupSummaryViewModel group)
        {
            selectedGroup = group;
            loadingDetails = true;
            loadingSummary = true;
            spendingSummary = null;
            StateHasChanged();
            
            try 
            {
                var breakdownTask = ExpenseService.GetGroupSpendingBreakdownAsync(group.Id);
                var membersTask = ExpenseService.GetGroupMembersAsync(group.Id);
                var balancesTask = ExpenseService.GetGroupBalancesAsync(group.Id);
                var summaryTask = isPremium 
                    ? ExpenseService.GetGroupSpendingSummaryAsync(Guid.Parse(group.Id)) 
                    : Task.FromResult(ApiResult<GroupSpendingSummaryViewModel>.Failure("Premium required"));

                await Task.WhenAll(breakdownTask, membersTask, balancesTask, summaryTask);
                
                breakdown = breakdownTask.Result.Succeeded ? breakdownTask.Result.Data : null;
                members = membersTask.Result.Succeeded ? membersTask.Result.Data : new List<GroupMemberResponse>();
                balances = balancesTask.Result.Succeeded ? balancesTask.Result.Data : new List<BalanceResponse>();
                spendingSummary = summaryTask.Result?.Succeeded == true ? summaryTask.Result.Data : null;
            }
            catch (Exception)
            {
                ModalService.ShowModal("Error", "No se pudieron obtener los detalles.", ModalType.Error);
            }
            finally
            {
                loadingDetails = false;
                loadingSummary = false;
                StateHasChanged();
            }
        }
        
        private async Task HandleExport()
        {
            if (selectedGroup == null) return;
            exporting = true;
            try
            {
                var result = await ExpenseService.ExportGroupReportAsync(Guid.Parse(selectedGroup.Id));
                if (result.Succeeded && result.Data != null)
                {
                    var fileName = $"Reporte_{selectedGroup.Name}_{DateTime.Now:yyyyMMdd}.xlsx";
                    await JSRuntime.InvokeVoidAsync("downloadFile", fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Convert.ToBase64String(result.Data));
                    ToastService.ShowToast("Reporte descargado 📄", ToastLevel.Success);
                }
                else
                {
                    ToastService.ShowToast(result.Message ?? "Error al exportar", ToastLevel.Error);
                }
            }
            finally
            {
                exporting = false;
            }
        }

        private void ShowPaywall(string title)
        {
            paywallTitle = title;
            showPaywall = true;
        }

        private void HandleUpgrade() => Navigation.NavigateTo("/premium");

        private async Task HandleSimulatePremium() 
        {
            isPremium = true;
            await LocalStorage.SetItemAsync("is_simulated_premium", true);
            if (selectedGroup != null) await ShowGroupDetails(selectedGroup);
            else StateHasChanged();
        }

        private void CloseDetails()
        {
            selectedGroup = null;
            members = null;
            breakdown = null;
            spendingSummary = null;
        }
        
        private void NavigateToCreateGroup() 
        {
            if (!isPremium && groups?.Count >= 3)
            {
                ShowPaywall("Límite de Grupos Alcanzado");
                return;
            }
            Navigation.NavigateTo("/groups/new");
        }
        
        private void NavigateToNewExpense()
        {
            if (selectedGroup != null) Navigation.NavigateTo($"/expenses/new?groupId={selectedGroup.Id}");
        }

        private void NavigateToEditGroup()
        {
            if (selectedGroup != null) Navigation.NavigateTo($"/groups/edit/{selectedGroup.Id}");
        }

        private async Task ConfirmDeleteGroup()
        {
            if (selectedGroup == null) return;
            bool confirmed = await ModalService.ShowConfirmAsync("Eliminar Grupo", $"¿Borrar '{selectedGroup.Name}'? Esta acción es irreversible.", ModalType.Warning);
            if (confirmed)
            {
                var result = await ExpenseService.DeleteGroupAsync(Guid.Parse(selectedGroup.Id));
                if (result.Succeeded)
                {
                    ToastService.ShowToast("Grupo eliminado 🗑️", ToastLevel.Success);
                    CloseDetails();
                    await LoadGroups();
                }
                else
                {
                    ToastService.ShowToast(result.Message ?? "Error al eliminar grupo", ToastLevel.Error);
                }
            }
        }

        private string GetMemberEmail(string userId)
        {
            return members?.FirstOrDefault(x => x.UserId == userId)?.Email ?? "";
        }

        private bool ShouldShowInviteButton(string userId)
        {
            var email = GetMemberEmail(userId);
            if (string.IsNullOrEmpty(email) || email == currentUserEmail) return false;
            
            var member = members?.FirstOrDefault(x => x.UserId == userId);
            if (member == null) return false;

            // Unregistered (FullName == Email)
            if (member.FullName == member.Email || string.IsNullOrEmpty(member.FullName))
                return true;

            // Registered user who owes money (can be reminded)
            var memberBreakdown = breakdown?.Members.FirstOrDefault(x => x.UserId == userId);
            if (memberBreakdown != null && memberBreakdown.NetBalance < 0)
                return true;

            return false;
        }

        private async Task CompartirEnlaceInvitacion(string userId)
        {
            var member = members?.FirstOrDefault(x => x.UserId == userId);
            if (member == null || selectedGroup == null) return;

            string mensaje;
            var memberBreakdown = breakdown?.Members.FirstOrDefault(x => x.UserId == userId);
            
            if (member.FullName == member.Email || string.IsNullOrEmpty(member.FullName))
            {
                // Unregistered user invitation
                mensaje = $"¡Hola! Te acabo de agregar al grupo '{selectedGroup.Name}' en SplitMoney para dividir gastos. " +
                           $"Regístrate con tu correo ({member.Email}) para ver las cuentas y saldar balances: https://splitmoney.app/register";
            }
            else
            {
                // Registered user reminder (if they owe money)
                if (memberBreakdown != null && memberBreakdown.NetBalance < 0)
                {
                    mensaje = $"¡Hola {member.FullName}! Te recuerdo que en el grupo '{selectedGroup.Name}' de SplitMoney " +
                               $"tenemos un saldo pendiente de {Math.Abs(memberBreakdown.NetBalance):C0}. ¡Pásate por la app para saldarlo! https://splitmoney.app";
                }
                else
                {
                    mensaje = $"¡Hola {member.FullName}! Échale un vistazo al grupo '{selectedGroup.Name}' en SplitMoney para ver los nuevos gastos: https://splitmoney.app";
                }
            }

            try
            {
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = mensaje,
                    Title = "Compartir en SplitMoney"
                });
            }
            catch (Exception)
            {
                ToastService.ShowToast("No se pudo abrir el menú de compartir", ToastLevel.Error);
            }
        }

        private string GetMemberDisplayName(string userId, string currentName)
        {
            if (!string.IsNullOrWhiteSpace(currentName) && currentName != "Usuario desconocido" && currentName != "Unknown")
            {
                return currentName;
            }

            var member = members?.FirstOrDefault(x => x.UserId == userId);
            if (member != null)
            {
                if (!string.IsNullOrWhiteSpace(member.FullName) && member.FullName != "Usuario desconocido" && member.FullName != "Unknown")
                {
                    return member.FullName;
                }
                if (!string.IsNullOrWhiteSpace(member.Email))
                {
                    return member.Email;
                }
            }

            return "Invitado";
        }
    }
}
