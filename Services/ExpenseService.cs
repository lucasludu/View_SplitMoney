using System.Net.Http.Json;
using SplitMoney.Client.Models;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Services;

public class ExpenseService : IExpenseService
{
    private readonly HttpClient _httpClient;
    private readonly IToastService _toastService;

    public ExpenseService(HttpClient httpClient, IToastService toastService)
    {
        _httpClient = httpClient;
        _toastService = toastService;
    }

    public async Task<DashboardViewModel?> GetDashboardAsync()
    {
        try 
        {
            var response = await _httpClient.GetAsync("api/v1/expenses/dashboard");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<DashboardViewModel>>();
                return result?.Data; // Devolvemos null si result?.Data es null
            }
            
            _toastService.ShowToast($"Error de servidor: {response.StatusCode}", ToastLevel.Error);
            return null;
        }
        catch (Exception ex)
        {
            _toastService.ShowToast($"Error de conexión: {ex.Message}", ToastLevel.Error);
            return null;
        }
    }

    public async Task<List<GroupSummaryViewModel>> GetUserGroupsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/groups");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<List<GroupSummaryViewModel>>>();
                return result?.Data ?? new List<GroupSummaryViewModel>();
            }
            
            return new List<GroupSummaryViewModel>();
        }
        catch
        {
            return new List<GroupSummaryViewModel>();
        }
    }

    public async Task<bool> CreateExpenseAsync(CreateExpenseModel expense)
    {
        var request = new 
        {
            Title = expense.Title,
            TotalAmount = expense.TotalAmount,
            GroupId = expense.GroupId,
            CategoryId = expense.CategoryId,
            Splits = expense.Splits.Select(s => new { UserId = s.UserId, SplitType = (int)expense.SelectedSplitType, SplitValue = s.Amount }).ToList(),
            Payments = expense.Payments.Select(p => new { UserId = p.UserId, AmountPaid = p.Amount }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("api/v1/expenses", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateGroupAsync(string name, List<MemberSpendRecordViewModel> initialMembers)
    {
        var request = new 
        { 
            Name = name, 
            InitialMembers = initialMembers.Select(m => new { Email = m.Email, AmountSpent = m.AmountSpent }).ToList() 
        };
        var response = await _httpClient.PostAsJsonAsync("api/v1/groups", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<GroupMemberResponse>> GetGroupMembersAsync(string groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/groups/{groupId}/members");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<List<GroupMemberResponse>>>();
                return result?.Data ?? new List<GroupMemberResponse>();
            }
            return new List<GroupMemberResponse>();
        }
        catch
        {
            return new List<GroupMemberResponse>();
        }
    }

    public async Task<List<BalanceResponse>> GetGroupBalancesAsync(string groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/{groupId}/balances");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<List<BalanceResponse>>>();
                return result?.Data ?? new List<BalanceResponse>();
            }
            return new List<BalanceResponse>();
        }
        catch
        {
            return new List<BalanceResponse>();
        }
    }

    public async Task<GroupSpendingBreakdownViewModel?> GetGroupSpendingBreakdownAsync(string groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/groups/{groupId}/breakdown");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<GroupSpendingBreakdownViewModel>>();
                return result?.Data;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ExpenseDetailViewModel?> GetExpenseDetailsAsync(Guid expenseId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/{expenseId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<ExpenseDetailViewModel>>();
                return result?.Data;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SettleDebtAsync(SettleDebtModel settlement)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/groups/settle", settlement);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateExpenseAsync(Guid id, CreateExpenseModel expense)
    {
        var request = new 
        {
            Id = id,
            Title = expense.Title,
            TotalAmount = expense.TotalAmount,
            GroupId = expense.GroupId,
            CategoryId = expense.CategoryId,
            Splits = expense.Splits.Select(s => new { UserId = s.UserId, SplitType = (int)expense.SelectedSplitType, SplitValue = s.Amount }).ToList()
        };

        var response = await _httpClient.PutAsJsonAsync($"api/v1/expenses/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteExpenseAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/expenses/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/categories");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<List<CategoryDto>>>();
                return result?.Data ?? new List<CategoryDto>();
            }
            return new List<CategoryDto>();
        }
        catch
        {
            return new List<CategoryDto>();
        }
    }

    public async Task<ExpenseAuditViewModel?> GetExpenseAuditAsync(Guid expenseId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/{expenseId}/audit");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<ExpenseAuditViewModel>>();
                return result?.Data;
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<GroupSpendingSummaryViewModel?> GetGroupSpendingSummaryAsync(Guid groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/groups/{groupId}/summary");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<GroupSpendingSummaryViewModel>>();
                return result?.Data;
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<byte[]?> ExportGroupReportAsync(Guid groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/groups/{groupId}/export");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }
        catch { return null; }
    }
}
