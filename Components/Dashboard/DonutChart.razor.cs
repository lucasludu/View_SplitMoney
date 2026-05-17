using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Dashboard
{
    public partial class DonutChart
    {
        [Parameter]
        public List<CategorySpendingViewModel> Data { get; set; } = new();

        [Parameter]
        public decimal Total { get; set; }

        [Parameter]
        public int Size { get; set; } = 220;
    }
}
