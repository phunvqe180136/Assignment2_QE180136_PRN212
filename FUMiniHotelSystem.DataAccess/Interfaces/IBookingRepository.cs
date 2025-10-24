using FUMiniHotelSystem.DataAccess.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Interfaces
{
    public interface IBookingRepository
    {
        List<Booking> GetAll();
        Booking? GetById(int id);
        List<Booking> GetByCustomerId(int customerId);
        List<Booking> Search(Expression<Func<Booking, bool>> predicate);
        void Add(Booking booking);
        void Update(Booking booking);
        void Delete(int id);
        bool Exists(int id);
    }
}
