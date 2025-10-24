using System.Windows;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.Business.Services;
using System.Linq;

namespace StudentNameWPF.Views
{
    public partial class CustomerMainWindow : Window
    {
        private readonly BookingService _bookingService;
        private Customer? _currentUser;

        public CustomerMainWindow()
        {
            InitializeComponent();
            _bookingService = new BookingService();
            LoadCurrentUser();
            LoadProfile();
            LoadBookingHistory();
        }

        private void LoadCurrentUser()
        {
            _currentUser = Application.Current.Properties["CurrentUser"] as Customer;
            if (_currentUser != null)
            {
                var welcomeText = this.FindName("WelcomeText") as System.Windows.Controls.TextBlock;
                if (welcomeText != null)
                {
                    welcomeText.Text = _currentUser.CustomerFullName;
                }
            }
        }

        private void LoadProfile()
        {
            if (_currentUser != null)
            {
                ProfileFullName.Text = _currentUser.CustomerFullName;
                ProfileEmail.Text = _currentUser.EmailAddress;
                ProfileTelephone.Text = _currentUser.Telephone;
                ProfileBirthday.Text = _currentUser.CustomerBirthday.ToString("yyyy-MM-dd");
                ProfileStatus.Text = _currentUser.CustomerStatus == 1 ? "Active" : "Inactive";
                ProfileMemberSince.Text = "2024-01-01"; // Default value since we don't have this field
            }
        }

        private void LoadBookingHistory()
        {
            if (_currentUser != null)
            {
                try
                {
                    var bookings = _bookingService.GetBookingsByCustomerId(_currentUser.CustomerID);
                    var bookingData = bookings.Select(b => new
                    {
                        b.BookingID,
                        RoomNumber = b.Room?.RoomNumber ?? "Unknown",
                        b.CheckInDate,
                        b.CheckOutDate,
                        b.TotalAmount,
                        b.BookingStatus
                    }).ToList();

                    BookingHistoryGrid.ItemsSource = bookingData;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading booking history: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Properties["CurrentUser"] = null;
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                var dialog = new CustomerDialog(_currentUser);
                if (dialog.ShowDialog() == true)
                {
                    var updatedCustomer = dialog.Customer;
                    var customerService = new CustomerService();
                    var result = customerService.UpdateCustomer(updatedCustomer);
                    
                    if (result.IsSuccess)
                    {
                        _currentUser = updatedCustomer;
                        Application.Current.Properties["CurrentUser"] = _currentUser;
                        LoadProfile();
                        MessageBox.Show("Profile updated successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshProfile_Click(object sender, RoutedEventArgs e)
        {
            LoadProfile();
        }

        private void NewBooking_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                var dialog = new BookingDialog(_currentUser.CustomerID);
                if (dialog.ShowDialog() == true)
                {
                    LoadBookingHistory();
                    MessageBox.Show("Booking created successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RefreshBookings_Click(object sender, RoutedEventArgs e)
        {
            LoadBookingHistory();
        }
    }
}
