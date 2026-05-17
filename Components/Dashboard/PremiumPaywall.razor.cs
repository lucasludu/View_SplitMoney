using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SplitMoney.Client.Components.Dashboard
{
    public partial class PremiumPaywall
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter] public string Title { get; set; } = "Límite Alcanzado";
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnUpgrade { get; set; }
        [Parameter] public EventCallback OnSimulate { get; set; }

        private async Task HandleUpgrade()
        {
            await OnUpgrade.InvokeAsync();
            await OnClose.InvokeAsync();
        }

        private async Task HandleSimulate()
        {
            await OnSimulate.InvokeAsync();
            await OnClose.InvokeAsync();
        }
    }
}
