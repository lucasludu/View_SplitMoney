using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Layout
{
    public partial class NavMenu
    {
        [Inject] public IAuthService AuthService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] public Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public IModalService ModalService { get; set; } = default!;
        [Inject] public INotificationService NotificationService { get; set; } = default!;

        private bool collapseNavMenu = true;
        private string userName = "User";
        private bool isPremium = false;
        private bool isSimulatedPremium = false;
        private bool isRealPremium = false;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            
            // Check real and simulated premium status
            isRealPremium = user.IsInRole("PremiumUser");
            isSimulatedPremium = await LocalStorage.GetItemAsync<bool>("is_simulated_premium");
            isPremium = isRealPremium || isSimulatedPremium;

            if (user.Identity?.IsAuthenticated == true)
            {
                var name = user.Claims.FirstOrDefault(c => c.Type == "FirstName" || c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(name))
                    userName = name;
                
                // Removed notification refresh from here
            }
        }

        private void CloseAllOverlays()
        {
            collapseNavMenu = true;
            StateHasChanged();
        }

        private void ToggleNavMenu() 
        {
            collapseNavMenu = !collapseNavMenu;
            StateHasChanged();
        }

        private void CloseMenu() 
        {
            collapseNavMenu = true;
            StateHasChanged();
        }

        private async Task DisableSimulation()
        {
            await LocalStorage.RemoveItemAsync("is_simulated_premium");
            isSimulatedPremium = false;
            isPremium = isRealPremium;
            StateHasChanged();
            // Force reload to update other components
            Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
        }

        private async Task HandleLogout()
        {
            await AuthService.Logout();
            Navigation.NavigateTo("/login");
        }

        private async Task HandleCreateExpenseNav()
        {
            CloseMenu();
            var groupsResult = await ExpenseService.GetUserGroupsAsync();
            var groups = groupsResult.Succeeded ? groupsResult.Data : new List<GroupSummaryViewModel>();
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
    }
}
