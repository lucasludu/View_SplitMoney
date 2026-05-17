using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;

namespace SplitMoney.Client.Components.Pages
{
    public partial class ResetPassword
    {
        [Inject] public IAuthService AuthService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;

        private ResetPasswordRequest resetModel = new();
        private bool loading = false;
        private string errorMessage = "";

        private async Task HandleSubmit()
        {
            loading = true;
            errorMessage = "";
            
            var result = await AuthService.ResetPassword(resetModel);
            
            if (result.Succeeded)
            {
                ToastService.ShowToast("¡Contraseña restablecida! 🚀", ToastLevel.Success);
                Navigation.NavigateTo("/login");
            }
            else
            {
                errorMessage = result.Message ?? "No pudimos restablecer tu contraseña. Verifica el código.";
            }
            
            loading = false;
        }
    }
}
