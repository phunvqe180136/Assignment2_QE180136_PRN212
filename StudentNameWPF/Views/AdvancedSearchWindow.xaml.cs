using System.Windows;
using FUMiniHotelSystem.Business.Services;
using FUMiniHotelSystem.DataAccess.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace StudentNameWPF.Views
{
    public partial class AdvancedSearchWindow : Window
    {
        private readonly CustomerService _customerService;
        private readonly RoomService _roomService;
        private readonly BookingService _bookingService;

        public AdvancedSearchWindow()
        {
            InitializeComponent();
            
            _customerService = new CustomerService();
            _roomService = new RoomService();
            _bookingService = new BookingService();
            
            LoadRoomTypes();
            SetupDefaultDates();
        }

        private void LoadRoomTypes()
        {
            try
            {
                var roomTypes = _roomService.GetAllRoomTypes();
                RoomTypeComboBox.ItemsSource = roomTypes;
                RoomTypeComboBox.DisplayMemberPath = "RoomTypeName";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading room types: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupDefaultDates()
        {
            FromDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
            ToDatePicker.SelectedDate = DateTime.Today;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all search fields
            SearchTermTextBox.Text = "";
            FromDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
            ToDatePicker.SelectedDate = DateTime.Today;
            CustomerStatusComboBox.SelectedIndex = 0;
            RoomTypeComboBox.SelectedIndex = -1;
            BookingStatusComboBox.SelectedIndex = 0;
            MinPriceTextBox.Text = "";
            MaxPriceTextBox.Text = "";
            
            // Clear results
            SearchResultsGrid.ItemsSource = null;
            ResultsCountText.Text = "";
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchResults = new List<SearchResult>();
                var searchTerm = SearchTermTextBox.Text?.Trim() ?? "";

                // Search customers if selected
                if (SearchCustomersCheckBox.IsChecked == true)
                {
                    var customers = _customerService.GetAllCustomers();
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        customers = customers.Where(c => 
                            c.CustomerFullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.EmailAddress.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.Telephone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    // Apply filters
                    if (CustomerStatusComboBox.SelectedIndex > 0)
                    {
                        var status = CustomerStatusComboBox.SelectedIndex;
                        customers = customers.Where(c => c.CustomerStatus == status).ToList();
                    }

                    foreach (var customer in customers)
                    {
                        searchResults.Add(new SearchResult
                        {
                            Type = "Customer",
                            ID = customer.CustomerID,
                            Name = customer.CustomerFullName,
                            Details = $"Email: {customer.EmailAddress}, Phone: {customer.Telephone}",
                            Date = customer.CustomerBirthday.ToString("yyyy-MM-dd"),
                            Status = customer.CustomerStatus == 1 ? "Active" : "Inactive"
                        });
                    }
                }

                // Search rooms if selected
                if (SearchRoomsCheckBox.IsChecked == true)
                {
                    var rooms = _roomService.GetAllRooms();
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        rooms = rooms.Where(r => 
                            r.RoomNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            r.RoomDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    // Apply room type filter
                    if (RoomTypeComboBox.SelectedItem is RoomType selectedRoomType)
                    {
                        rooms = rooms.Where(r => r.RoomTypeID == selectedRoomType.RoomTypeID).ToList();
                    }

                    // Apply price range filter
                    if (decimal.TryParse(MinPriceTextBox.Text, out decimal minPrice))
                    {
                        rooms = rooms.Where(r => r.RoomPricePerDate >= minPrice).ToList();
                    }
                    if (decimal.TryParse(MaxPriceTextBox.Text, out decimal maxPrice))
                    {
                        rooms = rooms.Where(r => r.RoomPricePerDate <= maxPrice).ToList();
                    }

                    foreach (var room in rooms)
                    {
                        searchResults.Add(new SearchResult
                        {
                            Type = "Room",
                            ID = room.RoomID,
                            Name = $"Room {room.RoomNumber}",
                            Details = room.RoomDescription,
                            Date = $"Capacity: {room.RoomMaxCapacity}",
                            Status = room.RoomStatus == 1 ? "Available" : "Unavailable"
                        });
                    }
                }

                // Search bookings if selected
                if (SearchBookingsCheckBox.IsChecked == true)
                {
                    var fromDate = FromDatePicker.SelectedDate ?? DateTime.Today.AddDays(-30);
                    var toDate = ToDatePicker.SelectedDate ?? DateTime.Today;
                    
                    var bookings = _bookingService.GetBookingsByDateRange(fromDate, toDate);
                    
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        bookings = bookings.Where(b => 
                            b.BookingID.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            (b.Customer?.CustomerFullName ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            (b.Room?.RoomNumber ?? "").Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    // Apply booking status filter
                    if (BookingStatusComboBox.SelectedIndex > 0)
                    {
                        var statuses = new[] { "", "Booked", "Not Booked" };
                        var selectedStatus = statuses[BookingStatusComboBox.SelectedIndex];
                        var statusValue = selectedStatus == "Booked" ? 1 : 0;
                        bookings = bookings.Where(b => b.BookingStatus == statusValue).ToList();
                    }

                    foreach (var booking in bookings)
                    {
                        searchResults.Add(new SearchResult
                        {
                            Type = "Booking",
                            ID = booking.BookingID,
                            Name = $"Booking #{booking.BookingID}",
                            Details = $"Check-in: {booking.CheckInDate:yyyy-MM-dd}, Check-out: {booking.CheckOutDate:yyyy-MM-dd}",
                            Date = booking.CheckInDate.ToString("yyyy-MM-dd"),
                            Status = booking.BookingStatus == 1 ? "Booked" : "Not Booked"
                        });
                    }
                }

                // Display results
                SearchResultsGrid.ItemsSource = searchResults;
                ResultsCountText.Text = $"Found {searchResults.Count} results";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error performing search: {ex.Message}", "Search Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportResultsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchResultsGrid.ItemsSource is not List<SearchResult> results || !results.Any())
                {
                    MessageBox.Show("No results to export.", "Export", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt",
                    DefaultExt = "csv",
                    FileName = $"SearchResults_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csvContent = new System.Text.StringBuilder();
                    csvContent.AppendLine("Type,ID,Name,Details,Date,Status");
                    
                    foreach (var result in results)
                    {
                        csvContent.AppendLine($"{result.Type},{result.ID},{result.Name},{result.Details},{result.Date},{result.Status}");
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csvContent.ToString());
                    MessageBox.Show($"Results exported successfully to:\n{saveFileDialog.FileName}", "Export Complete", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting results: {ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class SearchResult
    {
        public string Type { get; set; } = string.Empty;
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
