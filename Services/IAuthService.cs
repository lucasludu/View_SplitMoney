using SplitMoney.Client.Models;

namespace SplitMoney.Client.Services
{
    public interface IAuthService
    {
        Task<Response<LoginResponse>> Login(LoginRequest loginRequest);
        Task<Response<string>> Register(RegisterUserRequest registerRequest);
        Task Logout();
        Task<string> RefreshToken();
    }
}
