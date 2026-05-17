using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Pages
{
    public partial class Expenses
    {
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;

        private List<RecentExpenseViewModel>? expenses;
        private bool isLoading = true;
        private string searchTerm = "";

        private IEnumerable<RecentExpenseViewModel> FilteredExpenses => 
            expenses?.Where(e => string.IsNullOrEmpty(searchTerm) || e.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<RecentExpenseViewModel>();

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            try
            {
                var result = await ExpenseService.GetDashboardAsync();
                if (result.Succeeded)
                {
                    expenses = result.Data?.RecentExpenses?.ToList();
                }
            }
            finally { isLoading = false; }
        }

        private void GoBack() => Navigation.NavigateTo("/");
        private void NavigateToDetail(Guid id) => Navigation.NavigateTo($"/expenses/detail/{id}");

        private string GetCategoryIcon(string currentIcon) => currentIcon switch
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
