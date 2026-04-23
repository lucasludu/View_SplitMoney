namespace SplitMoney.Client.Models.ViewModels;

public class DashboardViewModel
{
    public decimal TotalToReceive { get; set; }
    public decimal TotalToPay { get; set; }
    public decimal TotalMonthSpending { get; set; }
    public List<RecentExpenseViewModel> RecentExpenses { get; set; } = new();
}

public class RecentExpenseViewModel
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CategoryIcon { get; set; } = "💰";
    public string CategoryColor { get; set; } = "#000000";
    public bool IsConfirmed { get; set; } = true;
}

public class ExpenseDetailViewModel
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime Date { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "💰";
    public string CategoryColor { get; set; } = "#000000";
    public bool IsConfirmed { get; set; } = true;
    public SplitType SplitType { get; set; } = SplitType.Equal;
    public List<PaymentDetailViewModel> Payments { get; set; } = new();
    public List<SplitDetailViewModel> Splits { get; set; } = new();
}

public class PaymentDetailViewModel
{
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SplitDetailViewModel
{
    public string UserName { get; set; } = string.Empty;
    public decimal AmountOwed { get; set; }
}

public enum SplitType 
{
    Equal = 0,
    Percentage = 1,
    Exact = 2
}

public class CreateExpenseModel
{
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string Currency { get; set; } = "ARS";
    public DateTime Date { get; set; } = DateTime.Now;
    public SplitType SelectedSplitType { get; set; } = SplitType.Equal;
    public List<ExpenseSplitViewModel> Splits { get; set; } = new();
    public List<ExpensePaymentViewModel> Payments { get; set; } = new();
    
    // UI Helpers (optional, can be mapped from UI)
    public decimal Amount { get => TotalAmount; set => TotalAmount = value; }
    public string Description { get => Title; set => Title = value; }
}

public class ExpenseSplitViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty; // UI helper
    public decimal Amount { get; set; } // Can be value or percentage depending on type
    public SplitType SplitType { get; set; } = SplitType.Equal;
}

public class ExpensePaymentViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty; // For UI
    public decimal Amount { get; set; }
}

public class GroupSummaryViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class MemberSpendRecordViewModel
{
    public string Email { get; set; } = string.Empty;
    public decimal AmountSpent { get; set; }
}

public class GroupMemberResponse
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class BalanceResponse
{
    public string DebtorId { get; set; } = string.Empty;
    public string DebtorName { get; set; } = string.Empty;
    public string CreditorId { get; set; } = string.Empty;
    public string CreditorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SettleDebtModel
{
    public Guid GroupId { get; set; }
    public string PayeeId { get; set; } = string.Empty; // A quién se le paga
    public decimal Amount { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconIdentifier { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
    public bool IsGlobal { get; set; }
}

public class ExpenseAuditViewModel
{
    public Guid ExpenseId { get; set; }
    public List<ExpenseAuditLogEntryViewModel> History { get; set; } = new();
}

public class ExpenseAuditLogEntryViewModel
{
    public string Action { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
}

public class GroupSpendingSummaryViewModel
{
    public Guid GroupId { get; set; }
    public decimal TotalGroupSpending { get; set; }
    public List<CategorySpendingViewModel> Categories { get; set; } = new();
}

public class CategorySpendingViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public double Percentage { get; set; }
}

public class SettlementViewModel
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string PayerId { get; set; } = string.Empty;
    public string PayeeId { get; set; } = string.Empty;
    public string PayeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? ProofImageUrl { get; set; }
}
