using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Repositories;
using FUMiniHotelSystem.Business.Interfaces;
using FUMiniHotelSystem.Business.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService()
        {
            _customerRepository = CustomerRepository.Instance;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await Task.FromResult(_customerRepository.GetAll());
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await Task.FromResult(_customerRepository.GetById(id));
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await Task.FromResult(_customerRepository.GetByEmail(email));
        }

        public async Task<List<Customer>> SearchCustomersAsync(Expression<Func<Customer, bool>> predicate)
        {
            return await Task.FromResult(_customerRepository.Search(predicate));
        }

        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            try
            {
                var result = AddCustomer(customer);
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                var result = UpdateCustomer(customer);
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var result = DeleteCustomer(id);
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await Task.FromResult(_customerRepository.Exists(id));
        }

        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            return await Task.FromResult(_customerRepository.GetAll().Where(c => c.CustomerStatus == 1).ToList());
        }

        public async Task<int> GetCustomerCountAsync()
        {
            return await Task.FromResult(_customerRepository.GetAll().Count);
        }

        public List<Customer> GetAllCustomers()
        {
            return _customerRepository.GetAll();
        }

        public Customer? GetCustomerById(int id)
        {
            return _customerRepository.GetById(id);
        }

        public Customer? GetCustomerByEmail(string email)
        {
            return _customerRepository.GetByEmail(email);
        }

        public List<Customer> SearchCustomers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllCustomers();

            return _customerRepository.Search(c => 
                c.CustomerFullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.EmailAddress.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Telephone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        public ServiceResult AddCustomer(Customer customer)
        {
            try
            {
                // Validation
                var validationResult = ValidateCustomer(customer);
                if (!validationResult.IsSuccess)
                    return validationResult;

                // Check if email already exists
                if (_customerRepository.GetByEmail(customer.EmailAddress) != null)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "A customer with this email already exists."
                    };
                }

                _customerRepository.Add(customer);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Customer added successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error adding customer: {ex.Message}"
                };
            }
        }

        public ServiceResult UpdateCustomer(Customer customer)
        {
            try
            {
                // Validation
                var validationResult = ValidateCustomer(customer);
                if (!validationResult.IsSuccess)
                    return validationResult;

                // Check if customer exists
                if (!_customerRepository.Exists(customer.CustomerID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Customer not found."
                    };
                }

                // Check if email is taken by another customer
                var existingCustomer = _customerRepository.GetByEmail(customer.EmailAddress);
                if (existingCustomer != null && existingCustomer.CustomerID != customer.CustomerID)
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "A customer with this email already exists."
                    };
                }

                _customerRepository.Update(customer);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Customer updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error updating customer: {ex.Message}"
                };
            }
        }

        public ServiceResult DeleteCustomer(int id)
        {
            try
            {
                if (!_customerRepository.Exists(id))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Customer not found."
                    };
                }

                _customerRepository.Delete(id);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Customer deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error deleting customer: {ex.Message}"
                };
            }
        }

        private ServiceResult ValidateCustomer(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.CustomerFullName))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Customer full name is required."
                };
            }

            if (customer.CustomerFullName.Length > 50)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Customer full name cannot exceed 50 characters."
                };
            }

            if (string.IsNullOrWhiteSpace(customer.EmailAddress))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Email address is required."
                };
            }

            if (customer.EmailAddress.Length > 50)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Email address cannot exceed 50 characters."
                };
            }

            if (!IsValidEmail(customer.EmailAddress))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Invalid email format."
                };
            }

            if (string.IsNullOrWhiteSpace(customer.Telephone))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Telephone is required."
                };
            }

            if (customer.Telephone.Length > 12)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Telephone cannot exceed 12 characters."
                };
            }

            if (string.IsNullOrWhiteSpace(customer.Password))
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Password is required."
                };
            }

            if (customer.Password.Length > 50)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Password cannot exceed 50 characters."
                };
            }

            return new ServiceResult { IsSuccess = true };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

}
