using System;

namespace FUMiniHotelSystem.DataAccess.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerFullName { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public DateTime CustomerBirthday { get; set; }
        public int CustomerStatus { get; set; } // 1 Active, 2 Deleted
        public string Password { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
    }
}
