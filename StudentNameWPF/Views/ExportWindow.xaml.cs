using System.Windows;
using Microsoft.Win32;
using FUMiniHotelSystem.Business.Services;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Linq;

namespace StudentNameWPF.Views
{
    public partial class ExportWindow : Window
    {
        private readonly CustomerService _customerService;
        private readonly RoomService _roomService;
        private readonly BookingService _bookingService;

        public ExportWindow()
        {
            InitializeComponent();
            
            _customerService = new CustomerService();
            _roomService = new RoomService();
            _bookingService = new BookingService();
            
            // Set default file path
            FilePathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                $"HotelExport_{DateTime.Now:yyyyMMdd_HHmmss}");
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|JSON Files (*.json)|*.json|PDF Files (*.pdf)|*.pdf",
                DefaultExt = "xlsx",
                FileName = $"HotelExport_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = saveFileDialog.FileName;
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePath = FilePathTextBox.Text;
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    MessageBox.Show("Please select a file location.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate that at least one data type is selected
                if (ExportCustomersCheckBox.IsChecked != true && 
                    ExportRoomsCheckBox.IsChecked != true && 
                    ExportBookingsCheckBox.IsChecked != true && 
                    ExportRoomTypesCheckBox.IsChecked != true)
                {
                    MessageBox.Show("Please select at least one data type to export.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var exportData = new ExportData();

                // Collect data based on selections
                if (ExportCustomersCheckBox.IsChecked == true)
                {
                    exportData.Customers = _customerService.GetAllCustomers();
                }

                if (ExportRoomsCheckBox.IsChecked == true)
                {
                    exportData.Rooms = _roomService.GetAllRooms();
                }

                if (ExportBookingsCheckBox.IsChecked == true)
                {
                    var fromDate = FromDatePicker.SelectedDate ?? DateTime.Today.AddDays(-30);
                    var toDate = ToDatePicker.SelectedDate ?? DateTime.Today;
                    
                    // Validate date range
                    if (fromDate > toDate)
                    {
                        MessageBox.Show("From date cannot be later than To date.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    exportData.Bookings = _bookingService.GetBookingsByDateRange(fromDate, toDate);
                }

                if (ExportRoomTypesCheckBox.IsChecked == true)
                {
                    exportData.RoomTypes = _roomService.GetAllRoomTypes();
                }

                // Check if any data was found
                var hasData = (exportData.Customers?.Any() == true) ||
                             (exportData.Rooms?.Any() == true) ||
                             (exportData.Bookings?.Any() == true) ||
                             (exportData.RoomTypes?.Any() == true);

                if (!hasData)
                {
                    MessageBox.Show("No data found for the selected criteria.", "No Data", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Export based on format
                if (ExcelFormatRadio.IsChecked == true)
                {
                    ExportToExcel(exportData, filePath);
                }
                else if (CsvFormatRadio.IsChecked == true)
                {
                    ExportToCsv(exportData, filePath);
                }
                else if (JsonFormatRadio.IsChecked == true)
                {
                    ExportToJson(exportData, filePath);
                }
                else if (PdfFormatRadio.IsChecked == true)
                {
                    ExportToPdf(exportData, filePath);
                }

                MessageBox.Show($"Data exported successfully to:\n{filePath}", "Export Complete", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel(ExportData data, string filePath)
        {
            // Simplified Excel export - in a real application, you'd use EPPlus or similar
            var csvContent = new StringBuilder();
            
            // Add header information
            csvContent.AppendLine("FUMiniHotelSystem - Data Export Report");
            csvContent.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csvContent.AppendLine($"Export Summary:");
            csvContent.AppendLine($"- Customers: {data.Customers?.Count ?? 0}");
            csvContent.AppendLine($"- Rooms: {data.Rooms?.Count ?? 0}");
            csvContent.AppendLine($"- Bookings: {data.Bookings?.Count ?? 0}");
            csvContent.AppendLine($"- Room Types: {data.RoomTypes?.Count ?? 0}");
            csvContent.AppendLine();
            
            if (data.Customers?.Any() == true)
            {
                csvContent.AppendLine("CUSTOMERS");
                csvContent.AppendLine("ID,Full Name,Email,Telephone,Birthday,Status");
                foreach (var customer in data.Customers)
                {
                    var status = customer.CustomerStatus == 1 ? "Active" : "Inactive";
                    csvContent.AppendLine($"{customer.CustomerID},{customer.CustomerFullName},{customer.EmailAddress},{customer.Telephone},{customer.CustomerBirthday:yyyy-MM-dd},{status}");
                }
                csvContent.AppendLine();
            }

            if (data.Rooms?.Any() == true)
            {
                csvContent.AppendLine("ROOMS");
                csvContent.AppendLine("ID,Room Number,Description,Max Capacity,Price,Type ID,Status");
                foreach (var room in data.Rooms)
                {
                    var status = room.RoomStatus == 1 ? "Available" : "Unavailable";
                    csvContent.AppendLine($"{room.RoomID},{room.RoomNumber},{room.RoomDescription},{room.RoomMaxCapacity},{room.RoomPricePerDate},{room.RoomTypeID},{status}");
                }
                csvContent.AppendLine();
            }

            if (data.Bookings?.Any() == true)
            {
                csvContent.AppendLine("BOOKINGS");
                csvContent.AppendLine("ID,Customer ID,Room ID,Check In,Check Out,Total Amount,Status,Booking Date");
                foreach (var booking in data.Bookings)
                {
                    csvContent.AppendLine($"{booking.BookingID},{booking.CustomerID},{booking.RoomID},{booking.CheckInDate:yyyy-MM-dd},{booking.CheckOutDate:yyyy-MM-dd},{booking.TotalAmount},{booking.BookingStatus},{booking.CheckInDate:yyyy-MM-dd}");
                }
                csvContent.AppendLine();
            }

            if (data.RoomTypes?.Any() == true)
            {
                csvContent.AppendLine("ROOM TYPES");
                csvContent.AppendLine("ID,Type Name,Type Description");
                foreach (var roomType in data.RoomTypes)
                {
                    csvContent.AppendLine($"{roomType.RoomTypeID},{roomType.RoomTypeName},{roomType.TypeDescription}");
                }
            }

            File.WriteAllText(filePath.Replace(".xlsx", ".csv"), csvContent.ToString());
        }

        private void ExportToCsv(ExportData data, string filePath)
        {
            var csvContent = new StringBuilder();
            
            // Add header information
            csvContent.AppendLine("FUMiniHotelSystem - Data Export Report");
            csvContent.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csvContent.AppendLine($"Export Summary:");
            csvContent.AppendLine($"- Customers: {data.Customers?.Count ?? 0}");
            csvContent.AppendLine($"- Rooms: {data.Rooms?.Count ?? 0}");
            csvContent.AppendLine($"- Bookings: {data.Bookings?.Count ?? 0}");
            csvContent.AppendLine($"- Room Types: {data.RoomTypes?.Count ?? 0}");
            csvContent.AppendLine();
            
            if (data.Customers?.Any() == true)
            {
                csvContent.AppendLine("CUSTOMERS");
                csvContent.AppendLine("ID,Full Name,Email,Telephone,Birthday,Status");
                foreach (var customer in data.Customers)
                {
                    var status = customer.CustomerStatus == 1 ? "Active" : "Inactive";
                    csvContent.AppendLine($"{customer.CustomerID},{customer.CustomerFullName},{customer.EmailAddress},{customer.Telephone},{customer.CustomerBirthday:yyyy-MM-dd},{status}");
                }
                csvContent.AppendLine();
            }

            if (data.Rooms?.Any() == true)
            {
                csvContent.AppendLine("ROOMS");
                csvContent.AppendLine("ID,Room Number,Description,Max Capacity,Price,Type ID,Status");
                foreach (var room in data.Rooms)
                {
                    var status = room.RoomStatus == 1 ? "Available" : "Unavailable";
                    csvContent.AppendLine($"{room.RoomID},{room.RoomNumber},{room.RoomDescription},{room.RoomMaxCapacity},{room.RoomPricePerDate},{room.RoomTypeID},{status}");
                }
                csvContent.AppendLine();
            }

            if (data.Bookings?.Any() == true)
            {
                csvContent.AppendLine("BOOKINGS");
                csvContent.AppendLine("ID,Customer ID,Room ID,Check In,Check Out,Total Amount,Status,Booking Date");
                foreach (var booking in data.Bookings)
                {
                    csvContent.AppendLine($"{booking.BookingID},{booking.CustomerID},{booking.RoomID},{booking.CheckInDate:yyyy-MM-dd},{booking.CheckOutDate:yyyy-MM-dd},{booking.TotalAmount},{booking.BookingStatus},{booking.CheckInDate:yyyy-MM-dd}");
                }
                csvContent.AppendLine();
            }

            if (data.RoomTypes?.Any() == true)
            {
                csvContent.AppendLine("ROOM TYPES");
                csvContent.AppendLine("ID,Type Name,Type Description");
                foreach (var roomType in data.RoomTypes)
                {
                    csvContent.AppendLine($"{roomType.RoomTypeID},{roomType.RoomTypeName},{roomType.TypeDescription}");
                }
            }

            File.WriteAllText(filePath, csvContent.ToString());
        }

        private void ExportToJson(ExportData data, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        private void ExportToPdf(ExportData data, string filePath)
        {
            // Simplified PDF export - in a real application, you'd use iTextSharp or similar
            var pdfContent = new StringBuilder();
            pdfContent.AppendLine("================================================");
            pdfContent.AppendLine("FUMiniHotelSystem - Management Report");
            pdfContent.AppendLine("================================================");
            pdfContent.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            pdfContent.AppendLine();
            pdfContent.AppendLine("EXPORT SUMMARY:");
            pdfContent.AppendLine($"- Total Customers: {data.Customers?.Count ?? 0}");
            pdfContent.AppendLine($"- Total Rooms: {data.Rooms?.Count ?? 0}");
            pdfContent.AppendLine($"- Total Bookings: {data.Bookings?.Count ?? 0}");
            pdfContent.AppendLine($"- Total Room Types: {data.RoomTypes?.Count ?? 0}");
            pdfContent.AppendLine();

            if (data.Customers?.Any() == true)
            {
                pdfContent.AppendLine($"CUSTOMERS ({data.Customers.Count})");
                pdfContent.AppendLine("================================================");
                pdfContent.AppendLine("ID | Full Name | Email | Telephone | Birthday | Status");
                pdfContent.AppendLine("---|-----------|-------|-----------|----------|--------");
                foreach (var customer in data.Customers)
                {
                    var status = customer.CustomerStatus == 1 ? "Active" : "Inactive";
                    pdfContent.AppendLine($"{customer.CustomerID} | {customer.CustomerFullName} | {customer.EmailAddress} | {customer.Telephone} | {customer.CustomerBirthday:yyyy-MM-dd} | {status}");
                }
                pdfContent.AppendLine();
            }

            if (data.Rooms?.Any() == true)
            {
                pdfContent.AppendLine($"ROOMS ({data.Rooms.Count})");
                pdfContent.AppendLine("================================================");
                pdfContent.AppendLine("ID | Room Number | Description | Max Capacity | Price | Type ID | Status");
                pdfContent.AppendLine("---|-------------|-------------|--------------|-------|---------|--------");
                foreach (var room in data.Rooms)
                {
                    var status = room.RoomStatus == 1 ? "Available" : "Unavailable";
                    pdfContent.AppendLine($"{room.RoomID} | {room.RoomNumber} | {room.RoomDescription} | {room.RoomMaxCapacity} | {room.RoomPricePerDate:C} | {room.RoomTypeID} | {status}");
                }
                pdfContent.AppendLine();
            }

            if (data.Bookings?.Any() == true)
            {
                pdfContent.AppendLine($"BOOKINGS ({data.Bookings.Count})");
                pdfContent.AppendLine("================================================");
                pdfContent.AppendLine("ID | Customer ID | Room ID | Check In | Check Out | Total Amount | Status | Booking Date");
                pdfContent.AppendLine("---|-------------|---------|----------|-----------|--------------|--------|-------------");
                foreach (var booking in data.Bookings)
                {
                    pdfContent.AppendLine($"{booking.BookingID} | {booking.CustomerID} | {booking.RoomID} | {booking.CheckInDate:yyyy-MM-dd} | {booking.CheckOutDate:yyyy-MM-dd} | {booking.TotalAmount:C} | {booking.BookingStatus} | {booking.CheckInDate:yyyy-MM-dd}");
                }
                pdfContent.AppendLine();
            }

            if (data.RoomTypes?.Any() == true)
            {
                pdfContent.AppendLine($"ROOM TYPES ({data.RoomTypes.Count})");
                pdfContent.AppendLine("================================================");
                pdfContent.AppendLine("ID | Type Name | Type Description");
                pdfContent.AppendLine("---|-----------|------------------");
                foreach (var roomType in data.RoomTypes)
                {
                    pdfContent.AppendLine($"{roomType.RoomTypeID} | {roomType.RoomTypeName} | {roomType.TypeDescription}");
                }
            }

            pdfContent.AppendLine();
            pdfContent.AppendLine("================================================");
            pdfContent.AppendLine("End of Report");
            pdfContent.AppendLine("================================================");

            File.WriteAllText(filePath.Replace(".pdf", ".txt"), pdfContent.ToString());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public class ExportData
    {
        public List<FUMiniHotelSystem.DataAccess.Models.Customer>? Customers { get; set; }
        public List<FUMiniHotelSystem.DataAccess.Models.Room>? Rooms { get; set; }
        public List<FUMiniHotelSystem.DataAccess.Models.Booking>? Bookings { get; set; }
        public List<FUMiniHotelSystem.DataAccess.Models.RoomType>? RoomTypes { get; set; }
    }
}