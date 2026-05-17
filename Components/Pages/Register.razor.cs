using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;

namespace SplitMoney.Client.Components.Pages
{
    public partial class Register
    {
        [Inject] public IAuthService AuthService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        private RegisterUserRequest registerModel = new RegisterUserRequest();
        private string? errorMessage;
        private List<string> errors = new List<string>();
        private bool loading = false;
        private bool showPassword = false;

        private async Task HandleRegister()
        {
            loading = true;
            errorMessage = null;
            errors.Clear();

            try
            {
                var result = await AuthService.Register(registerModel);

                if (result.Succeeded)
                {
                    Navigation.NavigateTo("/login");
                }
                else
                {
                    errorMessage = result.Message ?? "Registration failed.";
                    if (result.Errors != null)
                    {
                        errors = result.Errors;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                loading = false;
            }
        }
    }
}
