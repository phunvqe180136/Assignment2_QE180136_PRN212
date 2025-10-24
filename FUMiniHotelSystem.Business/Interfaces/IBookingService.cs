using FUMiniHotelSystem.DataAccess.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.Business.Interfaces
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId);
        Task<List<Booking>> SearchBookingsAsync(Expression<Func<Booking, bool>> predicate);
        Task<bool> AddBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> DeleteBookingAsync(int id);
        Task<bool> BookingExistsAsync(int id);
        Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Booking>> GetActiveBookingsAsync();
        Task<decimal> CalculateTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
    }
}
