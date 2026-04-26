using SplitMoney.Client.Models.ViewModels;
using System.Net.Http.Json;
using SplitMoney.Client.Models;

namespace SplitMoney.Client.Services;

public interface INotificationService
{
    Task<List<NotificationViewModel>> GetNotificationsAsync();
    Task<bool> ConfirmExpenseAsync(Guid expenseId);
    Task<bool> DeleteNotificationAsync(Guid id);
    Task<bool> ClearAllNotificationsAsync();
    event Action NotificationsChanged;
}

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    public event Action? NotificationsChanged;

    public NotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<NotificationViewModel>> GetNotificationsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/notifications");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResult<List<NotificationViewModel>>>();
                return result?.Data ?? new List<NotificationViewModel>();
            }
        }
        catch (Exception)
        {
            // Silently fail or log
        }
        return new List<NotificationViewModel>();
    }

    public async Task<bool> ConfirmExpenseAsync(Guid expenseId)
    {
        var response = await _httpClient.PostAsync($"api/v1/expenses/{expenseId}/confirm", null);
        if (response.IsSuccessStatusCode)
        {
            NotificationsChanged?.Invoke();
        }
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> DeleteNotificationAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/notifications/{id}");
        if (response.IsSuccessStatusCode)
        {
            NotificationsChanged?.Invoke();
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ClearAllNotificationsAsync()
    {
        var response = await _httpClient.DeleteAsync("api/v1/notifications/clear-all");
        if (response.IsSuccessStatusCode)
        {
            NotificationsChanged?.Invoke();
        }
        return response.IsSuccessStatusCode;
    }
}
