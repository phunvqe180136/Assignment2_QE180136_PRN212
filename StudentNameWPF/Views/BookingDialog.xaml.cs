using System.Windows;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.Business.Services;
using System.Windows.Controls;

namespace StudentNameWPF.Views
{
    public partial class BookingDialog : Window
    {
        public Booking Booking { get; private set; }
        private readonly BookingService _bookingService;
        private readonly RoomService _roomService;
        private readonly int _customerId;

        public BookingDialog(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            _bookingService = new BookingService();
            _roomService = new RoomService();
            Booking = new Booking
            {
                CustomerID = customerId,
                BookingStatus = 1, // 1: Booked (theo SQL)
                CreatedDate = DateTime.Now
            };
            LoadData();
        }

        private void LoadData()
        {
            // Load available rooms
            var rooms = _roomService.GetAllRooms();
            RoomComboBox.ItemsSource = rooms;
            
            // Set default dates
            CheckInDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            CheckOutDatePicker.SelectedDate = DateTime.Today.AddDays(2);
            
            // Set default room
            if (rooms.Any())
            {
                RoomComboBox.SelectedItem = rooms.First();
            }
            
            // Calculate initial total
            CalculateTotal();
            
            // Add event handlers
            RoomComboBox.SelectionChanged += (s, e) => CalculateTotal();
            CheckInDatePicker.SelectedDateChanged += (s, e) => CalculateTotal();
            CheckOutDatePicker.SelectedDateChanged += (s, e) => CalculateTotal();
        }

        private void CalculateTotal()
        {
            if (RoomComboBox.SelectedItem is Room selectedRoom && 
                CheckInDatePicker.SelectedDate.HasValue && 
                CheckOutDatePicker.SelectedDate.HasValue)
            {
                var days = (CheckOutDatePicker.SelectedDate.Value - CheckInDatePicker.SelectedDate.Value).Days;
                if (days > 0)
                {
                    var total = selectedRoom.RoomPricePerDate * days;
                    TotalAmountTextBox.Text = total.ToString("C");
                }
                else
                {
                    TotalAmountTextBox.Text = "$0.00";
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                if (RoomComboBox.SelectedItem is Room selectedRoom)
                {
                    Booking.RoomID = selectedRoom.RoomID;
                }
                
                Booking.CheckInDate = CheckInDatePicker.SelectedDate!.Value;
                Booking.CheckOutDate = CheckOutDatePicker.SelectedDate!.Value;
                Booking.TotalAmount = decimal.Parse(TotalAmountTextBox.Text.Replace("$", "").Replace(",", ""));
                Booking.BookingStatus = 1; // 1: Booked (theo SQL)
                Booking.CreatedDate = DateTime.Now;

                var result = _bookingService.AddBooking(Booking);
                
                if (result.IsSuccess)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(result.Message);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;
            ErrorMessageText.Text = "";

            if (RoomComboBox.SelectedItem == null)
            {
                ShowError("Please select a room.");
                return false;
            }

            if (!CheckInDatePicker.SelectedDate.HasValue)
            {
                ShowError("Check-in date is required.");
                return false;
            }

            if (CheckInDatePicker.SelectedDate.Value < DateTime.Today)
            {
                ShowError("Check-in date cannot be in the past.");
                return false;
            }

            if (!CheckOutDatePicker.SelectedDate.HasValue)
            {
                ShowError("Check-out date is required.");
                return false;
            }

            if (CheckOutDatePicker.SelectedDate.Value <= CheckInDatePicker.SelectedDate.Value)
            {
                ShowError("Check-out date must be after check-in date.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TotalAmountTextBox.Text))
            {
                ShowError("Total amount is required.");
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }
    }
}
