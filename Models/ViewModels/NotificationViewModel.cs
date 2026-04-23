namespace SplitMoney.Client.Models.ViewModels;

public class NotificationViewModel
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public Guid? RelatedId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum NotificationType
{
    ExpenseConfirmation = 1,
    Information = 2
}
