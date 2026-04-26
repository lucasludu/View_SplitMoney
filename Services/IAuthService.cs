using SplitMoney.Client.Models;

namespace SplitMoney.Client.Services
{
    public interface IAuthService
    {
        Task<ApiResult<LoginResponse>> Login(LoginRequest loginRequest);
        Task<ApiResult<string>> Register(RegisterUserRequest registerRequest);
        Task Logout();
        Task<string> RefreshToken();
        Task<ApiResult<UserDto>> GetProfile();
        Task<ApiResult<string>> UpdateProfile(UserDto userUpdate);
        Task<bool> IsPremiumAsync();
        Task SimulatePremiumAsync();
        Task<ApiResult<string>> ForgotPassword(string email);
        Task<ApiResult<string>> ResetPassword(ResetPasswordRequest resetRequest);
    }
}
