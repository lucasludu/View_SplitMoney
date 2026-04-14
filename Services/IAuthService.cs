using SplitMoney.Client.Models;

namespace SplitMoney.Client.Services
{
    public interface IAuthService
    {
        Task<Response<LoginResponse>> Login(LoginRequest loginRequest);
        Task<Response<string>> Register(RegisterUserRequest registerRequest);
        Task Logout();
        Task<string> RefreshToken();
        Task<Response<UserDto>> GetProfile();
        Task<Response<string>> UpdateProfile(UserDto userUpdate);
    }
}
