using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Services;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private static BookingRepository? _instance;
        private static readonly object _lock = new object();
        private List<Booking> _bookings;
        private readonly SqlDataService _sqlDataService;

        private BookingRepository()
        {
            _sqlDataService = new SqlDataService();
            _bookings = new List<Booking>();
            LoadBookingsAsync();
        }

        public static BookingRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BookingRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        private async void LoadBookingsAsync()
        {
            _bookings = await _sqlDataService.LoadBookingsAsync();
        }

        public List<Booking> GetAll()
        {
            return _bookings.ToList();
        }

        public Booking? GetById(int id)
        {
            return _bookings.FirstOrDefault(b => b.BookingID == id);
        }

        public List<Booking> GetByCustomerId(int customerId)
        {
            return _bookings.Where(b => b.CustomerID == customerId).ToList();
        }

        public List<Booking> Search(Expression<Func<Booking, bool>> predicate)
        {
            return _bookings.AsQueryable().Where(predicate).ToList();
        }

        public async void Add(Booking booking)
        {
            await _sqlDataService.SaveBookingAsync(booking);
            _bookings.Add(booking);
        }

        public async void Update(Booking booking)
        {
            await _sqlDataService.SaveBookingAsync(booking);
            var existingBooking = _bookings.FirstOrDefault(b => b.BookingID == booking.BookingID);
            if (existingBooking != null)
            {
                var index = _bookings.IndexOf(existingBooking);
                _bookings[index] = booking;
            }
        }

        public async void Delete(int id)
        {
            await _sqlDataService.DeleteBookingAsync(id);
            var booking = _bookings.FirstOrDefault(b => b.BookingID == id);
            if (booking != null)
            {
                booking.BookingStatus = 0; // Set to Not Booked
            }
        }

        public bool Exists(int id)
        {
            return _bookings.Any(b => b.BookingID == id);
        }
    }
}
