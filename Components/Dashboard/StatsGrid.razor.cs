using Microsoft.AspNetCore.Components;

namespace SplitMoney.Client.Components.Dashboard
{
    public partial class StatsGrid
    {
        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Parameter] public decimal TeDeben { get; set; }
        [Parameter] public decimal Debes { get; set; }
        [Parameter] public decimal TotalMes { get; set; }

        private void NavigateToSettle() => Navigation.NavigateTo("/settle");
    }
}
