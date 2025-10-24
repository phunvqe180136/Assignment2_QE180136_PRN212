using FUMiniHotelSystem.DataAccess.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface ICustomerRepository
    {
        List<Customer> GetAll();
        Customer? GetById(int id);
        Customer? GetByEmail(string email);
        List<Customer> Search(Expression<Func<Customer, bool>> predicate);
        void Add(Customer customer);
        void Update(Customer customer);
        void Delete(int id);
        bool Exists(int id);
    }
}
