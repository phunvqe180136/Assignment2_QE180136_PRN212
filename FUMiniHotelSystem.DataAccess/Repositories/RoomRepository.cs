using FUMiniHotelSystem.DataAccess.Interfaces;
using FUMiniHotelSystem.DataAccess.Models;
using FUMiniHotelSystem.DataAccess.Services;
using System.Linq.Expressions;

namespace FUMiniHotelSystem.DataAccess.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private static RoomRepository? _instance;
        private static readonly object _lock = new object();
        private List<Room> _rooms;
        private readonly SqlDataService _sqlDataService;

        private RoomRepository()
        {
            _sqlDataService = new SqlDataService();
            _rooms = new List<Room>();
            LoadRooms();
        }

        public static RoomRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RoomRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        private async void LoadRoomsAsync()
        {
            _rooms = await _sqlDataService.LoadRoomsAsync();
        }

        private void LoadRooms()
        {
            _rooms = _sqlDataService.LoadRooms();
        }

        public List<Room> GetAll()
        {
            return _rooms.Where(r => r.RoomStatus == 1).ToList();
        }

        public Room? GetById(int id)
        {
            return _rooms.FirstOrDefault(r => r.RoomID == id && r.RoomStatus == 1);
        }

        public List<Room> Search(Expression<Func<Room, bool>> predicate)
        {
            return _rooms.Where(r => r.RoomStatus == 1).AsQueryable().Where(predicate).ToList();
        }

        public async void Add(Room room)
        {
            await _sqlDataService.SaveRoomAsync(room);
            _rooms.Add(room);
        }

        public async void Update(Room room)
        {
            await _sqlDataService.SaveRoomAsync(room);
            var existingRoom = _rooms.FirstOrDefault(r => r.RoomID == room.RoomID);
            if (existingRoom != null)
            {
                var index = _rooms.IndexOf(existingRoom);
                _rooms[index] = room;
            }
        }

        public async void Delete(int id)
        {
            await _sqlDataService.DeleteRoomAsync(id);
            var room = _rooms.FirstOrDefault(r => r.RoomID == id);
            if (room != null)
            {
                room.RoomStatus = 2; // Soft delete
            }
        }

        public bool Exists(int id)
        {
            return _rooms.Any(r => r.RoomID == id && r.RoomStatus == 1);
        }
    }
}
