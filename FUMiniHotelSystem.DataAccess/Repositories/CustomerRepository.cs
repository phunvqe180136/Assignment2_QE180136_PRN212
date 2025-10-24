using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Services;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private static CustomerRepository? _instance;
        private static readonly object _lock = new object();
        private List<Customer> _customers;
        private readonly SqlDataService _sqlDataService;

        private CustomerRepository()
        {
            _sqlDataService = new SqlDataService();
            _customers = new List<Customer>();
            LoadCustomersAsync();
        }

        public static CustomerRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CustomerRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        private async void LoadCustomersAsync()
        {
            _customers = await _sqlDataService.LoadCustomersAsync();
        }

        public List<Customer> GetAll()
        {
            return _customers.Where(c => c.CustomerStatus == 1).ToList();
        }

        public Customer? GetById(int id)
        {
            return _customers.FirstOrDefault(c => c.CustomerID == id && c.CustomerStatus == 1);
        }

        public Customer? GetByEmail(string email)
        {
            return _customers.FirstOrDefault(c => c.EmailAddress == email && c.CustomerStatus == 1);
        }

        public List<Customer> Search(Expression<Func<Customer, bool>> predicate)
        {
            return _customers.Where(c => c.CustomerStatus == 1).AsQueryable().Where(predicate).ToList();
        }

        public async void Add(Customer customer)
        {
            await _sqlDataService.SaveCustomerAsync(customer);
            _customers.Add(customer);
        }

        public async void Update(Customer customer)
        {
            await _sqlDataService.SaveCustomerAsync(customer);
            var existingCustomer = _customers.FirstOrDefault(c => c.CustomerID == customer.CustomerID);
            if (existingCustomer != null)
            {
                var index = _customers.IndexOf(existingCustomer);
                _customers[index] = customer;
            }
        }

        public async void Delete(int id)
        {
            await _sqlDataService.DeleteCustomerAsync(id);
            var customer = _customers.FirstOrDefault(c => c.CustomerID == id);
            if (customer != null)
            {
                customer.CustomerStatus = 2; // Soft delete
            }
        }

        public bool Exists(int id)
        {
            return _customers.Any(c => c.CustomerID == id && c.CustomerStatus == 1);
        }
    }
}