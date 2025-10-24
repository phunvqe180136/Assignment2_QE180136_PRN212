using FUMiniHotelSystem.DataAccess.Models;

namespace FUMiniHotelSystem.Business.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Customer?> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(Customer customer);
        Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<Customer?> GetCurrentUserAsync();
        Task LogoutAsync();
    }
}
