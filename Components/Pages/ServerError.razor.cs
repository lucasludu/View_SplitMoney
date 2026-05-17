using System;
using Microsoft.AspNetCore.Components;

namespace SplitMoney.Client.Components.Pages
{
    public partial class ServerError
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        private void GoBack() => Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
    }
}
