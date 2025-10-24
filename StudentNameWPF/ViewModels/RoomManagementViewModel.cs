using FUMiniHotelSystem.Business.Services;
using FUMiniHotelSystem.DataAccess.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace StudentNameWPF.ViewModels
{
    public class RoomManagementViewModel : BaseViewModel
    {
        private readonly RoomService _roomService;
        private ObservableCollection<Room> _rooms;
        private ObservableCollection<RoomType> _roomTypes;
        private Room? _selectedRoom;
        private string _searchTerm = string.Empty;
        private string _statusMessage = string.Empty;
        private RoomType? _selectedRoomType;

        public RoomManagementViewModel()
        {
            _roomService = new RoomService();
            _rooms = new ObservableCollection<Room>();
            _roomTypes = new ObservableCollection<RoomType>();
            
            LoadData();
            
            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, () => SelectedRoom != null);
            DeleteCommand = new RelayCommand(ExecuteDelete, () => SelectedRoom != null);
            SearchCommand = new RelayCommand(ExecuteSearch);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            FilterByTypeCommand = new RelayCommand(ExecuteFilterByType);
        }

        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value);
        }

        public ObservableCollection<RoomType> RoomTypes
        {
            get => _roomTypes;
            set => SetProperty(ref _roomTypes, value);
        }

        public Room? SelectedRoom
        {
            get => _selectedRoom;
            set => SetProperty(ref _selectedRoom, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RoomType? SelectedRoomType
        {
            get => _selectedRoomType;
            set => SetProperty(ref _selectedRoomType, value);
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand FilterByTypeCommand { get; }

        private void LoadData()
        {
            try
            {
                var rooms = _roomService.GetAllRooms();
                var roomTypes = _roomService.GetAllRoomTypes();
                
                Rooms.Clear();
                foreach (var room in rooms)
                {
                    Rooms.Add(room);
                }
                
                RoomTypes.Clear();
                foreach (var roomType in roomTypes)
                {
                    RoomTypes.Add(roomType);
                }
                
                StatusMessage = $"Loaded {rooms.Count} rooms";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
            }
        }

        private void ExecuteAdd()
        {
            try
            {
                var dialog = new Views.RoomDialog();
                if (dialog.ShowDialog() == true)
                {
                    var room = dialog.Room;
                    var result = _roomService.AddRoom(room);
                    
                    if (result.IsSuccess)
                    {
                        // Reload room types to get the populated RoomType
                        var roomTypes = _roomService.GetAllRoomTypes();
                        room.RoomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeID == room.RoomTypeID);
                        Rooms.Add(room);
                        StatusMessage = result.Message;
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEdit()
        {
            if (SelectedRoom == null) return;

            try
            {
                var dialog = new Views.RoomDialog(SelectedRoom);
                if (dialog.ShowDialog() == true)
                {
                    var updatedRoom = dialog.Room;
                    var result = _roomService.UpdateRoom(updatedRoom);
                    
                    if (result.IsSuccess)
                    {
                        var index = Rooms.IndexOf(SelectedRoom);
                        Rooms[index] = updatedRoom;
                        StatusMessage = result.Message;
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedRoom == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete room '{SelectedRoom.RoomNumber}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var deleteResult = _roomService.DeleteRoom(SelectedRoom.RoomID);
                    
                    if (deleteResult.IsSuccess)
                    {
                        Rooms.Remove(SelectedRoom);
                        StatusMessage = deleteResult.Message;
                    }
                    else
                    {
                        MessageBox.Show(deleteResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteSearch()
        {
            try
            {
                var rooms = _roomService.SearchRooms(SearchTerm);
                Rooms.Clear();
                foreach (var room in rooms)
                {
                    Rooms.Add(room);
                }
                StatusMessage = $"Found {rooms.Count} rooms";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching rooms: {ex.Message}";
            }
        }

        private void ExecuteRefresh()
        {
            LoadData();
        }

        private void ExecuteFilterByType()
        {
            if (SelectedRoomType == null)
            {
                ExecuteRefresh();
                return;
            }

            try
            {
                var rooms = _roomService.GetRoomsByType(SelectedRoomType.RoomTypeID);
                Rooms.Clear();
                foreach (var room in rooms)
                {
                    Rooms.Add(room);
                }
                StatusMessage = $"Found {rooms.Count} rooms of type '{SelectedRoomType.RoomTypeName}'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error filtering rooms: {ex.Message}";
            }
        }
    }
}
