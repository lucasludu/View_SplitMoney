using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;

namespace SplitMoney.Client.Components.Pages
{
    public partial class Profile
    {
        [Inject] public IAuthService AuthService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IModalService ModalService { get; set; } = default!;

        private UserDto userProfile = new();
        private bool isLoading = true;
        private bool submitting = false;
        private bool IsUserPremium = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsUserPremium = await AuthService.IsPremiumAsync();

                var response = await AuthService.GetProfile();
                if (response != null && response.Succeeded && response.Data != null)
                {
                    userProfile = response.Data;
                }
                else Navigation.NavigateTo("/");
            }
            catch (Exception)
            {
                if (IsUserPremium) userProfile.Role = "PremiumUser";
                isLoading = false;
            }
            finally { isLoading = false; }
        }

        private async Task HandleUpdateProfile()
        {
            submitting = true;
            try
            {
                var response = await AuthService.UpdateProfile(userProfile);
                if (response != null && response.Succeeded) 
                    ModalService.ShowModal("¡Perfil Actualizado!", "Tus cambios han sido guardados correctamente.", ModalType.Success);
                else 
                    ModalService.ShowModal("Error", response?.Message ?? "No se pudo actualizar.", ModalType.Error);
            }
            finally { submitting = false; }
        }

        private void GoBack() => Navigation.NavigateTo("/");
    }
}
