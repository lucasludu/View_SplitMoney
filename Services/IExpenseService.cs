using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Services;

public interface IExpenseService
{
    Task<DashboardViewModel?> GetDashboardAsync();
    Task<List<GroupSummaryViewModel>> GetUserGroupsAsync();
    Task<bool> CreateExpenseAsync(CreateExpenseModel expense);
    Task<bool> CreateGroupAsync(string name, List<MemberSpendRecordViewModel> initialMembers);
    Task<List<GroupMemberResponse>> GetGroupMembersAsync(string groupId);
    Task<List<BalanceResponse>> GetGroupBalancesAsync(string groupId);
    Task<GroupSpendingBreakdownViewModel?> GetGroupSpendingBreakdownAsync(string groupId);
    Task<ExpenseDetailViewModel?> GetExpenseDetailsAsync(Guid expenseId);
    Task<bool> SettleDebtAsync(SettleDebtModel settlement);
    Task<List<CategoryDto>> GetCategoriesAsync();
}
