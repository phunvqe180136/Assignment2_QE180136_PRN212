using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Repositories;
using FUMiniHotelSystem.Business.Models;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.Business.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRoomRepository _roomRepository;

        public BookingService()
        {
            _bookingRepository = BookingRepository.Instance;
            _customerRepository = CustomerRepository.Instance;
            _roomRepository = RoomRepository.Instance;
        }

        public List<Booking> GetAllBookings()
        {
            var bookings = _bookingRepository.GetAll();
            var customers = _customerRepository.GetAll();
            var rooms = _roomRepository.GetAll();
            
            // Populate navigation properties
            foreach (var booking in bookings)
            {
                booking.Customer = customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                booking.Room = rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
            }
            
            return bookings;
        }

        public Booking? GetBookingById(int id)
        {
            var booking = _bookingRepository.GetById(id);
            if (booking != null)
            {
                var customers = _customerRepository.GetAll();
                var rooms = _roomRepository.GetAll();
                booking.Customer = customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                booking.Room = rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
            }
            return booking;
        }

        public List<Booking> GetBookingsByCustomerId(int customerId)
        {
            var bookings = _bookingRepository.GetByCustomerId(customerId);
            var customers = _customerRepository.GetAll();
            var rooms = _roomRepository.GetAll();
            
            // Populate navigation properties
            foreach (var booking in bookings)
            {
                booking.Customer = customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                booking.Room = rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
            }
            
            return bookings;
        }

        public List<Booking> SearchBookings(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllBookings();

            var bookings = _bookingRepository.Search(b => 
                b.CustomerID.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.RoomID.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.TotalAmount.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            
            var customers = _customerRepository.GetAll();
            var rooms = _roomRepository.GetAll();
            
            // Populate navigation properties
            foreach (var booking in bookings)
            {
                booking.Customer = customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                booking.Room = rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
            }
            
            return bookings;
        }

        public List<Booking> GetBookingsByDateRange(DateTime startDate, DateTime endDate)
        {
            var bookings = _bookingRepository.Search(b => 
                b.CheckInDate >= startDate && b.CheckInDate <= endDate);
            
            var customers = _customerRepository.GetAll();
            var rooms = _roomRepository.GetAll();
            
            // Populate navigation properties
            foreach (var booking in bookings)
            {
                booking.Customer = customers.FirstOrDefault(c => c.CustomerID == booking.CustomerID);
                booking.Room = rooms.FirstOrDefault(r => r.RoomID == booking.RoomID);
            }
            
            return bookings.OrderByDescending(b => b.CheckInDate).ToList();
        }

        public ServiceResult AddBooking(Booking booking)
        {
            try
            {
                // Validation
                var validationResult = ValidateBooking(booking);
                if (!validationResult.IsSuccess)
                    return validationResult;

                // Check if customer exists
                if (!_customerRepository.Exists(booking.CustomerID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Customer not found."
                    };
                }

                // Check if room exists
                if (!_roomRepository.Exists(booking.RoomID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Room not found."
                    };
                }

                // Check for room availability
                var conflictingBookings = _bookingRepository.Search(b => 
                    b.RoomID == booking.RoomID && 
                    b.BookingStatus == 1 && // 1: Booked (theo SQL)
                    ((booking.CheckInDate >= b.CheckInDate && booking.CheckInDate < b.CheckOutDate) ||
                     (booking.CheckOutDate > b.CheckInDate && booking.CheckOutDate <= b.CheckOutDate) ||
                     (booking.CheckInDate <= b.CheckInDate && booking.CheckOutDate >= b.CheckOutDate)));

                if (conflictingBookings.Any())
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Room is not available for the selected dates."
                    };
                }

                booking.CreatedDate = DateTime.Now;
                _bookingRepository.Add(booking);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Booking created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error creating booking: {ex.Message}"
                };
            }
        }

        public ServiceResult UpdateBooking(Booking booking)
        {
            try
            {
                // Validation
                var validationResult = ValidateBooking(booking);
                if (!validationResult.IsSuccess)
                    return validationResult;

                // Check if booking exists
                if (!_bookingRepository.Exists(booking.BookingID))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Booking not found."
                    };
                }

                _bookingRepository.Update(booking);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Booking updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error updating booking: {ex.Message}"
                };
            }
        }

        public ServiceResult DeleteBooking(int id)
        {
            try
            {
                if (!_bookingRepository.Exists(id))
                {
                    return new ServiceResult
                    {
                        IsSuccess = false,
                        Message = "Booking not found."
                    };
                }

                _bookingRepository.Delete(id);
                return new ServiceResult
                {
                    IsSuccess = true,
                    Message = "Booking deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = $"Error deleting booking: {ex.Message}"
                };
            }
        }

        private ServiceResult ValidateBooking(Booking booking)
        {
            if (booking.CustomerID <= 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Customer is required."
                };
            }

            if (booking.RoomID <= 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Room is required."
                };
            }

            if (booking.CheckInDate < DateTime.Today)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Check-in date cannot be in the past."
                };
            }

            if (booking.CheckOutDate <= booking.CheckInDate)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Check-out date must be after check-in date."
                };
            }

            if (booking.TotalAmount < 0)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Total amount cannot be negative."
                };
            }

            if (booking.BookingStatus < 0 || booking.BookingStatus > 1)
            {
                return new ServiceResult
                {
                    IsSuccess = false,
                    Message = "Booking status must be 0 (Not Booked) or 1 (Booked)."
                };
            }

            return new ServiceResult { IsSuccess = true };
        }
    }
}
