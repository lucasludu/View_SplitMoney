using System.Collections.Generic;

namespace SplitMoney.Client.Models.ViewModels
{
    public class GroupSpendingBreakdownViewModel
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public decimal TotalGroupExpense { get; set; }
        public List<MemberSpendingViewModel> Members { get; set; } = new();
    }

    public class MemberSpendingViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal NetBalance { get; set; }
    }
}
