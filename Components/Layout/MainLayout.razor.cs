using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SplitMoney.Client.Components.Layout
{
    public partial class MainLayout
    {
        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        private ErrorBoundary? errorBoundary;

        protected override void OnParametersSet()
        {
            // On navigation, recover from previous errors
            errorBoundary?.Recover();
        }

        private void Recover()
        {
            errorBoundary?.Recover();
        }

        private void ReloadPage()
        {
            Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
        }
    }
}
