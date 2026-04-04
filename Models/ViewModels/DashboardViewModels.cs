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

public class CreateExpenseModel
{
    public string Title { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public List<ExpenseSplitViewModel> Splits { get; set; } = new();
    public List<ExpensePaymentViewModel> Payments { get; set; } = new();
    
    // UI Helpers (optional, can be mapped from UI)
    public decimal Amount { get => TotalAmount; set => TotalAmount = value; }
    public string Description { get => Title; set => Title = value; }
}

public class ExpenseSplitViewModel
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
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
