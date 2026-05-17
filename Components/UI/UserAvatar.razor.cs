using Microsoft.AspNetCore.Components;

namespace SplitMoney.Client.Components.UI
{
    public partial class UserAvatar
    {
        [Parameter] public string? Name { get; set; }
        [Parameter] public string Class { get; set; } = string.Empty;
        [Parameter] public string Style { get; set; } = string.Empty;

        private string Initials => 
            (!string.IsNullOrEmpty(Name) ? Name[0].ToString() : "U").ToUpper();
    }
}
