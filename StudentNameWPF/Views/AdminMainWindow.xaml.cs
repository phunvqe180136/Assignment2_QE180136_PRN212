using System.Windows;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.Business.Services;
using System.Windows.Controls;
using System.Linq;
using StudentNameWPF.ViewModels;

namespace StudentNameWPF.Views
{
    public partial class AdminMainWindow : Window
    {
        private readonly CustomerManagementViewModel _customerViewModel;
        private readonly RoomManagementViewModel _roomViewModel;
        private readonly BookingService _bookingService;

        public AdminMainWindow()
        {
            InitializeComponent();
            
            _customerViewModel = new CustomerManagementViewModel();
            _roomViewModel = new RoomManagementViewModel();
            _bookingService = new BookingService();
            
            LoadCurrentUser();
            SetupEventHandlers();
            LoadCustomerData();
            LoadRoomData();
        }

        private void SetupEventHandlers()
        {
            // Customer Management Events
            AddCustomerButton.Click += (s, e) => _customerViewModel.AddCommand.Execute(null);
            EditCustomerButton.Click += (s, e) => _customerViewModel.EditCommand.Execute(null);
            DeleteCustomerButton.Click += (s, e) => _customerViewModel.DeleteCommand.Execute(null);
            CustomerSearchButton.Click += (s, e) => 
            {
                _customerViewModel.SearchTerm = CustomerSearchBox.Text;
                _customerViewModel.SearchCommand.Execute(null);
            };
            CustomerRefreshButton.Click += (s, e) => _customerViewModel.RefreshCommand.Execute(null);
            
            // Room Management Events
            AddRoomButton.Click += (s, e) => _roomViewModel.AddCommand.Execute(null);
            EditRoomButton.Click += (s, e) => _roomViewModel.EditCommand.Execute(null);
            DeleteRoomButton.Click += (s, e) => _roomViewModel.DeleteCommand.Execute(null);
            RoomSearchButton.Click += (s, e) => 
            {
                _roomViewModel.SearchTerm = RoomSearchBox.Text;
                _roomViewModel.SearchCommand.Execute(null);
            };
            RoomRefreshButton.Click += (s, e) => _roomViewModel.RefreshCommand.Execute(null);
            FilterByTypeButton.Click += (s, e) => _roomViewModel.FilterByTypeCommand.Execute(null);
        }

        private void LoadCustomerData()
        {
            try
            {
                CustomerDataGrid.DataContext = _customerViewModel;
                CustomerDataGrid.ItemsSource = _customerViewModel.Customers;
                CustomerStatusMessage.Text = $"Loaded {_customerViewModel.Customers.Count} customers";
            }
            catch (Exception ex)
            {
                CustomerStatusMessage.Text = $"Error loading customers: {ex.Message}";
            }
        }

        private void LoadRoomData()
        {
            try
            {
                RoomDataGrid.DataContext = _roomViewModel;
                RoomDataGrid.ItemsSource = _roomViewModel.Rooms;
                RoomTypeFilter.ItemsSource = _roomViewModel.RoomTypes;
                RoomStatusMessage.Text = $"Loaded {_roomViewModel.Rooms.Count} rooms";
            }
            catch (Exception ex)
            {
                RoomStatusMessage.Text = $"Error loading rooms: {ex.Message}";
            }
        }

        private void LoadCurrentUser()
        {
            if (Application.Current.Properties["CurrentUser"] is Customer currentUser)
            {
                var welcomeText = this.FindName("WelcomeText") as System.Windows.Controls.TextBlock;
                if (welcomeText != null)
                {
                    welcomeText.Text = currentUser.CustomerFullName;
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

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra xem có chọn ngày không
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    ReportDataGrid.ItemsSource = null;
                    MessageBox.Show("Vui lòng chọn ngày bắt đầu và ngày kết thúc!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var startDate = StartDatePicker.SelectedDate.Value;
                var endDate = EndDatePicker.SelectedDate.Value;
                
                // Kiểm tra ngày hợp lệ
                if (startDate > endDate)
                {
                    MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var bookings = _bookingService.GetBookingsByDateRange(startDate, endDate);
                
                // Hiển thị booking nếu có
                if (bookings.Any())
                {
                    ReportDataGrid.ItemsSource = bookings.Select(b => new
                    {
                        BookingID = b.BookingID,
                        CustomerName = b.Customer?.CustomerFullName ?? "Unknown",
                        RoomNumber = b.Room?.RoomNumber ?? "Unknown",
                        CheckInDate = b.CheckInDate.ToString("yyyy-MM-dd"),
                        CheckOutDate = b.CheckOutDate.ToString("yyyy-MM-dd"),
                        TotalAmount = b.TotalAmount.ToString("C"),
                        Status = b.BookingStatus == 1 ? "Booked" : "Not Booked",
                        BookingDate = b.CheckInDate.ToString("yyyy-MM-dd")
                    }).ToList();
                }
                else
                {
                    ReportDataGrid.ItemsSource = null;
                    MessageBox.Show($"Không tìm thấy booking nào từ {startDate:yyyy-MM-dd} đến {endDate:yyyy-MM-dd}", 
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo báo cáo: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenDashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dashboardWindow = new DashboardWindow();
                dashboardWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening dashboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
