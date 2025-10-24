using System;

namespace FUMiniHotelSystem.DataAccess.Models
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int CustomerID { get; set; }
        public int RoomID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int BookingStatus { get; set; } = 1; // 1: Booked, 0: Not Booked (theo SQL)
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
        public Customer? Customer { get; set; }
        public Room? Room { get; set; }
    }
}
