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

    public async Task<ApiResult<DashboardViewModel>> GetDashboardAsync()
    {
        try
        {
            var cached = await _cacheService.GetAsync<DashboardViewModel>(DASHBOARD_KEY);
            if (cached != null) return ApiResult<DashboardViewModel>.Success(cached);

            var response = await _httpClient.GetAsync("api/v1/expenses/dashboard");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<DashboardViewModel>>();
                if (result != null && result.Succeeded && result.Data != null)
                {
                    await _cacheService.SetAsync(DASHBOARD_KEY, result.Data, TimeSpan.FromMinutes(2));
                    return result;
                }
                return result ?? ApiResult<DashboardViewModel>.Failure("Respuesta inválida del servidor.");
            }
            return ApiResult<DashboardViewModel>.Failure($"Error del servidor: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResult<DashboardViewModel>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResult<List<GroupSummaryViewModel>>> GetUserGroupsAsync()
    {
        try
        {
            var cached = await _cacheService.GetAsync<List<GroupSummaryViewModel>>(GROUPS_KEY);
            if (cached != null) return ApiResult<List<GroupSummaryViewModel>>.Success(cached);

            var response = await _httpClient.GetAsync("api/v1/groups");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<List<GroupSummaryViewModel>>>();
                if (result != null && result.Succeeded && result.Data != null)
                {
                    if (result.Data.Any())
                    {
                        await _cacheService.SetAsync(GROUPS_KEY, result.Data, TimeSpan.FromMinutes(5));
                    }
                    return result;
                }
                return result ?? ApiResult<List<GroupSummaryViewModel>>.Failure("Error al obtener grupos.");
            }
            return ApiResult<List<GroupSummaryViewModel>>.Failure($"Error del servidor: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResult<List<GroupSummaryViewModel>>.Failure($"Error de conexión: {ex.Message}");
        }
    }

    public async Task<ApiResult> CreateExpenseAsync(CreateExpenseModel expense)
    {
        try 
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
                return ApiResult.Success();
            }
            return ApiResult.Failure("Error al crear el gasto.");
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<ApiResult> CreateGroupAsync(string name, List<MemberSpendRecordViewModel> initialMembers)
    {
        try 
        {
            var request = new 
            { 
                Name = name, 
                InitialMembers = initialMembers.Select(m => new { Email = m.Email, AmountSpent = m.AmountSpent }).ToList() 
            };
            var response = await _httpClient.PostAsJsonAsync("api/v1/groups", request);
            
            if (response.IsSuccessStatusCode) 
            {
                await InvalidateMainCache();
                return ApiResult.Success();
            }
            return ApiResult.Failure("Error al crear el grupo.");
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<ApiResult<List<GroupMemberResponse>>> GetGroupMembersAsync(string groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/groups/{groupId}/members");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<List<GroupMemberResponse>>>();
                return result ?? ApiResult<List<GroupMemberResponse>>.Failure("Error al obtener miembros.");
            }
            return ApiResult<List<GroupMemberResponse>>.Failure("Error de servidor.");
        }
        catch (Exception ex)
        {
            return ApiResult<List<GroupMemberResponse>>.Failure(ex.Message);
        }
    }

    public async Task<ApiResult<List<BalanceResponse>>> GetGroupBalancesAsync(string groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/{groupId}/balances");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<List<BalanceResponse>>>();
                return result ?? ApiResult<List<BalanceResponse>>.Failure("Error al obtener balances.");
            }
            return ApiResult<List<BalanceResponse>>.Failure("Error de servidor.");
        }
        catch (Exception ex)
        {
            return ApiResult<List<BalanceResponse>>.Failure(ex.Message);
        }
    }

    public async Task<ApiResult<GroupSpendingBreakdownViewModel>> GetGroupSpendingBreakdownAsync(string groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/groups/{groupId}/breakdown");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<GroupSpendingBreakdownViewModel>>();
                return result ?? ApiResult<GroupSpendingBreakdownViewModel>.Failure("Error al obtener desglose.");
            }
            return ApiResult<GroupSpendingBreakdownViewModel>.Failure("Error de servidor.");
        }
        catch (Exception ex)
        {
            return ApiResult<GroupSpendingBreakdownViewModel>.Failure(ex.Message);
        }
    }

    public async Task<ApiResult<ExpenseDetailViewModel>> GetExpenseDetailsAsync(Guid expenseId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/{expenseId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<ExpenseDetailViewModel>>();
                return result ?? ApiResult<ExpenseDetailViewModel>.Failure("Error al obtener detalles.");
            }
            return ApiResult<ExpenseDetailViewModel>.Failure("Gasto no encontrado.");
        }
        catch (Exception ex)
        {
            return ApiResult<ExpenseDetailViewModel>.Failure(ex.Message);
        }
    }

    public async Task<ApiResult> SettleDebtAsync(SettleDebtModel settlement)
    {
        try 
        {
            var wrap = new { request = settlement };
            var response = await _httpClient.PostAsJsonAsync("api/v1/groups/settle", wrap);
            if (response.IsSuccessStatusCode) return ApiResult.Success();
            return ApiResult.Failure("Error al saldar la deuda.");
        }
        catch (Exception ex) { return ApiResult.Failure(ex.Message); }
    }

    public async Task<ApiResult> UpdateExpenseAsync(Guid id, CreateExpenseModel expense)
    {
        try 
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
            if (response.IsSuccessStatusCode) 
            {
                await InvalidateMainCache();
                return ApiResult.Success();
            }
            return ApiResult.Failure("Error al actualizar gasto.");
        }
        catch (Exception ex) { return ApiResult.Failure(ex.Message); }
    }

    public async Task<ApiResult> DeleteExpenseAsync(Guid id)
    {
        try 
        {
            var response = await _httpClient.DeleteAsync($"api/v1/expenses/{id}");
            if (response.IsSuccessStatusCode) 
            {
                await InvalidateMainCache();
                return ApiResult.Success();
            }
            return ApiResult.Failure("Error al eliminar gasto.");
        }
        catch (Exception ex) { return ApiResult.Failure(ex.Message); }
    }

    public async Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync()
    {
        try
        {
            var cached = await _cacheService.GetAsync<List<CategoryDto>>(CATEGORIES_KEY);
            if (cached != null) return ApiResult<List<CategoryDto>>.Success(cached);

            var response = await _httpClient.GetAsync("api/v1/categories");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<List<CategoryDto>>>();
                if (result != null && result.Succeeded && result.Data != null)
                {
                    if (result.Data.Any())
                    {
                        await _cacheService.SetAsync(CATEGORIES_KEY, result.Data, TimeSpan.FromHours(24), persist: true);
                    }
                    return result;
                }
                return ApiResult<List<CategoryDto>>.Failure("Error al obtener categorías.");
            }
            return ApiResult<List<CategoryDto>>.Failure("Error de servidor.");
        }
        catch (Exception ex)
        {
            return ApiResult<List<CategoryDto>>.Failure(ex.Message);
        }
    }

    private async Task InvalidateMainCache()
    {
        await _cacheService.RemoveAsync(DASHBOARD_KEY);
        await _cacheService.RemoveAsync(GROUPS_KEY);
    }

    public async Task<ApiResult<ExpenseAuditViewModel>> GetExpenseAuditAsync(Guid expenseId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/{expenseId}/audit");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<ExpenseAuditViewModel>>();
                return result ?? ApiResult<ExpenseAuditViewModel>.Failure("Error al obtener auditoría.");
            }
            return ApiResult<ExpenseAuditViewModel>.Failure("Error de servidor.");
        }
        catch (Exception ex) { return ApiResult<ExpenseAuditViewModel>.Failure(ex.Message); }
    }

    public async Task<ApiResult<GroupSpendingSummaryViewModel>> GetGroupSpendingSummaryAsync(Guid groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/groups/{groupId}/summary");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<GroupSpendingSummaryViewModel>>();
                return result ?? ApiResult<GroupSpendingSummaryViewModel>.Failure("Error al obtener resumen.");
            }
            return ApiResult<GroupSpendingSummaryViewModel>.Failure("Error de servidor.");
        }
        catch (Exception ex) { return ApiResult<GroupSpendingSummaryViewModel>.Failure(ex.Message); }
    }

    public async Task<ApiResult<byte[]>> ExportGroupReportAsync(Guid groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/expenses/groups/{groupId}/export");
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return ApiResult<byte[]>.Success(bytes);
            }
            return ApiResult<byte[]>.Failure("Error al exportar reporte.");
        }
        catch (Exception ex) { return ApiResult<byte[]>.Failure(ex.Message); }
    }

    public async Task<ApiResult> UpdateGroupAsync(Guid id, string name, List<MemberSpendRecordViewModel> members)
    {
        try 
        {
            var request = new 
            { 
                Id = id,
                Name = name, 
                Members = members.Select(m => new { Email = m.Email }).ToList()
            };
            var response = await _httpClient.PutAsJsonAsync($"api/v1/groups/{id}", request);
            if (response.IsSuccessStatusCode) 
            {
                await InvalidateMainCache();
                return ApiResult.Success();
            }
            return ApiResult.Failure("Error al actualizar grupo.");
        }
        catch (Exception ex) { return ApiResult.Failure(ex.Message); }
    }

    public async Task<ApiResult> DeleteGroupAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/groups/{id}");
            if (response.IsSuccessStatusCode) 
            {
                await InvalidateMainCache();
                return ApiResult.Success();
            }
            return ApiResult.Failure("Error al eliminar grupo.");
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<ApiResult<List<SettlementViewModel>>> GetMySettlementsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/user/me/settlements");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<List<SettlementViewModel>>>();
                return result ?? ApiResult<List<SettlementViewModel>>.Failure("Error al obtener deudas.");
            }
            return ApiResult<List<SettlementViewModel>>.Failure("Error de servidor.");
        }
        catch (Exception ex)
        {
            return ApiResult<List<SettlementViewModel>>.Failure(ex.Message);
        }
    }
}
