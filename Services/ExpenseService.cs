using System.Net.Http.Json;
using SplitMoney.Client.Models;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Services;

public class ExpenseService : IExpenseService
{
    private readonly HttpClient _httpClient;
    private readonly IToastService _toastService;
    private readonly ICacheService _cacheService;

    private const string DASHBOARD_KEY = "dashboard_data";
    private const string GROUPS_KEY = "user_groups_data";
    private const string CATEGORIES_KEY = "expense_categories_data";

    public ExpenseService(HttpClient httpClient, IToastService toastService, ICacheService cacheService)
    {
        _httpClient = httpClient;
        _toastService = toastService;
        _cacheService = cacheService;
    }

    public async Task<DashboardViewModel?> GetDashboardAsync()
    {
        var cached = await _cacheService.GetAsync<DashboardViewModel>(DASHBOARD_KEY);
        if (cached != null) return cached;

        var response = await _httpClient.GetAsync("api/v1/expenses/dashboard");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<Response<DashboardViewModel>>();
            if (result?.Data != null)
            {
                await _cacheService.SetAsync(DASHBOARD_KEY, result.Data, TimeSpan.FromMinutes(2));
                return result.Data;
            }
        }
        
        return null;
    }

    public async Task<List<GroupSummaryViewModel>> GetUserGroupsAsync()
    {
        try
        {
            var cached = await _cacheService.GetAsync<List<GroupSummaryViewModel>>(GROUPS_KEY);
            if (cached != null) return cached;

            var response = await _httpClient.GetAsync("api/v1/groups");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<List<GroupSummaryViewModel>>>();
                var data = result?.Data ?? new List<GroupSummaryViewModel>();
                if (data.Any())
                {
                    await _cacheService.SetAsync(GROUPS_KEY, data, TimeSpan.FromMinutes(5));
                }
                return data;
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
            Currency = expense.Currency,
            Date = expense.Date,
            Splits = expense.Splits.Select(s => new { UserId = s.UserId, SplitType = (int)expense.SelectedSplitType, SplitValue = s.Amount }).ToList(),
            Payments = expense.Payments.Select(p => new { UserId = p.UserId, AmountPaid = p.Amount }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("api/v1/expenses", request);
        
        if (response.IsSuccessStatusCode)
        {
            await InvalidateMainCache();
        }
        
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
        if (response.IsSuccessStatusCode) await InvalidateMainCache();
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
        var wrap = new { request = settlement };
        var response = await _httpClient.PostAsJsonAsync("api/v1/groups/settle", wrap);
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
            Currency = expense.Currency,
            Date = expense.Date,
            Splits = expense.Splits.Select(s => new { UserId = s.UserId, SplitType = (int)expense.SelectedSplitType, SplitValue = s.Amount }).ToList(),
            Payments = expense.Payments.Select(p => new { UserId = p.UserId, AmountPaid = p.Amount }).ToList()
        };

        var response = await _httpClient.PutAsJsonAsync($"api/v1/expenses/{id}", request);
        if (response.IsSuccessStatusCode) await InvalidateMainCache();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteExpenseAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/expenses/{id}");
        if (response.IsSuccessStatusCode) await InvalidateMainCache();
        return response.IsSuccessStatusCode;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            var cached = await _cacheService.GetAsync<List<CategoryDto>>(CATEGORIES_KEY);
            if (cached != null) return cached;

            var response = await _httpClient.GetAsync("api/v1/categories");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<List<CategoryDto>>>();
                var data = result?.Data ?? new List<CategoryDto>();
                if (data.Any())
                {
                    await _cacheService.SetAsync(CATEGORIES_KEY, data, TimeSpan.FromHours(24), persist: true);
                }
                return data;
            }
            return new List<CategoryDto>();
        }
        catch
        {
            return new List<CategoryDto>();
        }
    }

    private async Task InvalidateMainCache()
    {
        await _cacheService.RemoveAsync(DASHBOARD_KEY);
        await _cacheService.RemoveAsync(GROUPS_KEY);
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

    public async Task<bool> UpdateGroupAsync(Guid id, string name, List<MemberSpendRecordViewModel> members)
    {
        var request = new 
        { 
            Id = id,
            Name = name, 
            Members = members.Select(m => new { Email = m.Email }).ToList()
        };
        var response = await _httpClient.PutAsJsonAsync($"api/v1/groups/{id}", request);
        if (response.IsSuccessStatusCode) await InvalidateMainCache();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteGroupAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/groups/{id}");
            if (response.IsSuccessStatusCode) await InvalidateMainCache();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SettlementViewModel>> GetMySettlementsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/user/me/settlements");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Response<List<SettlementViewModel>>>();
                return result?.Data ?? new List<SettlementViewModel>();
            }
            return new List<SettlementViewModel>();
        }
        catch
        {
            return new List<SettlementViewModel>();
        }
    }
}
