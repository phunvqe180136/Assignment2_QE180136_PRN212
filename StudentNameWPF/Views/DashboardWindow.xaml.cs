using System.Windows;
using FUMiniHotelSystem.Business.Services;
using FUMiniHotelSystem.DataAccess.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace StudentNameWPF.Views
{
    public partial class DashboardWindow : Window
    {
        private readonly CustomerService _customerService;
        private readonly RoomService _roomService;
        private readonly BookingService _bookingService;

        public DashboardWindow()
        {
            InitializeComponent();
            
            _customerService = new CustomerService();
            _roomService = new RoomService();
            _bookingService = new BookingService();
            
            LoadDashboardData();
            SetupRecentActivities();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Load statistics
                var customers = _customerService.GetAllCustomers();
                var rooms = _roomService.GetAllRooms();
                var bookings = _bookingService.GetAllBookings();
                
                // Update statistics cards
                TotalCustomersText.Text = customers.Count.ToString();
                TotalRoomsText.Text = rooms.Count.ToString();
                
                // Calculate active bookings (Booked status = 1)
                var activeBookings = bookings.Count(b => 
                    b.BookingStatus == 1);
                ActiveBookingsText.Text = activeBookings.ToString();
                
                // Calculate monthly revenue (current month)
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var monthlyRevenue = bookings
                    .Where(b => b.CheckInDate.Month == currentMonth && 
                               b.CheckInDate.Year == currentYear &&
                               b.BookingStatus == 1)
                    .Sum(b => b.TotalAmount);
                MonthlyRevenueText.Text = monthlyRevenue.ToString("C");
                
                // Calculate occupancy rate (rooms currently occupied)
                var totalRooms = rooms.Count;
                var occupiedRooms = bookings.Count(b => 
                    b.BookingStatus == 1 && 
                    b.CheckInDate <= DateTime.Now && 
                    b.CheckOutDate >= DateTime.Now);
                var occupancyRate = totalRooms > 0 ? (double)occupiedRooms / totalRooms * 100 : 0;
                OccupancyRateText.Text = $"{occupancyRate:F1}%";
                
                // Calculate booking trend (compare current month vs last month)
                var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;
                
                var lastMonthBookings = bookings.Count(b => 
                    b.CheckInDate.Month == lastMonth && 
                    b.CheckInDate.Year == lastMonthYear &&
                    b.BookingStatus == 1);
                    
                var thisMonthBookings = bookings.Count(b => 
                    b.CheckInDate.Month == currentMonth && 
                    b.CheckInDate.Year == currentYear &&
                    b.BookingStatus == 1);
                
                var trend = lastMonthBookings > 0 ? 
                    ((double)(thisMonthBookings - lastMonthBookings) / lastMonthBookings * 100) : 
                    (thisMonthBookings > 0 ? 100 : 0);
                    
                TrendText.Text = trend >= 0 ? $"+{trend:F1}%" : $"{trend:F1}%";
                TrendText.Foreground = trend >= 0 ? 
                    System.Windows.Media.Brushes.Green : 
                    System.Windows.Media.Brushes.Red;
                
                // Update last updated time
                LastUpdatedText.Text = DateTime.Now.ToString("HH:mm:ss");
                
                // Update recent activities with real data
                UpdateRecentActivities(bookings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupRecentActivities()
        {
            // Initialize with empty activities - will be populated by LoadDashboardData
            RecentActivitiesGrid.ItemsSource = new ObservableCollection<ActivityItem>();
        }

        private void UpdateRecentActivities(List<Booking> bookings)
        {
            var activities = new ObservableCollection<ActivityItem>();
            
            // Get recent bookings (last 10)
            var recentBookings = bookings
                .OrderByDescending(b => b.CheckInDate)
                .Take(10)
                .ToList();
            
            foreach (var booking in recentBookings)
            {
                var customerName = booking.Customer?.CustomerFullName ?? "Unknown Customer";
                var roomNumber = booking.Room?.RoomNumber ?? "Unknown Room";
                
                activities.Add(new ActivityItem
                {
                    Time = booking.CheckInDate.ToString("HH:mm"),
                    Activity = $"Booking #{booking.BookingID} - {roomNumber}",
                    User = customerName,
                    Status = booking.BookingStatus == 1 ? "Booked" : "Not Booked"
                });
            }
            
            // Add some system activities
            activities.Add(new ActivityItem
            {
                Time = DateTime.Now.AddMinutes(-5).ToString("HH:mm"),
                Activity = "Dashboard refreshed",
                User = "System",
                Status = "Success"
            });
            
            RecentActivitiesGrid.ItemsSource = activities;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboardData();
            MessageBox.Show("Dashboard data refreshed successfully!", "Refresh", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class ActivityItem
    {
        public string Time { get; set; } = string.Empty;
        public string Activity { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
