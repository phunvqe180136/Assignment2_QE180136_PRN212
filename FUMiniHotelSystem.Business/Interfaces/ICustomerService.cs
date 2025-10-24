using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.Business.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.Business.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<List<Customer>> SearchCustomersAsync(Expression<Func<Customer, bool>> predicate);
        Task<bool> AddCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> CustomerExistsAsync(int id);
        Task<List<Customer>> GetActiveCustomersAsync();
        Task<int> GetCustomerCountAsync();
        
        // Synchronous methods
        List<Customer> GetAllCustomers();
        Customer? GetCustomerById(int id);
        Customer? GetCustomerByEmail(string email);
        List<Customer> SearchCustomers(string searchTerm);
        ServiceResult AddCustomer(Customer customer);
        ServiceResult UpdateCustomer(Customer customer);
        ServiceResult DeleteCustomer(int id);
    }
}
