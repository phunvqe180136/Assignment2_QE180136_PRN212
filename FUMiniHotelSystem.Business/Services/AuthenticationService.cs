using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Repositories;
using FUMiniHotelSystem.Business.Interfaces;

namespace FUMiniHotelSystem.Business.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICustomerRepository _customerRepository;

        public AuthenticationService()
        {
            _customerRepository = CustomerRepository.Instance;
        }

        public async Task<Customer?> AuthenticateAsync(string email, string password)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return null;
                }

                var customer = _customerRepository.GetByEmail(email);
                if (customer == null || customer.Password != password)
                {
                    return null;
                }

                return customer;
            });
        }

        public async Task<bool> RegisterAsync(Customer customer)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _customerRepository.Add(customer);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword)
        {
            return await Task.Run(() =>
            {
                var customer = _customerRepository.GetById(customerId);
                if (customer == null || customer.Password != oldPassword)
                {
                    return false;
                }

                customer.Password = newPassword;
                _customerRepository.Update(customer);
                return true;
            });
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            return await Task.Run(() =>
            {
                var customer = _customerRepository.GetByEmail(email);
                if (customer == null)
                {
                    return false;
                }

                // In a real application, you would send an email with reset link
                return true;
            });
        }

        public async Task<Customer?> GetCurrentUserAsync()
        {
            return await Task.FromResult<Customer?>(null);
        }

        public async Task LogoutAsync()
        {
            await Task.CompletedTask;
        }

        public AuthenticationResult Authenticate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Message = "Vui lòng nhập đầy đủ email và mật khẩu."
                };
            }

            var customer = _customerRepository.GetByEmail(email);
            if (customer == null)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Message = "Email không tồn tại trong hệ thống."
                };
            }

            if (customer.Password != password)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Message = "Mật khẩu không đúng."
                };
            }

            return new AuthenticationResult
            {
                IsSuccess = true,
                Message = "Đăng nhập thành công.",
                Customer = customer
            };
        }
    }

    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Customer? Customer { get; set; }
    }
}
