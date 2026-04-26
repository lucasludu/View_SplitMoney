using SplitMoney.Client.Models;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Services;

public interface IExpenseService
{
    Task<ApiResult<DashboardViewModel>> GetDashboardAsync();
    Task<ApiResult<List<GroupSummaryViewModel>>> GetUserGroupsAsync();
    Task<ApiResult> CreateExpenseAsync(CreateExpenseModel expense);
    Task<ApiResult> CreateGroupAsync(string name, List<MemberSpendRecordViewModel> initialMembers);
    Task<ApiResult<List<GroupMemberResponse>>> GetGroupMembersAsync(string groupId);
    Task<ApiResult<List<BalanceResponse>>> GetGroupBalancesAsync(string groupId);
    Task<ApiResult<GroupSpendingBreakdownViewModel>> GetGroupSpendingBreakdownAsync(string groupId);
    Task<ApiResult<ExpenseDetailViewModel>> GetExpenseDetailsAsync(Guid expenseId);
    Task<ApiResult> SettleDebtAsync(SettleDebtModel settlement);
    Task<ApiResult> UpdateExpenseAsync(Guid id, CreateExpenseModel expense);
    Task<ApiResult> DeleteExpenseAsync(Guid id);
    Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync();
    Task<ApiResult<ExpenseAuditViewModel>> GetExpenseAuditAsync(Guid expenseId);
    Task<ApiResult<GroupSpendingSummaryViewModel>> GetGroupSpendingSummaryAsync(Guid groupId);
    Task<ApiResult<byte[]>> ExportGroupReportAsync(Guid groupId);
    Task<ApiResult> UpdateGroupAsync(Guid id, string name, List<MemberSpendRecordViewModel> initialMembers);
    Task<ApiResult> DeleteGroupAsync(Guid id);
    Task<ApiResult<List<SettlementViewModel>>> GetMySettlementsAsync();
}
