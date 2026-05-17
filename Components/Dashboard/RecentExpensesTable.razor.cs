using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Dashboard
{
    public partial class RecentExpensesTable
    {
        [Parameter] public IEnumerable<RecentExpenseViewModel>? Expenses { get; set; }
        [Parameter] public EventCallback<Guid> OnExpenseSelected { get; set; }

        private string GetCategoryIcon(string currentIcon)
        {
            return currentIcon switch
            {
                "🛒" => "shopping_cart",
                "🍔" => "restaurant",
                "🏠" => "home",
                "🚕" => "local_taxi",
                "🎉" => "celebration",
                "💡" => "lightbulb",
                _ => "receipt_long"
            };
        }
    }
}
