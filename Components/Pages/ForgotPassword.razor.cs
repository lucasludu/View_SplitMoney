using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;

namespace SplitMoney.Client.Components.Pages
{
    public partial class ForgotPassword
    {
        [Inject] public IAuthService AuthService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;

        private ForgotPasswordRequest forgotModel = new();
        private bool loading = false;
        private string errorMessage = "";
        private bool emailSent = false;

        private async Task HandleSubmit()
        {
            loading = true;
            errorMessage = "";
            
            var result = await AuthService.ForgotPassword(forgotModel.Email);
            
            if (result.Succeeded)
            {
                emailSent = true;
                ToastService.ShowToast("¡Correo enviado con éxito! 📧", ToastLevel.Success);
            }
            else
            {
                errorMessage = result.Message ?? "No pudimos procesar tu solicitud en este momento.";
            }
            
            loading = false;
        }
    }
}
