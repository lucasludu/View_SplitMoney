using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Pages
{
    public partial class ExpenseDetail
    {
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public IModalService ModalService { get; set; } = default!;

        [Parameter] public Guid Id { get; set; }
        private ExpenseDetailViewModel? expense;
        private bool isLoading = true;
        private bool isDeleting = false;

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            try 
            { 
                var result = await ExpenseService.GetExpenseDetailsAsync(Id); 
                if (result.Succeeded) expense = result.Data;
                else ToastService.ShowToast(result.Message ?? "Error al cargar detalle", ToastLevel.Error);
            }
            finally { isLoading = false; }
        }

        private void GoBack() => Navigation.NavigateTo("/expenses");
        private void NavigateToEdit() => Navigation.NavigateTo($"/expenses/edit/{Id}");

        private async Task ConfirmDelete()
        {
            if (expense == null) return;
            
            bool confirmed = await ModalService.ShowConfirmAsync("Eliminar Gasto", $"¿Estás seguro de que deseas borrar '{expense.Description}'? Esta acción no se puede deshacer.", ModalType.Warning);
            
            if (confirmed)
            {
                isDeleting = true;
                var result = await ExpenseService.DeleteExpenseAsync(Id);
                isDeleting = false;
                
                if (result.Succeeded) 
                { 
                    ToastService.ShowToast("Gasto eliminado 🗑️", ToastLevel.Success); 
                    Navigation.NavigateTo("/expenses"); 
                }
                else 
                { 
                    ToastService.ShowToast(result.Message ?? "No se pudo eliminar", ToastLevel.Error); 
                }
            }
        }

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
