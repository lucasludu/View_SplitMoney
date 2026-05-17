using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;

namespace SplitMoney.Client.Components.Pages
{
    public partial class Login
    {
        [Inject] public IAuthService AuthService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        private LoginRequest loginModel = new LoginRequest();
        private string? errorMessage;
        private bool loading = false;
        private bool showPassword = false;

        private async Task HandleLogin()
        {
            loading = true;
            errorMessage = null;

            try
            {
                var result = await AuthService.Login(loginModel);

                if (result.Succeeded)
                {
                    Navigation.NavigateTo("/");
                }
                else
                {
                    errorMessage = result.Message ?? "Login failed.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message} {(ex.InnerException != null ? " -> " + ex.InnerException.Message : "")}";
            }
            finally
            {
                loading = false;
            }
        }
    }
}
