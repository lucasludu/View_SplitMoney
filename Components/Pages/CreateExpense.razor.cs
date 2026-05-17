using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Pages
{
    public partial class CreateExpense
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public IModalService ModalService { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] public IAuthService AuthService { get; set; } = default!;

        private CreateExpenseModel expense = new() { Title = "", TotalAmount = 0 };
        private List<GroupSummaryViewModel> userGroups = new();
        private List<CategoryDto> categories = new();
        private List<GroupMemberResponse> currentGroupMembers = new();
        private HashSet<string> selectedSplits = new();
        private List<ExpensePaymentViewModel> availableSplits = new(); 
        private bool loadingGroups = true;
        private bool submitting = false;
        private bool isPremium = false;
        private bool showPaywall = false;
        private string paywallTitle = "Repartos Pro";

        private void TrySetPremiumSplit(SplitType type)
        {
            if (isPremium) SetSplitType(type);
            else { paywallTitle = "Repartos Pro"; showPaywall = true; }
        }

        private void HandleUpgrade() => Navigation.NavigateTo("/premium");

        private async Task HandleSimulatePremium() 
        {
            await AuthService.SimulatePremiumAsync();
            isPremium = true;
            StateHasChanged();
        }

        private bool isCategoryDropdownOpen = false;
        private CategoryDto? SelectedCategory => categories.FirstOrDefault(c => c.Id == expense.CategoryId);

        private void ToggleCategoryDropdown() => isCategoryDropdownOpen = !isCategoryDropdownOpen;
        private void SelectCategory(Guid categoryId) { expense.CategoryId = categoryId; isCategoryDropdownOpen = false; }
        private void CloseDropdowns() { if (isCategoryDropdownOpen) isCategoryDropdownOpen = false; }

        [Parameter] public Guid? ExpenseId { get; set; }
        [Parameter] [SupplyParameterFromQuery] public string? GroupId { get; set; }

        private string _selectedGroupId = "";
        private string SelectedGroupId 
        { 
            get => _selectedGroupId; 
            set {
                if (_selectedGroupId != value) {
                    _selectedGroupId = value;
                    _ = LoadGroupMembers();
                }
            } 
        }

        private decimal AssignedTotal => availableSplits.Where(s => selectedSplits.Contains(s.UserId)).Sum(s => s.Amount);
        private decimal TotalPercentage => availableSplits.Where(s => selectedSplits.Contains(s.UserId)).Sum(s => s.Amount);

        private bool IsSplitBalanced {
            get {
                if (expense.SelectedSplitType == SplitType.Equal) return true;
                if (expense.SelectedSplitType == SplitType.Exact) return Math.Abs(AssignedTotal - expense.TotalAmount) < 0.01m;
                if (expense.SelectedSplitType == SplitType.Percentage) return Math.Abs(TotalPercentage - 100) < 0.1m;
                return false;
            }
        }

        private bool IsPaymentBalanced => Math.Abs(expense.Payments.Sum(p => p.Amount) - expense.TotalAmount) < 0.01m;
        private bool CanSubmit => IsPaymentBalanced && IsSplitBalanced && expense.TotalAmount > 0 && !string.IsNullOrWhiteSpace(expense.Title) && selectedSplits.Any();

        private void SetSplitType(SplitType type)
        {
            expense.SelectedSplitType = type;
            if (type == SplitType.Equal) foreach (var s in availableSplits) s.Amount = 0;
        }

        protected override async Task OnInitializedAsync()
        {
            isPremium = await AuthService.IsPremiumAsync();

            var groupsResult = await ExpenseService.GetUserGroupsAsync();
            userGroups = groupsResult.Succeeded && groupsResult.Data != null ? groupsResult.Data : new List<GroupSummaryViewModel>();
            
            var catResult = await ExpenseService.GetCategoriesAsync();
            categories = catResult.Succeeded && catResult.Data != null ? catResult.Data : new List<CategoryDto>();

            if (ExpenseId.HasValue)
            {
                var detailResult = await ExpenseService.GetExpenseDetailsAsync(ExpenseId.Value);
                if (detailResult.Succeeded && detailResult.Data != null)
                {
                    var detail = detailResult.Data;
                    expense.Title = detail.Description;
                    expense.TotalAmount = detail.TotalAmount;
                    expense.Date = detail.Date;
                    expense.CategoryId = categories.FirstOrDefault(c => c.IconIdentifier == detail.CategoryIcon)?.Id;
                    var group = userGroups.FirstOrDefault(g => g.Name == detail.GroupName);
                    if (group != null) SelectedGroupId = group.Id;
                    await LoadGroupMembers();
                    foreach (var p in expense.Payments) {
                        var detailPayment = detail.Payments.FirstOrDefault(dp => dp.UserName == p.UserName);
                        if (detailPayment != null) p.Amount = detailPayment.Amount;
                    }
                    expense.SelectedSplitType = detail.SplitType;
                    selectedSplits.Clear();
                    foreach (var ds in detail.Splits) {
                        var member = currentGroupMembers.FirstOrDefault(m => m.FullName == ds.UserName);
                        if (member != null) {
                            selectedSplits.Add(member.UserId);
                            var splitInput = availableSplits.FirstOrDefault(s => s.UserId == member.UserId);
                            if (splitInput != null) splitInput.Amount = ds.AmountOwed;
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(GroupId) && userGroups.Any(g => g.Id == GroupId)) SelectedGroupId = GroupId;
            else if (userGroups.Any()) SelectedGroupId = userGroups.First().Id;
            loadingGroups = false;
        }

        private async Task LoadGroupMembers()
        {
            if (string.IsNullOrEmpty(SelectedGroupId)) return;
            var membersResult = await ExpenseService.GetGroupMembersAsync(SelectedGroupId);
            currentGroupMembers = membersResult.Succeeded && membersResult.Data != null ? membersResult.Data : new List<GroupMemberResponse>();
            expense.Payments = currentGroupMembers.Select(m => new ExpensePaymentViewModel { UserId = m.UserId, UserName = m.FullName, Amount = 0 }).ToList();
            availableSplits = currentGroupMembers.Select(m => new ExpensePaymentViewModel { UserId = m.UserId, UserName = m.FullName }).ToList();
            selectedSplits = new HashSet<string>(currentGroupMembers.Select(m => m.UserId));
            expense.GroupId = SelectedGroupId;
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var selfId = authState.User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (selfId != null && expense.TotalAmount > 0) {
                var selfPayment = expense.Payments.FirstOrDefault(p => p.UserId == selfId);
                if (selfPayment != null) selfPayment.Amount = expense.TotalAmount;
            }
            StateHasChanged();
        }

        private void ToggleSplit(string userId) { if (selectedSplits.Contains(userId)) selectedSplits.Remove(userId); else selectedSplits.Add(userId); }
        private void ToggleAllSplits() { if (selectedSplits.Count == currentGroupMembers.Count) selectedSplits.Clear(); else selectedSplits = new HashSet<string>(currentGroupMembers.Select(m => m.UserId)); }
        private void GoBack() => Navigation.NavigateTo("/");

        private async Task HandleSubmit()
        {
            if (!CanSubmit) return;
            submitting = true;
            if (expense.SelectedSplitType == SplitType.Equal) {
                var splitVal = Math.Round(expense.TotalAmount / selectedSplits.Count, 2);
                expense.Splits = selectedSplits.Select(uid => new ExpenseSplitViewModel { UserId = uid, Amount = splitVal, SplitType = SplitType.Equal }).ToList();
            } else {
                expense.Splits = availableSplits.Where(s => selectedSplits.Contains(s.UserId)).Select(s => new ExpenseSplitViewModel { UserId = s.UserId, Amount = s.Amount, SplitType = expense.SelectedSplitType }).ToList();
            }
            var finalPayments = expense.Payments.Where(p => p.Amount > 0).ToList();
            var originalPayments = expense.Payments;
            expense.Payments = finalPayments;
            
            ApiResult result;
            if (ExpenseId.HasValue) result = await ExpenseService.UpdateExpenseAsync(ExpenseId.Value, expense);
            else result = await ExpenseService.CreateExpenseAsync(expense);
            
            submitting = false;
            if (result.Succeeded) { ToastService.ShowToast("¡Gasto guardado! 🚀", ToastLevel.Success); Navigation.NavigateTo("/"); }
            else { expense.Payments = originalPayments; ModalService.ShowModal("Error", result.Message ?? "No se pudo guardar el gasto.", ModalType.Error); }
        }
    }
}
