using System.Windows;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.Business.Services;
using System.Windows.Controls;

namespace StudentNameWPF.Views
{
    public partial class RoomDialog : Window
    {
        public Room Room { get; private set; }
        private bool _isEditMode;
        private readonly RoomService _roomService;

        public RoomDialog()
        {
            InitializeComponent();
            _isEditMode = false;
            _roomService = new RoomService();
            Room = new Room
            {
                RoomStatus = 1,
                RoomID = 0 // Will be set by repository
            };
            LoadData();
        }

        public RoomDialog(Room room)
        {
            InitializeComponent();
            _isEditMode = true;
            _roomService = new RoomService();
            Room = room;
            LoadData();
        }

        private void LoadData()
        {
            RoomNumberTextBox.Text = Room.RoomNumber;
            DescriptionTextBox.Text = Room.RoomDescription;
            MaxCapacityTextBox.Text = Room.RoomMaxCapacity.ToString();
            PriceTextBox.Text = Room.RoomPricePerDate.ToString();
            StatusComboBox.SelectedIndex = Room.RoomStatus - 1;
            
            // Load room types
            var roomTypes = _roomService.GetAllRoomTypes();
            RoomTypeComboBox.ItemsSource = roomTypes;
            
            if (_isEditMode)
            {
                this.Title = "Edit Room";
                RoomTypeComboBox.SelectedItem = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == Room.RoomTypeID);
            }
            else
            {
                this.Title = "Add Room";
                if (roomTypes.Any())
                {
                    RoomTypeComboBox.SelectedItem = roomTypes.First();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                Room.RoomNumber = RoomNumberTextBox.Text.Trim();
                Room.RoomDescription = DescriptionTextBox.Text.Trim();
                Room.RoomMaxCapacity = int.Parse(MaxCapacityTextBox.Text);
                Room.RoomPricePerDate = decimal.Parse(PriceTextBox.Text);
                Room.RoomStatus = ((ComboBoxItem)StatusComboBox.SelectedItem).Tag.ToString() == "1" ? 1 : 2;
                
                if (RoomTypeComboBox.SelectedItem is RoomType selectedRoomType)
                {
                    Room.RoomTypeID = selectedRoomType.RoomTypeID;
                }

                DialogResult = true;
                Close();
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

            if (string.IsNullOrWhiteSpace(RoomNumberTextBox.Text))
            {
                ShowError("Room number is required.");
                return false;
            }

            if (RoomNumberTextBox.Text.Length > 50)
            {
                ShowError("Room number cannot exceed 50 characters.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                ShowError("Room description is required.");
                return false;
            }

            if (DescriptionTextBox.Text.Length > 220)
            {
                ShowError("Room description cannot exceed 220 characters.");
                return false;
            }

            if (!int.TryParse(MaxCapacityTextBox.Text, out int maxCapacity) || maxCapacity <= 0)
            {
                ShowError("Max capacity must be a positive number.");
                return false;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                ShowError("Price must be a non-negative number.");
                return false;
            }

            if (RoomTypeComboBox.SelectedItem == null)
            {
                ShowError("Room type is required.");
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
