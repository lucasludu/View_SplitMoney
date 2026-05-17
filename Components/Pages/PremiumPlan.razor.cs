using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;

namespace SplitMoney.Client.Components.Pages
{
    public partial class PremiumPlan
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; } = default!;

        private async Task HandleUpgrade()
        {
            // En un escenario real, aquí se llamaría al SDK de Google Play
            ToastService.ShowToast("Integración con Play Store pendiente: usa el botón de simulación abajo.", ToastLevel.Info);
        }

        private async Task SimulateSuccess()
        {
            await LocalStorage.SetItemAsync("is_simulated_premium", true);
            ToastService.ShowToast("¡Bienvenido al nivel Premium! 🌟", ToastLevel.Success);
            
            // Efecto de confeti o espera
            await Task.Delay(1500);
            Navigation.NavigateTo("/");
        }
    }
}
