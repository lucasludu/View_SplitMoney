using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Dashboard
{
    public partial class MonthlyTipModal
    {
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public TipViewModel? Tip { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
    }
}
