using Microsoft.AspNetCore.Components;

namespace SplitMoney.Client.Components
{
    public partial class RedirectToLogin
    {
        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        protected override void OnInitialized()
        {
            Navigation.NavigateTo("/login");
        }
    }
}
